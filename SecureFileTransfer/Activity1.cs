﻿using System;

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
            EditText connPass = FindViewById<EditText>(Resource.Id.ConnectionPasswordField);
            TextView publicConnPass = FindViewById<TextView>(Resource.Id.PublicConnectionPasswordField);

            ipField.Text = "192.168.0.139";
            publicConnPass.Text = Network.LocalServer.PublicConnectionPassword;


            testServer.Click += async (s, e) =>
            {
                Network.LocalServerConnection connection = await Network.LocalServer.WaitForConnectionAsync();

                AlertDialog.Builder b = new AlertDialog.Builder(this);
                AlertDialog a = b.Create();
                a.SetTitle("Info");
                a.SetMessage("Established connection to client.");
                a.Show();

            };
            testClient.Click += async (s, e) =>
            {
                Network.ClientConnection connection = await Network.ClientConnection.ConnectToAsync(ipField.Text, Network.LocalServer.Port, connPass.Text);

                AlertDialog.Builder b = new AlertDialog.Builder(this);
                AlertDialog a = b.Create();
                a.SetTitle("Info");
                a.SetMessage("Established connection to server.");
                a.Show();
            };

            Security.KeyProvider.StartKeyGeneration();
        }
    }
}

