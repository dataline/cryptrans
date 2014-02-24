using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureFileTransfer.Features
{
    public static class ClientConnectionEstablisher
    {
        public static async Task<bool> EstablishClientConnection(Context ctx)
        {
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new System.Collections.Generic.List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE }
            };
            var scanner = new ZXing.Mobile.MobileBarcodeScanner(ctx);
            var result = await scanner.Scan(options);

            if (result == null)
                return false;

            string resString = result.Text;
            if (!Features.QR.IsValid(resString))
            {
                ShowInvalidCodeAlert(ctx);
                return false;
            }

            string host, password;
            int port;
            Features.QR.GetComponents(resString, out host, out port, out password);

            var progressDialog = new ProgressDialog(ctx)
            {
                Indeterminate = true
            };
            progressDialog.SetMessage(ctx.GetString(Resource.String.Connecting));
            progressDialog.Show();

            var connection = await Network.ClientConnection.ConnectToAsync(host, port, password);

            progressDialog.Dismiss();

            Console.WriteLine("Starting new client for " + resString);

            return true;
        }

        static void ShowInvalidCodeAlert(Context ctx)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(ctx);
            AlertDialog alert = builder.Create();
            alert.SetTitle(Resource.String.QRCodeInvalidTitle);
            alert.SetMessage(ctx.GetString(Resource.String.QRCodeInvalid));
            alert.Show();
        }
    }
}
