#if WIFIDIRECT

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
using Android.Net.Wifi.P2p;

namespace SecureFileTransfer.Network.WiFiDirect
{
   // [BroadcastReceiver]
    public class WiFiDirectBroadcastReceiver : BroadcastReceiver
    {
        WifiP2pManager manager;
        WifiP2pManager.Channel channel;

        public WiFiDirectBroadcastReceiver(WifiP2pManager manager, WifiP2pManager.Channel channel)
        {
            this.manager = manager;
            this.channel = channel;
        }
        
        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;

            switch (action)
            {
                case WifiP2pManager.WifiP2pStateChangedAction:
                    WifiP2pState state = (WifiP2pState)intent.GetIntExtra(WifiP2pManager.ExtraWifiState, -1);
                    P2PManager.Available = state == WifiP2pState.Enabled;
                    break;
                case WifiP2pManager.WifiP2pPeersChangedAction:
                    manager.RequestPeers(channel, new PeerListListener(peerList =>
                    {
                        P2PManager.peerList = peerList.DeviceList;
                        P2PManager.waitForPeerListSuccess = true;
                        P2PManager.waitForPeerList.Set();
                    }));
                    break;
                case WifiP2pManager.WifiP2pConnectionChangedAction:

                    break;
                case WifiP2pManager.WifiP2pThisDeviceChangedAction:

                    break;
            }
        }
    }
}

#endif