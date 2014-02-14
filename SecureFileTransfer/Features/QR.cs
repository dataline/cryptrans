using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;

namespace SecureFileTransfer.Features
{
    public static class QR
    {
        const string QRStringPrefix = "`?SecureFileTransfer::/";
        public static Android.Graphics.Bitmap Create(string hostName, int port, string connectionPassword)
        {
            StringBuilder qrString = new StringBuilder(QRStringPrefix);
            qrString.Append(hostName);
            qrString.Append("/");
            qrString.Append(port);
            qrString.Append("/");
            qrString.Append(connectionPassword);

            BitMatrix qrMatrix = new QRCodeWriter().encode(qrString.ToString(), ZXing.BarcodeFormat.QR_CODE, 800, 800);

            return new BitmapRenderer().Render(qrMatrix, ZXing.BarcodeFormat.QR_CODE, string.Empty);
        }

        public static bool IsValid(string qrString)
        {
            if (!qrString.StartsWith(QRStringPrefix))
                return false;

            int count = 0;
            foreach (char c in qrString)
            {
                if (c == '/')
                    count++;
            }

            return count == 3;
        }

        public static void GetComponents(string qrString, out string host, out int port, out string password)
        {
            string[] components = qrString.Split('/');

            host = components[1];
            port = Convert.ToInt32(components[2]);
            password = components[3];
        }
    }
}
