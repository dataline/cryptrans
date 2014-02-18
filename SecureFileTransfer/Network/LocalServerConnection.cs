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

        public SingleTransferServer DataConnection { get; set; }

        public delegate bool AcceptRequestEventHandler(Request req);
        public event AcceptRequestEventHandler AcceptRequest;

        public delegate void FileTransferStartedEventHandler(SingleTransferServer srv);
        public event FileTransferStartedEventHandler FileTransferStarted;

        public delegate void FileTransferEndedEventHandler(SingleTransferServer srv, bool success);
        public event FileTransferEndedEventHandler FileTransferEnded;


        public LocalServerConnection(Socket sock) : base(sock) {
            CurrentConnection = this;
        }

        public void RaiseFileTransferStarted(SingleTransferServer srv)
        {
            if (FileTransferStarted != null)
                FileTransferStarted(srv);
        }

        public void RaiseFileTransferEnded(SingleTransferServer srv, bool success)
        {
            if (FileTransferEnded != null)
                FileTransferEnded(srv, success);
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

            SendAccept();

            Write(Android.OS.Build.Model, true);

            //AES dataConnectionAES = new AES();
            //dataConnectionAES.Generate();

            DataConnection = SingleTransferServer.GetServer();
            DataConnection.ParentConnection = this;
            
            Write(DataConnection.Address, true);
            Write(SingleTransferServer.Port.ToString(), true);
            //Write(dataConnectionAES.aesKey);
            //Write(dataConnectionAES.aesIV);

            return DataConnection.GetConnection();
        }

        protected override void InternalBeginReceiving()
        {
            Task.Run(() =>
            {
                string requestString;
                try
                {
                    requestString = ASCII.GetString(GetUndefinedLength());
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
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

        public bool DoesAcceptRequest(Request req)
        {
            if (AcceptRequest == null)
                return false;
            return AcceptRequest(req);
        }

        public override void Dispose()
        {
            //SendShutdown();
            ConnectionSocket.Shutdown(SocketShutdown.Both);

            CurrentConnection = null;

            if (DataConnection != null)
                DataConnection.Dispose();

            Console.WriteLine("LocalServerConnection terminated.");

            base.Dispose();
        }
    }
}
