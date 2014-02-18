using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    public abstract class Notice : Entity
    {
        public Notice()
            : base(0, false)
        { 
        
        }
    }
}
