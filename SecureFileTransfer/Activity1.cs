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
        int count = 1;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            Security.KeyProvider.StartKeyGeneration();
            //Security.AES aes = await Security.KeyProvider.GetAESAsync();
            //string debugvalues = "Key:";
            //for (int i = 0; i < aes.aesKey.Length; i++)
            //{
            //    debugvalues += " " + aes.aesKey[i].ToString();
            //}
            //AlertDialog.Builder builder = new AlertDialog.Builder(this);
            //AlertDialog dialog = builder.Create();
            //dialog.SetTitle("Info");
            //dialog.SetMessage(debugvalues);
            //dialog.Show();

            if (Network.LocalServer.Instance == null)
                await Network.LocalServer.WaitForConnection();
        }
    }
}

