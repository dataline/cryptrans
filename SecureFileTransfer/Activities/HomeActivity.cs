using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Icon = "@drawable/icon")]
    public class HomeActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Button receiveFilesButton = FindViewById<Button>(Resource.Id.ReceiveFilesButton);
            Button sendFilesButton = FindViewById<Button>(Resource.Id.SendFilesButton);

            receiveFilesButton.Click += (s, e) => StartActivity(typeof(ServerActivity));


            //testServer.Click += async (s, e) =>
            //{
            //    Network.LocalServerConnection connection = await Network.LocalServer.WaitForConnectionAsync();
            //
            //    AlertDialog.Builder b = new AlertDialog.Builder(this);
            //    AlertDialog a = b.Create();
            //    a.SetTitle("Info");
            //    a.SetMessage("Established connection to client.");
            //    a.Show();
            //
            //};
            //testClient.Click += async (s, e) =>
            //{
            //    Network.ClientConnection connection = await Network.ClientConnection.ConnectToAsync(ipField.Text, Network.LocalServer.Port, connPass.Text);
            //
            //    AlertDialog.Builder b = new AlertDialog.Builder(this);
            //    AlertDialog a = b.Create();
            //    a.SetTitle("Info");
            //    a.SetMessage("Established connection to server.");
            //    a.Show();
            //};

            Security.KeyProvider.StartKeyGeneration();
        }
    }
}

