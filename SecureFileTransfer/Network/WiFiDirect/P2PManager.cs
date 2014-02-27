using Android.Content;
using Android.Net.Wifi.P2p;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFileTransfer.Network.WiFiDirect
{
    public static class P2PManager
    {
        static IntentFilter intentFilter = null;
        static WifiP2pManager manager = null;
        static WifiP2pManager.Channel channel = null;
        static WiFiDirectBroadcastReceiver receiver = null;
        static Context ctx = null;

        public static ManualResetEvent waitForPeerList = new ManualResetEvent(false);
        public static bool waitForPeerListSuccess;
        public static ICollection<WifiP2pDevice> peerList = null;
        

        public static bool Available { get; set; }

        public static void Initialize(Context ctx)
        {
            P2PManager.ctx = ctx;

            if (intentFilter == null)
            {
                intentFilter = new IntentFilter();

                intentFilter.AddAction(WifiP2pManager.WifiP2pStateChangedAction);
                intentFilter.AddAction(WifiP2pManager.WifiP2pPeersChangedAction);
                intentFilter.AddAction(WifiP2pManager.WifiP2pConnectionChangedAction);
                intentFilter.AddAction(WifiP2pManager.WifiP2pThisDeviceChangedAction);
            }
            if (manager == null)
            {
                manager = (WifiP2pManager)ctx.GetSystemService(Context.WifiP2pService);
                channel = manager.Initialize(ctx, ctx.MainLooper, null);
            }
            if (receiver == null)
            {
                receiver = new WiFiDirectBroadcastReceiver(manager, channel);
            }

            ctx.RegisterReceiver(receiver, intentFilter);
        }

        public static void Shutdown()
        {
            if (ctx == null || receiver == null)
                return;

            ctx.UnregisterReceiver(receiver);
        }

        public static async Task<ICollection<WifiP2pDevice>> GetPeersAsync()
        {
            return await Task.Run<ICollection<WifiP2pDevice>>(() =>
            {
                waitForPeerListSuccess = false;
                waitForPeerList.Reset();

                manager.DiscoverPeers(channel, new WifiActionListener(null, reason =>
                {
                    throw new Exception();
                }));

                waitForPeerList.WaitOne();

                if (!waitForPeerListSuccess)
                    return null;
                else
                    return peerList;
            });
        }


    }
}
