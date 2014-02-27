using Android.Net.Wifi.P2p;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.WiFiDirect
{
    public class WifiActionListener : Java.Lang.Object, Android.Net.Wifi.P2p.WifiP2pManager.IActionListener
    {
        Action<WifiP2pFailureReason> failure;
        Action success;

        public WifiActionListener(Action successAction, Action<WifiP2pFailureReason> failureAction)
        {
            failure = failureAction;
            success = successAction;
        }

        public void OnSuccess()
        {
            if(success != null)
                success();
        }

        public void OnFailure(WifiP2pFailureReason reason)
        {
            if (failure != null)
                failure(reason);
        }
    }
}
