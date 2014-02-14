using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network
{
    public abstract class Request
    {
        public abstract void Process(LocalServerConnection conn);

        public abstract void Perform(ClientConnection conn);

        public static Request GetRequestForIdentifier(string id)
        {
            if (id == FileTransferRequest.RequestIdentifier)
                return new FileTransferRequest();

            return null;
        }
    }
}
