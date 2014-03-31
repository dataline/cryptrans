using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.Threading;

using SecureFileTransfer.Features;
using Android.Provider;
using SecureFileTransfer.Network;
using Android.Preferences;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "@string/ApplicationName", 
        MainLauncher = true,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class HomeActivity : Activity
    {
        ImageView qrContainerView;

        System.Threading.CancellationTokenSource cts;
        Task<Network.LocalServer> getServerTask;

        Dialogs.AndroidDialog currentDialog = null;

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

            var scanCodeButton = FindViewById<Button>(Resource.Id.ScanCodeButton);

            scanCodeButton.Click += (s, e) => StartActivity(typeof(ClientActivity));

            // Connect manually dialog (client side):
            scanCodeButton.LongClick += (s, e) => 
                new Dialogs.ConnectManuallyDialog(
                    this,
                    Resource.String.OK,
                    Resource.String.Cancel, 
                    (res, ip, pass) =>
                {
                    if (res == Dialogs.AndroidDialog.AndroidDialogResult.Yes)
                    {
                        if (ip.Length == 0 || pass.Length == 0)
                            return;

                        Intent clientIntent = new Intent(this, typeof(ClientActivity));
                        clientIntent.PutExtra(ClientActivity.IE_HOST, ip)
                            .PutExtra(ClientActivity.IE_PORT, Network.LocalServer.Port)
                            .PutExtra(ClientActivity.IE_PASSWORD, pass);

                        StartActivity(clientIntent);
                    }
                }).Show("connectmanually");

            // Connect manually information (server side):
            FindViewById<Button>(Resource.Id.ConnectManuallyButton).Click += (s, e) =>
            {
                var srv = Network.LocalServer.CurrentServer;

                if (srv == null)
                {
                    this.ShowToast(Resource.String.PleaseWaitForServerToInitialize);
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

            // First start?
            if (!Features.Preferences.GetBool(
                    PreferenceManager.GetDefaultSharedPreferences(this),
                    Features.Preferences.PrefInfoActivityShown))
            {
                StartActivity(typeof(WelcomeActivity));
                Finish();
            }
        }

        protected override async void OnStart()
        {
            base.OnStart();

            try
            {
                await EstablishServer();
            }
            catch (Exception)
            {
                this.ShowToast(Resource.String.ErrEstablishServer);
            }
        }

        protected override void OnStop()
        {
            DestroyServer();

            if (currentDialog != null)
            {
                currentDialog.Dismiss();
                currentDialog = null;
            }

            base.OnStop();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.HomeMenu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.BrowseReceivedFiles)
            {
                StartActivity(typeof(FileBrowserActivity));
                return true;
            }
            else if (item.ItemId == Resource.Id.About)
            {
                StartActivity(typeof(AboutActivity));
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public async Task EstablishServer()
        {
            if (!CheckWifi())
                return;

            cts = new CancellationTokenSource();
            getServerTask = Network.LocalServer.GetServerAsync(cts.Token);
            var srv = await getServerTask;
            getServerTask = null;

            if (srv == null)
                return;

            srv.UIThreadSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            srv.GotConnection += srv_GotConnection;
            srv.FailedConnectionAttempt += srv_FailedConnectionAttempt;

            qrContainerView.SetImageBitmap(Features.QR.Create(srv.Address, Network.LocalServer.Port, Network.LocalServer.PublicConnectionPassword));
        }

        public void DestroyServer()
        {
            if (getServerTask != null && cts != null)
                cts.Cancel();

            try
            {
                if (Network.LocalServer.CurrentServer != null)
                    Network.LocalServer.CurrentServer.Dispose();
            }
            catch (Exception ex)
            {
                this.HandleEx(ex);
            }

            qrContainerView.SetImageBitmap(null);
        }

        void srv_GotConnection(Network.LocalServerConnection connection)
        {
            DestroyServer();

            StartActivity(typeof(ServerConnectedActivity));
        }

        void srv_FailedConnectionAttempt(Exception ex)
        {
            var errorDesc = ex is InvalidHandshakeException ? 
                ((InvalidHandshakeException)ex).GetDescriptionResource() : 
                Resource.String.ErrUnexpected;

            var errorStr = string.Format(GetString(Resource.String.ErrInvalidClientFormatStr), GetString(errorDesc));

            this.ShowToast(errorStr);
        }

        bool CheckWifi()
        {
            if (!Features.ConnectivityTester.HasWifi(this))
            {
                currentDialog = new Dialogs.NoWifiDialog(this);
                currentDialog.ShowUntil(() => Features.ConnectivityTester.HasWifi(this),
                async (res) =>
                { 
                    // Tapped on Retry
                    if (res)
                        await EstablishServer();
                },
                (res) =>
                {
                    // Tapped on "Open WLAN settings"
                    var wifiIntent = new Intent(Settings.ActionWifiSettings);
                    StartActivity(wifiIntent);
                },
                () => 
                {
                    currentDialog = null;
                }, false, "nowifi");

                return false;
            }

            return true;
        }
    }
}

