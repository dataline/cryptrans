using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.Threading;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "@string/ApplicationName", 
        MainLauncher = true, 
        Icon = "@drawable/icon",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class HomeActivity : Activity
    {
        ImageView qrContainerView;

        System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
        Task<Network.LocalServer> getServerTask;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            try
            {
                SetContentView(Resource.Layout.Main);
            }
            catch (InflateException ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }

            qrContainerView = FindViewById<ImageView>(Resource.Id.QRContainerView);

            FindViewById<Button>(Resource.Id.ScanCodeButton).Click += (s, e) => StartActivity(typeof(ClientActivity));
            FindViewById<Button>(Resource.Id.ConnectManuallyButton).Click += (s, e) =>
            {
                var srv = Network.LocalServer.CurrentServer;

                if (srv == null)
                {
                    Toast.MakeText(this, Resource.String.PleaseWaitForServerToInitialize, ToastLength.Long)
                        .Show();
                }
                else
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                    AlertDialog info = builder.Create();

                    info.SetTitle(Resource.String.ServerConnectManually);
                    info.SetMessage(string.Format(GetString(Resource.String.ServerConnectManuallyInfoFormatStr), srv.Address, Network.LocalServer.PublicConnectionPassword));
                    info.Show();
                }
            };

            // Start generation of RSA keys in background (could take up to 10 seconds, we do not want the user to wait too long)
            Security.KeyProvider.StartKeyGeneration();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            await EstablishServer();
        }

        protected override void OnStop()
        {
            DestroyServer();

            base.OnStop();
        }

        public async Task EstablishServer()
        {
            getServerTask = Network.LocalServer.GetServerAsync(cts.Token);
            var srv = await getServerTask;
            getServerTask = null;

            if (srv == null)
                return;

            srv.UIThreadSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            srv.GotConnection += srv_GotConnection;

            qrContainerView.SetImageBitmap(Features.QR.Create(srv.Address, Network.LocalServer.Port, Network.LocalServer.PublicConnectionPassword));
        }

        public void DestroyServer()
        {
            if (getServerTask != null)
                cts.Cancel();

            if (Network.LocalServer.CurrentServer != null)
                Network.LocalServer.CurrentServer.Dispose();

            qrContainerView.SetImageBitmap(null);
        }

        void srv_GotConnection(Network.LocalServerConnection connection)
        {
            DestroyServer();

            StartActivity(typeof(ServerConnectedActivity));
        }
    }
}

