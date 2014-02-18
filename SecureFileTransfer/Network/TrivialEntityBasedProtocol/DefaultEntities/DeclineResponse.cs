using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol.DefaultEntities
{
    public class DeclineResponse : Response
    {
        public DeclineResponse()
        {
            Accepted = false;
        }
    }
}
