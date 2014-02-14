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
        public static LocalServerConnection CurrentConnection { get; set; }

        public LocalServerConnection(Socket sock) : base(sock) {
            CurrentConnection = this;
        }

        public override bool DoInitialHandshake()
        {
            Write("DLP2P");
            byte[] answer = new byte[2];
            Get(answer);
            if (ASCII.GetString(answer) != "OK")
                return false;

            EnableEncryption(EncryptionContext.ConnectionType.Server);

            string password = ASCII.GetString(GetUndefinedLength());
            if (password != LocalServer.PublicConnectionPassword)
                return false;
            RemoteName = ASCII.GetString(GetUndefinedLength());

            Write("OK");

            Write(Android.OS.Build.Model, true);

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            CurrentConnection = null;

            Console.WriteLine("LocalServerConnection terminated.");
        }
    }
}
