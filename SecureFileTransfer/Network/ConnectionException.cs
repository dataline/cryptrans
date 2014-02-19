using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string msg)
            : base(msg)
        { 
        }
    }
}
