using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;

using SecureFileTransfer.Features;
using SecureFileTransfer.Features.Transfers;
using System.Threading.Tasks;
using SecureFileTransfer.Network;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden),
    IntentFilter(new string[] { Intent.ActionSend },
        Categories = new string[] { Intent.CategoryOpenable },
        DataMimeType = "*/*",
        Label = "@string/ApplicationName"),
    IntentFilter(new string[] { Intent.ActionSend },
        Categories = new string[] { Intent.CategoryDefault },
        DataMimeType = Android.Provider.ContactsContract.Contacts.ContentVcardType,
        Label = "@string/ApplicationName"),
    IntentFilter(new string[] { Intent.ActionView },
        Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = Features.QR.DataScheme)]
    public class ClientActivity : Activity
    {

        TransferQueue transfers = new TransferQueue();

        LinearLayout currentTransferLayout;
        TextView currentTransferTitleLabel;
        TextView currentTransferFileNameField;
        TextView currentTransferStatusField;
        ProgressBar currentTransferProgressBar;
        Button abortButton;

        bool statusReloadingHandlerEnabled = false;

        const int REQUEST_FILECHOOSER = 1;
        const int REQUEST_CONTACTCHOOSER = 2;

        public const string IE_HOST = "chost";
        public const string IE_PORT = "cport";
        public const string IE_PASSWORD = "cpass";

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ClientActivity);

            // Establish a connection from various sources:
            try
            {
                if (!await InitializeConnection())
                {
                    Finish();
                    return;
                }
            }
            catch (Exception ex)
            {
                ex.Handle(true);

                FinishWithFailedConnection(ex);
                return;
            }

            var connectedToLabel = FindViewById<TextView>(Resource.Id.ConnectedToField);
            connectedToLabel.Text = string.Format(GetString(Resource.String.ConnectedToFormatStr), Network.SenderConnection.CurrentConnection.RemoteName);
            
            var disconnectButton = FindViewById<Button>(Resource.Id.DisconnectButton);
            disconnectButton.Click += (s, e) =>
            {
                PushedDisconnect();
                Finish();
            };
            
            var sendPicturesButton = FindViewById<Button>(Resource.Id.SendPicturesButton);
            sendPicturesButton.Click += sendPicturesButton_Click;
            var sendContactsButton = FindViewById<Button>(Resource.Id.SendContactsButton);
            sendContactsButton.Click += sendContactsButton_Click;
            
            currentTransferLayout = FindViewById<LinearLayout>(Resource.Id.CurrentTransferLayout);
            currentTransferTitleLabel = FindViewById<TextView>(Resource.Id.CurrentTransferTitleLabel);
            currentTransferFileNameField = FindViewById<TextView>(Resource.Id.CurrentTransferFileNameField);
            currentTransferStatusField = FindViewById<TextView>(Resource.Id.CurrentTransferStatus);
            currentTransferProgressBar = FindViewById<ProgressBar>(Resource.Id.CurrentTransferProgressBar);
            
            abortButton = FindViewById<Button>(Resource.Id.AbortButton);
            abortButton.Click += abortButton_Click;
            
            // Handle file transfer intent:
            var uri = (Android.Net.Uri)Intent.GetParcelableExtra(Intent.ExtraStream);
            if (uri != null)
                HandleIntentUri(uri, Intent.Type);
        }

        async Task<bool> InitializeConnection()
        {
            if (Network.SenderConnection.CurrentConnection == null &&
                Intent.Scheme == Features.QR.DataScheme && Intent.Data != null)
            {
                // Intent from a barcode scanner or something similar
                await SenderConnectionEstablisher.EstablishSenderConnection(this, Intent.Data);
            }

            if (Network.SenderConnection.CurrentConnection == null)
            {
                var host = Intent.GetStringExtra(IE_HOST);
                if (host != null)
                {
                    // Intent from a source that specifically defines host, port and password
                    var port = Intent.GetIntExtra(IE_PORT, 0);
                    var password = Intent.GetStringExtra(IE_PASSWORD);
                    if (port != 0 && password != null)
                    {
                        await SenderConnectionEstablisher.EstablishSenderConnection(this, host, port, password);
                    }
                }
            }

            // If no connection was established, use CCE to read QR code
            if (Network.SenderConnection.CurrentConnection == null && !await SenderConnectionEstablisher.EstablishSenderConnection(this))
                return false;

            Network.SenderConnection.CurrentConnection.UIThreadSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            Network.SenderConnection.CurrentConnection.Disconnected += CurrentConnection_Disconnected;
            Network.SenderConnection.CurrentConnection.BeginReceiving();

            transfers.Connection = Network.SenderConnection.CurrentConnection;
            transfers.FileTransferFailed += transfers_FileTransferFailed;

            return true;
        }

        void transfers_FileTransferFailed(Transfer transfer)
        {
            this.ShowToast(string.Format(GetString(Resource.String.ErrTransferFailedFormatStr), transfer.FileName));
        }

        void FinishWithFailedConnection(Exception ex)
        {
            ex.Handle(true);

            if (ex is InvalidQRCodeException)
            {
                new Dialogs.InvalidQRCodeDialog(this, result => Finish()).Show("invalidqr");
            }
            else if (ex is InvalidHandshakeException)
            {
                new Dialogs.InvalidHandshakeDialog(this, (InvalidHandshakeException)ex, result => Finish()).Show("invalidhandshake");
            }
            else
            {
                new Dialogs.ConnectionFailedDialog(this, result =>
                {
                    if (result == Dialogs.AndroidDialog.AndroidDialogResult.No)
                    {
                        // Tapped on "Open hotspot settings"
                        var intent = new Intent(Android.Provider.Settings.ActionWirelessSettings);
                        StartActivity(intent);
                    }

                    Finish();
                }).Show("connectionfailed");
            }
        }

        void abortButton_Click(object sender, EventArgs e)
        {
            transfers.Abort();
        }

        void StartReloadingCurrentTransferView(Handler handler = null, int invokeCount = 1)
        {
            if (statusReloadingHandlerEnabled && handler == null)
                return;

            statusReloadingHandlerEnabled = true;

            if (handler == null)
                handler = new Handler();

            if (transfers.CurrentTransfer == null)
            {
                currentTransferLayout.Visibility = ViewStates.Invisible;

                statusReloadingHandlerEnabled = false;
            }
            else
            {
                var dataConnection = Network.SenderConnection.CurrentConnection.DataConnection;

                if (invokeCount % 5 == 0 && dataConnection != null)
                    dataConnection.ReloadBytesPerSecond();

                currentTransferTitleLabel.Text = string.Format(GetString(Resource.String.CurrentFileTransferRemainingFormatStr), transfers.Remaining);
                currentTransferFileNameField.Text = transfers.CurrentTransfer.FileName;
                currentTransferStatusField.Text = dataConnection == null ?
                    GetString(Resource.String.Preparing) :
                    dataConnection.BytesPerSecond.HumanReadableSizePerSecond();
                currentTransferProgressBar.Progress = dataConnection == null ? 0 : dataConnection.Progress;

                abortButton.SetText(transfers.HasQueuedTransfers ? Resource.String.AbortAll : Resource.String.Abort);

                currentTransferLayout.Visibility = ViewStates.Visible;

                handler.PostDelayed(() => StartReloadingCurrentTransferView(handler, invokeCount + 1), 200);
            }
        }

        void DoTransfer(Transfer trans)
        {
            trans.Context = this;
            transfers.Enqueue(trans);

            StartReloadingCurrentTransferView();
        }

        void sendPicturesButton_Click(object sender, EventArgs e)
        {
            var fileChooser = new Intent();
            fileChooser.SetType("*/*");
            fileChooser.AddCategory(Intent.CategoryOpenable);
            fileChooser.SetAction(Intent.ActionGetContent);

            StartActivityForResult(Intent.CreateChooser(fileChooser, GetString(Resource.String.ClientSendFile)), REQUEST_FILECHOOSER);
        }

        void HandleIntentUri(Android.Net.Uri uri, string type)
        {
            if (type == Android.Provider.ContactsContract.Contacts.ContentVcardType)
            {
                var lookupKey = uri.LastPathSegment;
                var contactID = ContactProvider.GetContactIDFromLookupKey(this, lookupKey);

                HandleShareContacts(new string[] { contactID });
            }
            else
            {
                HandleShareFile(uri);
            }
        }

        void HandleShareContacts(string[] ids)
        {
            foreach (var id in ids)
            {
                DoTransfer(new ContactTransfer()
                {
                    Context = this,
                    ContactId = id
                });
            }
        }

        void HandleShareFile(Android.Net.Uri uri)
        {
            long fileSize;
            string fileName;

            uri.GetMetadataFromContentURI(ContentResolver, out fileSize, out fileName);

            DoTransfer(new ExistingFileTransfer()
            {
                FileStream = uri.GetInputStreamFromContentURI(ContentResolver),
                FileLength = fileSize,
                FileName = fileName
            });
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
                return;

            if (requestCode == REQUEST_FILECHOOSER)
            {
                HandleShareFile(data.Data);
            }
            else if (requestCode == REQUEST_CONTACTCHOOSER)
            {
                var contactIds = data.GetStringArrayExtra(ContactListActivity.IE_RESULT_SELECTED_CONTACT_IDS);

                HandleShareContacts(contactIds);
            }
        }

        void sendContactsButton_Click(object sender, EventArgs e)
        {
            StartActivityForResult(typeof(ContactListActivity), REQUEST_CONTACTCHOOSER);
        }

        void CurrentConnection_Disconnected()
        {
            transfers.Abort(false);
            Disconnect();

            Finish();
        }

        void PushedDisconnect()
        {
            try
            {
                transfers.Abort();
            }
            catch (Exception)
            {
                // Unimportant because we are disconnecting anyways.
            }

            Disconnect();
        }

        public override void OnBackPressed()
        {
            PushedDisconnect();
            base.OnBackPressed();
        }

        public void Disconnect()
        {
            if (Network.SenderConnection.CurrentConnection != null)
                Network.SenderConnection.CurrentConnection.Dispose();
        }
    }
}