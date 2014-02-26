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

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ClientActivity);

            if (Intent.Scheme == Features.QR.DataScheme)
            {
                // Intent from a barcode scanner or something similar
                await ClientConnectionEstablisher.EstablishClientConnection(this, Intent.Data);
            }

            if (Network.ClientConnection.CurrentConnection == null && !(await ClientConnectionEstablisher.EstablishClientConnection(this)))
            {
                Finish();
                return;
            }

            var connectedToLabel = FindViewById<TextView>(Resource.Id.ConnectedToField);
            connectedToLabel.Text = string.Format(GetString(Resource.String.ConnectedToFormatStr), Network.ClientConnection.CurrentConnection.RemoteName);

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

            Network.ClientConnection.CurrentConnection.UIThreadSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            Network.ClientConnection.CurrentConnection.Disconnected += CurrentConnection_Disconnected;
            Network.ClientConnection.CurrentConnection.BeginReceiving();

            transfers.Connection = Network.ClientConnection.CurrentConnection;

            // Handle file transfer intent:
            var uri = (Android.Net.Uri)Intent.GetParcelableExtra(Intent.ExtraStream);
            if (uri != null)
                HandleIntentUri(uri, Intent.Type);
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
                var dataConnection = Network.ClientConnection.CurrentConnection.DataConnection;

                if (invokeCount % 5 == 0 && dataConnection != null)
                    dataConnection.ReloadBytesPerSecond();

                currentTransferTitleLabel.Text = string.Format(GetString(Resource.String.CurrentFileTransferRemainingFormatStr), transfers.Remaining);
                currentTransferFileNameField.Text = transfers.CurrentTransfer.FileName;
                currentTransferStatusField.Text = dataConnection == null ? "Preparing..." : dataConnection.BytesPerSecond.HumanReadableSizePerSecond();
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
            transfers.Abort();

            Disconnect();
        }

        public override void OnBackPressed()
        {
            PushedDisconnect();
            base.OnBackPressed();
        }

        public void Disconnect()
        {
            if (Network.ClientConnection.CurrentConnection != null)
                Network.ClientConnection.CurrentConnection.Dispose();
        }
    }
}