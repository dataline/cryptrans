using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    public abstract class Response : Entity
    {
        public bool Accepted { get; set; }

        public Response()
            : base(0, false, TYPE_RESPONSE)
        { 
        
        }
    }
}
