using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    public abstract class Request : Entity
    {
        public Request()
            : base(TEBPProvider.GetNextIdentifier(), true, TYPE_REQUEST)
        { 
            
        }

        public void Respond(Response res)
        {
            if (Provider == null)
                throw new NotSupportedException("Tried to respond without a provider.");

            res.Identifier = this.Identifier;
            Provider.Send(res);
        }
    }
}
