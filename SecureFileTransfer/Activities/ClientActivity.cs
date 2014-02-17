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

            Network.ClientConnection.CurrentConnection.Disconnected += CurrentConnection_Disconnected;
            Network.ClientConnection.CurrentConnection.BeginReceiving();
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
            Network.ClientConnection.CurrentConnection.Dispose();
        }
    }
}