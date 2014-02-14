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

        void CancelServer()
        {
            cts.Cancel();

            if (Network.LocalServer.CurrentServer != null)
                Network.LocalServer.CurrentServer.Dispose();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnBackPressed()
        {
            CancelServer();

            base.OnBackPressed();
        }

        public async Task Server()
        {
            srv = await Network.LocalServer.GetServerAsync();

            qrContainerView.SetImageBitmap(Features.QR.Create(srv.Address, Network.LocalServer.Port, Network.LocalServer.PublicConnectionPassword));

            srv.GotConnection += srv_GotConnection;
        }

        void srv_GotConnection(Network.LocalServerConnection connection)
        {
            Network.LocalServer.CurrentServer.Dispose();

            StartActivity(typeof(ServerConnectedActivity));
            Finish();
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