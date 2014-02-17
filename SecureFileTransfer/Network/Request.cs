using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network
{
    public abstract class Request
    {
        public abstract void Process(LocalServerConnection conn);

        public abstract bool Perform(ClientConnection conn);

        public static Request GetRequestForIdentifier(string id)
        {
            if (id == Connection.CMD_SHUTDOWN)
                throw new ConnectionShutDownException();

            if (id == FileTransferRequest.RequestIdentifier)
                return new FileTransferRequest();

            return null;
        }
    }

    public class ConnectionShutDownException : Exception
    { 
    }
}
