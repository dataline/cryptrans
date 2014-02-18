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

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    public interface IConnection
    {
        void Send(string str);
        string Receive();
        void Shutdown();
    }
}