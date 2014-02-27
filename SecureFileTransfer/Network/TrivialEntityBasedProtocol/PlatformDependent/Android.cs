using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol.PlatformDependent
{
    public class Android : ITEBPPlatformDependent
    {
        public void RunAfterDelay(Action action, long delayMillisecs)
        {
            var handler = new Handler();
            handler.PostDelayed(action, delayMillisecs);
        }
    }
}
