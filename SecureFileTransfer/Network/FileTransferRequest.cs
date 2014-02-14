using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network
{
    public class FileTransferRequest : Request
    {
        public static string RequestIdentifier
        {
            get { return "ft"; }
        }

        public override void Process(LocalServerConnection conn)
        {
            throw new NotImplementedException();
        }

        public override void Perform(ClientConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}
