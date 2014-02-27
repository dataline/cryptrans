using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol.DefaultEntities
{
    /// <summary>
    /// This response will be sent if a request timed out.
    /// </summary>
    public class NoResponse : Response
    {
        public NoResponse(TEBPProvider provider)
        {
            Accepted = false;
            Provider = provider;
        }
    }
}
