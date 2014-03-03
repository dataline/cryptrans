using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    public interface ITEBPPlatformDependent
    {
        void RunAfterDelay(Action action, long delayMillisecs);
    }
}