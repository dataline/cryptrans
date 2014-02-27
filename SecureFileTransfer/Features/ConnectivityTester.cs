using Android.Content;
using Android.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public static class ConnectivityTester
    {
        public static bool HasWifi(Context ctx)
        {
            var con = (ConnectivityManager)ctx.GetSystemService(Context.ConnectivityService);
            var wifi = con.GetNetworkInfo(ConnectivityType.Wifi);

            return wifi != null && wifi.GetState() == NetworkInfo.State.Connected;
        }
    }
}
