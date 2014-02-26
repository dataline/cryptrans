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
        public const string DataScheme = "dltsec";
        public const string DataSchemeWithSuffix = DataScheme + "://";

        public static Android.Graphics.Bitmap Create(string hostName, int port, string connectionPassword)
        {
            StringBuilder qrString = new StringBuilder(DataSchemeWithSuffix);
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
            if (!qrString.StartsWith(DataSchemeWithSuffix))
                return false;

            int count = 0;
            foreach (char c in qrString)
            {
                if (c == '/')
                    count++;
            }

            return count == 4;
        }

        public static void GetComponents(string qrString, out string host, out int port, out string password)
        {
            string[] components = qrString.Split('/');

            host = components[2];
            port = Convert.ToInt32(components[3]);
            password = components[4];
        }

        public static bool GetComponents(Android.Net.Uri uri, out string host, out int port, out string password)
        {
            host = "";
            port = 0;
            password = "";

            var pars = uri.PathSegments;
            if (pars.Count != 2)
                return false;

            host = uri.Host;
            port = Convert.ToInt32(pars[0]);
            password = pars[1];

            return true;
        }
    }
}
