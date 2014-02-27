using Android.Net.Wifi.P2p;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.WiFiDirect
{
    public class PeerListListener : Java.Lang.Object, Android.Net.Wifi.P2p.WifiP2pManager.IPeerListListener
    {
        Action<WifiP2pDeviceList> peersAvailable;

        public PeerListListener(Action<WifiP2pDeviceList> peersAvailable)
        {
            this.peersAvailable = peersAvailable;
        }

        public void OnPeersAvailable(WifiP2pDeviceList peers)
        {
            if (peersAvailable != null)
                peersAvailable(peers);
        }
    }
}
