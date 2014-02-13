using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SecureFileTransfer
{
    [Activity(Label = "SecureFileTransfer", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Button testServer = FindViewById<Button>(Resource.Id.TestServer);
            Button testClient = FindViewById<Button>(Resource.Id.TestClient);
            EditText ipField = FindViewById<EditText>(Resource.Id.IPField);

            ipField.Text = "192.168.0.139";

            testServer.Click += (s, e) =>
            {
                Network.LocalServerConnection connection = Network.LocalServer.WaitForConnectionAsync().Result;
            };
            testClient.Click += (s, e) =>
            {
                Network.ClientConnection connection = Network.ClientConnection.ConnectTo(ipField.Text, Network.LocalServer.Port);
            };

            Security.KeyProvider.StartKeyGeneration();
        }
    }
}

