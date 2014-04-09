using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.Security;
using System.Threading.Tasks;
using SecureFileTransfer.Network.Entities;
using SecureFileTransfer.Features;
using SecureFileTransfer.Features.Transfers;

namespace SecureFileTransfer.Network
{
    /// <summary>
    /// An open connection on the receiving device.
    /// </summary>
    public class ReceiverConnection : Connection
    {
        public static ReceiverConnection CurrentConnection { get; set; }

        public SingleTransferReceiver DataConnection { get; set; }

        public TrivialEntityBasedProtocol.TEBPProvider TEBPProvider;

        public delegate void FileTransferStartedEventHandler(SingleTransferReceiver srv);
        public event FileTransferStartedEventHandler FileTransferStarted;

        public delegate void FileTransferEndedEventHandler(SingleTransferReceiver srv, bool success);
        public event FileTransferEndedEventHandler FileTransferEnded;


        public ReceiverConnection(Socket sock) : base(sock) {
            CurrentConnection = this;
        }

        public void RaiseFileTransferStarted(SingleTransferReceiver srv)
        {
            UIThreadSyncContext.Send(new System.Threading.SendOrPostCallback(state =>
            {
                if (FileTransferStarted != null)
                    FileTransferStarted(srv);
            }), null);
        }

        public void RaiseFileTransferEnded(SingleTransferReceiver srv, bool success)
        {
            UIThreadSyncContext.Send(new System.Threading.SendOrPostCallback(state =>
            {
                if (FileTransferEnded != null)
                    FileTransferEnded(srv, success);
            }), null);
        }

        public override bool DoInitialHandshake()
        {
            Write(CMD_CONN_MAGIC);
            Write(CreateServerInformation());

            byte[] answer = new byte[2];
            Get(answer);
            if (Encoding.GetString(answer) != CMD_OK)
                throw new InvalidHandshakeException(InvalidHandshakeException.HandshakePhase.InitialHello);

            EnableEncryption(EncryptionContext.ConnectionType.Server);

            string password = Encoding.GetString(GetUndefinedLength());
            if (password != LocalServer.PublicConnectionPassword)
                throw new InvalidHandshakeException(InvalidHandshakeException.HandshakePhase.Authentication);

            RemoteName = Encoding.GetString(GetUndefinedLength());

            SendAccept();

            Write(Android.OS.Build.Model, true);

            return true;
        }

        protected override void InternalBeginReceiving()
        {
            TEBPProvider = new TrivialEntityBasedProtocol.TEBPProvider(this, new TrivialEntityBasedProtocol.PlatformDependent.Android());
            TEBPProvider.ReceivedRequest += TEBPProvider_ReceivedRequest;
            TEBPProvider.ReceivedNotice += TEBPProvider_ReceivedNotice;
            TEBPProvider.Init();
        }

        void TEBPProvider_ReceivedNotice(TrivialEntityBasedProtocol.Notice not)
        {
            if (not is FileTransferAbortNotice)
            {
                Console.WriteLine("Got FileTransferAbortNotice");

                if (DataConnection != null)
                    DataConnection.Abort();
            }
        }

        void TEBPProvider_ReceivedRequest(TrivialEntityBasedProtocol.Request req)
        {
            if (req is FileTransferRequest)
            {
                Console.WriteLine("Got FileTransferRequest");

                AES fileAES = new AES();
                fileAES.Generate();

                Transfer transfer = Transfer.GetForRequest(req as FileTransferRequest);

                DataConnection = SingleTransferReceiver.GetServer();
                DataConnection.ParentConnection = this;

                FileTransferResponse resp = new FileTransferResponse()
                {
                    AesKey = fileAES.aesKey,
                    AesIv = fileAES.aesIV,
                    DataConnectionAddress = DataConnection.Address,
                    DataConnectionPort = SingleTransferReceiver.Port
                };
                req.Respond(resp);

                if (DataConnection.GetConnection())
                {
                    DataConnection.BeginReceiving(transfer, fileAES);
                }
                else
                {
                    DataConnection.Dispose();
                    DataConnection = null;
                }
            }
            else
            {
                req.Decline();
            }
        }

        public void AbortFileTransfer()
        {
            TEBPProvider.Send(new FileTransferAbortNotice());

            if (DataConnection != null)
                DataConnection.Abort();
        }

        public override void Shutdown()
        {
            RaiseDisconnected();
        }

        public override void Dispose()
        {
            if (TEBPProvider != null && !TEBPProvider.IsShutDown)
                TEBPProvider.Shutdown(false);

            CurrentConnection = null;

            if (DataConnection != null)
                DataConnection.Dispose();

            Console.WriteLine("LocalServerConnection terminated.");

            base.Dispose();
        }
    }
}
