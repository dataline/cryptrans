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
    [Activity(Label = "@string/Connecting")]
    public class ClientActivity : Activity
    {
        public const string IE_CONNECTION_HOSTNAME = "IntentConnectionHostname";
        public const string IE_CONNECTION_PORT = "IntentConnectionPort";
        public const string IE_CONNECTION_PASSWORD = "IntentConnectionPassword";

        string HostName;
        int Port;
        string Password;

        Network.ClientConnection connection = null;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ClientActivity);

            HostName = Intent.GetStringExtra(IE_CONNECTION_HOSTNAME);
            Port = Intent.GetIntExtra(IE_CONNECTION_PORT, -1);
            Password = Intent.GetStringExtra(IE_CONNECTION_PASSWORD);

            if (HostName == null || Password == null || Port == -1)
            {
                Finish();
                return;
            }

            var progressDialog = new ProgressDialog(this)
            {
                Indeterminate = true
            };
            progressDialog.SetMessage(GetString(Resource.String.Connecting));
            progressDialog.Show();

            connection = await Network.ClientConnection.ConnectToAsync(HostName, Port, Password);

            progressDialog.Dismiss();

            ActionBar.Title = ".";
        }
    }
}