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

        CancellationTokenSource cts = new CancellationTokenSource();

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ServerActivity);

            ActionBar.SetDisplayHomeAsUpEnabled(true);

            qrContainerView = FindViewById<ImageView>(Resource.Id.QRContainer);
            qrContainerView.SetImageBitmap(null);

            await Server();
        }

        protected override void OnDestroy()
        {
            cts.Cancel();

            base.OnDestroy();
        }

        public async Task Server()
        {
            Network.LocalServer srv = await Network.LocalServer.CreateServerAsync();

            qrContainerView.SetImageBitmap(Features.QR.Create(srv.Address, Network.LocalServer.Port, Network.LocalServer.PublicConnectionPassword));

            Network.LocalServerConnection connection = await srv.WaitForConnectionAsync(cts.Token);
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