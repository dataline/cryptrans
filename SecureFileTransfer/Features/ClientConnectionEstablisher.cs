﻿using Android.App;
using Android.Content;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureFileTransfer.Features
{
    public static class ClientConnectionEstablisher
    {
        /// <summary>
        /// Create connection from host, port and password.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<bool> EstablishClientConnection(Activity ctx, string host, int port, string password)
        {
            var connectingDialog = new Dialogs.ConnectingDialog(ctx);
            connectingDialog.Show("connecting");

            var connection = await Network.ClientConnection.ConnectToAsync(host, port, password);

            connectingDialog.Dismiss();

            return true;
        }

        /// <summary>
        /// Create connection by presenting in-app QR code scanner.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static async Task<bool> EstablishClientConnection(Activity ctx)
        {
            var overlay = ctx.LayoutInflater.Inflate(Resource.Layout.ScannerOverlay, null);

            var options = new ZXing.Mobile.MobileBarcodeScanningOptions()
            {
                PossibleFormats = new System.Collections.Generic.List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.QR_CODE }
            };
            var scanner = new ZXing.Mobile.MobileBarcodeScanner(ctx)
            {
                UseCustomOverlay = true,
                CustomOverlay = overlay
            };
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

            return await EstablishClientConnection(ctx, host, port, password);
        }

        /// <summary>
        /// Create connection from external app (like a QR code scanner)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static async Task<bool> EstablishClientConnection(Activity ctx, Android.Net.Uri uri)
        {
            string host, password;
            int port;
            if (!Features.QR.GetComponents(uri, out host, out port, out password))
                return false;

            return await EstablishClientConnection(ctx, host, port, password);
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
