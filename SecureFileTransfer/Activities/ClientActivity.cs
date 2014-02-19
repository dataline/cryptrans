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

            Network.ClientConnection.CurrentConnection.UIThreadSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            Network.ClientConnection.CurrentConnection.Disconnected += CurrentConnection_Disconnected;
            Network.ClientConnection.CurrentConnection.BeginReceiving();
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

                Network.ClientConnection.CurrentConnection.StartFileTransfer(data.Data.GetInputStreamFromContentURI(ContentResolver), fileSize, fileName);
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