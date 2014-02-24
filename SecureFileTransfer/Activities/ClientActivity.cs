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
    [Activity(Label = "")]
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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ClientActivity);

            if (Network.ClientConnection.CurrentConnection == null)
            {
                Finish();
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
                return;

            if (requestCode == REQUEST_FILECHOOSER)
            {
                long fileSize;
                string fileName;

                data.Data.GetMetadataFromContentURI(ContentResolver, out fileSize, out fileName);

                DoTransfer(new ExistingFileTransfer()
                {
                    FileStream = data.Data.GetInputStreamFromContentURI(ContentResolver),
                    FileLength = fileSize,
                    FileName = fileName
                });
            }
            else if (requestCode == REQUEST_CONTACTCHOOSER)
            {
                long[] contactIds = data.GetLongArrayExtra(ContactListActivity.IE_RESULT_SELECTED_CONTACT_IDS);

                foreach (var contactId in contactIds)
                {
                    DoTransfer(new ContactTransfer()
                    {
                        Context = this,
                        ContactId = contactId.ToString()
                    });
                }

                //List<Tuple<Android.Net.Uri, string>> vcfUris = ContactProvider.GetVcardUrisFromContactIds(this, contactIds).ToList();
                //
                //foreach (var vcf in vcfUris)
                //{
                //    DoTransfer(new VCardTransfer()
                //    {
                //        VcardStream = vcf.Item1.GetInputStreamFromContentURI(ContentResolver),
                //        FileName = vcf.Item2
                //    });
                //}
            }
        }

        void sendOtherButton_Click(object sender, EventArgs e)
        {
            //Network.ClientConnection.CurrentConnection.FileTransferTest();
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