using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.Security;

namespace SecureFileTransfer.Network
{
    public class LocalServerConnection : Connection
    {
        public string ReceivedConnectionPassword { get; set; }

        public LocalServerConnection(Socket sock) : base(sock) { }

        public override bool DoInitialHandshake()
        {
            Write("DLP2P");
            byte[] answer = new byte[2];
            Get(answer);
            if (ASCII.GetString(answer) != "OK")
                return false;

            EnableEncryption(EncryptionContext.ConnectionType.Server);

            byte[] password = GetUndefinedLength();
            ReceivedConnectionPassword = ASCII.GetString(password);

            Write("OK");

            return true;
        }
    }
}
