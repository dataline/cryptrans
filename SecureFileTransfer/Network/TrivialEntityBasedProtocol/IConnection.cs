using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    public interface IConnection
    {
        bool CanSend();
        void Send(string str);
        string Receive();
        void Shutdown();
    }
}