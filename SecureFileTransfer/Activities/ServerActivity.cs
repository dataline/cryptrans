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
using System.Threading.Tasks;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "@string/ApplicationName")]
    public class ServerActivity : Activity
    {
        ImageView qrContainerView;

        Network.LocalServer srv = null;

        CancellationTokenSource cts = new CancellationTokenSource();

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ServerActivity);

            ActionBar.SetDisplayHomeAsUpEnabled(true);

            var connectManuallyButton = FindViewById<Button>(Resource.Id.ConnectManuallyButton);
            connectManuallyButton.Click += (s, e) =>
            {
                if (srv == null)
                    return;

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                AlertDialog info = builder.Create();

                info.SetTitle(Resource.String.ServerConnectManually);
                info.SetMessage(string.Format(GetString(Resource.String.ServerConnectManuallyInfoFormatStr), srv.Address, Network.LocalServer.PublicConnectionPassword));
                info.Show();
            };

            qrContainerView = FindViewById<ImageView>(Resource.Id.QRContainer);
            qrContainerView.SetImageBitmap(null);

            await Server();
        }

        protected override void OnDestroy()
        {
            cts.Cancel();

            base.OnDestroy();
        }

        //public override void OnBackPressed()
        //{
        //    base.OnBackPressed();
        //}

        public async Task Server()
        {
            srv = await Network.LocalServer.CreateServerAsync();

            qrContainerView.SetImageBitmap(Features.QR.Create(srv.Address, Network.LocalServer.Port, Network.LocalServer.PublicConnectionPassword));

            Network.LocalServerConnection connection = null;

            while (connection == null)
            {
                try
                {
                    connection = await srv.WaitForConnectionAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            srv.Dispose();

            if (connection == null)
                return; //aborted.

            Console.WriteLine("Test.");
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}