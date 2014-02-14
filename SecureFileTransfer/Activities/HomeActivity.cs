using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;

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
            sendFilesButton.Click += async (s, e) => await TestClient();

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

        async Task TestClient()
        {
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new System.Collections.Generic.List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE }
            };
            var scanner = new ZXing.Mobile.MobileBarcodeScanner(this);
            var result = await scanner.Scan(options);

            if (result == null)
                return;

            string resString = result.Text;
            if (!Features.QR.IsValid(resString))
            {
                ShowInvalidCodeAlert();
                return;
            }

            string host, password;
            int port;
            Features.QR.GetComponents(resString, out host, out port, out password); 
            
            var progressDialog = new ProgressDialog(this)
            {
                Indeterminate = true
            };
            progressDialog.SetMessage(GetString(Resource.String.Connecting));
            progressDialog.Show();

            var connection = await Network.ClientConnection.ConnectToAsync(host, port, password);

            progressDialog.Dismiss();

            if (connection != null)
                StartActivity(typeof(ClientActivity));
        }

        void ShowInvalidCodeAlert()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            AlertDialog alert = builder.Create();
            alert.SetTitle(Resource.String.QRCodeInvalidTitle);
            alert.SetMessage(GetString(Resource.String.QRCodeInvalid));
            alert.Show();
        }
    }
}

