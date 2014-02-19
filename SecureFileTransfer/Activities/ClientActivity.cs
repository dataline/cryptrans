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

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "")]
    public class ClientActivity : Activity
    {

        TransferQueue transfers = new TransferQueue();

        LinearLayout currentTransferLayout;
        TextView currentTransferFileNameField;
        TextView currentTransferStatusField;
        ProgressBar currentTransferProgressBar;

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
                Disconnect();
                Finish();
            };

            var sendPicturesButton = FindViewById<Button>(Resource.Id.SendPicturesButton);
            sendPicturesButton.Click += sendPicturesButton_Click;
            var sendOtherButton = FindViewById<Button>(Resource.Id.SendOtherButton);
            sendOtherButton.Click += sendOtherButton_Click;

            currentTransferLayout = FindViewById<LinearLayout>(Resource.Id.CurrentTransferLayout);
            currentTransferFileNameField = FindViewById<TextView>(Resource.Id.CurrentTransferFileNameField);
            currentTransferStatusField = FindViewById<TextView>(Resource.Id.CurrentTransferStatus);
            currentTransferProgressBar = FindViewById<ProgressBar>(Resource.Id.CurrentTransferProgressBar);

            Network.ClientConnection.CurrentConnection.UIThreadSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            Network.ClientConnection.CurrentConnection.Disconnected += CurrentConnection_Disconnected;
            Network.ClientConnection.CurrentConnection.BeginReceiving();

            transfers.Connection = Network.ClientConnection.CurrentConnection;
        }

        void StartReloadingCurrentTransferView(Handler handler = null)
        {
            if (handler == null)
                handler = new Handler();

            if (transfers.CurrentTransfer == null)
            {
                currentTransferLayout.Visibility = ViewStates.Invisible;
            }
            else
            {
                currentTransferFileNameField.Text = transfers.CurrentTransfer.FileName;
                currentTransferStatusField.Text = "Transferring...";
                currentTransferProgressBar.Progress = Network.ClientConnection.CurrentConnection.DataConnection == null
                    ? 0 : Network.ClientConnection.CurrentConnection.DataConnection.Progress;

                currentTransferLayout.Visibility = ViewStates.Visible;

                handler.PostDelayed(() => StartReloadingCurrentTransferView(handler), 200);
            }
        }

        #region Image Chooser

        const int REQUEST_IMAGECHOOSER = 1;

        void sendPicturesButton_Click(object sender, EventArgs e)
        {
            var imageChooser = new Intent();
            imageChooser.SetType("image/*");
            imageChooser.SetAction(Intent.ActionGetContent);

            StartActivityForResult(Intent.CreateChooser(imageChooser, GetString(Resource.String.SelectPictures)), REQUEST_IMAGECHOOSER);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_IMAGECHOOSER && resultCode == Result.Ok)
            {
                long fileSize;
                string fileName;

                data.Data.GetMetadataFromContentURI(ContentResolver, out fileSize, out fileName);

                transfers.Enqueue(new ExistingFileTransfer()
                {
                    FileStream = data.Data.GetInputStreamFromContentURI(ContentResolver),
                    FileLength = fileSize,
                    FileName = fileName
                });
            }
        }
        #endregion

        void sendOtherButton_Click(object sender, EventArgs e)
        {
            //Network.ClientConnection.CurrentConnection.FileTransferTest();
        }

        void CurrentConnection_Disconnected()
        {
            Disconnect();

            Finish();
        }

        public override void OnBackPressed()
        {
            Disconnect();

            base.OnBackPressed();
        }

        public void Disconnect()
        {
            if (Network.ClientConnection.CurrentConnection != null)
                Network.ClientConnection.CurrentConnection.Dispose();
        }
    }
}