using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.Security;
using System.Threading.Tasks;

namespace SecureFileTransfer.Network
{
    public class LocalServerConnection : Connection
    {
        public static LocalServerConnection CurrentConnection { get; set; }

        public delegate bool AcceptRequestEventHandler(Request req);
        public event AcceptRequestEventHandler AcceptRequest;


        public LocalServerConnection(Socket sock) : base(sock) {
            CurrentConnection = this;
        }

        public override bool DoInitialHandshake()
        {
            Write(CMD_CONN_MAGIC);
            Write(CreateServerInformation());

            byte[] answer = new byte[2];
            Get(answer);
            if (ASCII.GetString(answer) != CMD_OK)
                return false;

            EnableEncryption(EncryptionContext.ConnectionType.Server);

            string password = ASCII.GetString(GetUndefinedLength());
            if (password != LocalServer.PublicConnectionPassword)
                return false;
            RemoteName = ASCII.GetString(GetUndefinedLength());

            Write(CMD_OK);

            Write(Android.OS.Build.Model, true);

            return true;
        }

        protected override void InternalBeginReceiving()
        {
            Task.Run(() =>
            {
                string requestString = ASCII.GetString(GetUndefinedLength());
                Request req;
                try
                {
                    req = Request.GetRequestForIdentifier(requestString);
                }
                catch (ConnectionShutDownException)
                {
                    RaiseDisconnected();
                    return;
                }

                if (req != null)
                {
                    SendAccept();
                    req.Process(this);
                }
                else
                {
                    SendDecline();
                }
            });
        }

        public override void Dispose()
        {
            SendShutdown();

            CurrentConnection = null;

            Console.WriteLine("LocalServerConnection terminated.");

            base.Dispose();
        }
    }
}
