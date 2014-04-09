using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using SecureFileTransfer.Network.Entities;
using SecureFileTransfer.Features;
using SecureFileTransfer.Features.Transfers;

namespace SecureFileTransfer.Network
{
    /// <summary>
    /// An open connection on the sending device.
    /// </summary>
    public class SenderConnection : Connection
    {
        public static SenderConnection CurrentConnection { get; private set; }

        public SingleTransferSender DataConnection { get; set; }

        public TrivialEntityBasedProtocol.TEBPProvider TEBPProvider;

        public string ConnectionPassword { get; set; }

        public delegate void FileTransferEndedEventHandler(Transfer trans, bool success, bool aborted);
        public event FileTransferEndedEventHandler FileTransferEnded;

        public static SenderConnection ConnectTo(string hostName, int port, string connectionPassword)
        {
            if (CurrentConnection != null)
                throw new NotSupportedException("There is already a client connection available.");

            SenderConnection c = new SenderConnection();
            c.ConnectionPassword = connectionPassword;
            c.Connect(hostName, port);

            if (!c.DoInitialHandshake())
                return null;

            CurrentConnection = c;

            return c;
        }

        public static SenderConnection CreateWithoutEndpoint()
        {
            CurrentConnection = new SenderConnection();
            CurrentConnection.ConnectionSocket = null;

            return CurrentConnection;
        }

        public static async Task<SenderConnection> ConnectToAsync(string hostName, int port, string connectionPassword)
        {
            return await Task.Run<SenderConnection>(() =>
            {
                return ConnectTo(hostName, port, connectionPassword);
            });
        }

        private SenderConnection() { }

        private void Connect(string host, int port)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            IPAddress addr = hostEntry.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(addr, port);

            ConnectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //ConnectionSocket.NoDelay = true;
            ConnectionSocket.Connect(endPoint);
        }

        public override bool DoInitialHandshake()
        {
            byte[] hello = new byte[5];
            Get(hello);
            if (Encoding.GetString(hello) != CMD_CONN_MAGIC)
                throw new InvalidHandshakeException(InvalidHandshakeException.HandshakePhase.InitialHello);

            ServerInformation si = GetServerInformation();
            if (si.version != CurrentVersion)
                throw new InvalidHandshakeException(InvalidHandshakeException.HandshakePhase.VersionExchange);

            // In future versions, we may read the connection flags here.

            WriteRaw(CMD_OK);

            EnableEncryption(Security.EncryptionContext.ConnectionType.Client);

            Write(ConnectionPassword, true);
            Write(Android.OS.Build.Model, true);

            byte[] ok = new byte[2];
            Get(ok);
            if (Encoding.GetString(ok) != CMD_OK)
                throw new InvalidHandshakeException(InvalidHandshakeException.HandshakePhase.EncryptionChannelTest);

            RemoteName = Encoding.GetString(GetUndefinedLength());

            return true;
        }

        protected override void InternalBeginReceiving()
        {
            TEBPProvider = new TrivialEntityBasedProtocol.TEBPProvider(this, new TrivialEntityBasedProtocol.PlatformDependent.Android());
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

        public override void Shutdown()
        {
            RaiseDisconnected();
        }
    
        /// <summary>
        /// Start a transfer for a local file
        /// </summary>
        /// <param name="localFilePath">Path to a local file</param>
        public void StartFileTransfer(string localFilePath)
        {
            var fileTransfer = new ExistingFileTransfer()
            {
                AbsoluteFilePath = localFilePath
            };

            StartFileTransfer(fileTransfer);
        }

        /// <summary>
        /// Start a transfer for local data
        /// </summary>
        /// <param name="fileStream">A data stream</param>
        /// <param name="size">The stream length</param>
        /// <param name="name">A file name representing the data</param>
        public void StartFileTransfer(System.IO.Stream fileStream, long size, string name)
        {
            var fileTransfer = new ExistingFileTransfer()
            {
                FileStream = fileStream,
                FileLength = size,
                FileName = name
            };

            StartFileTransfer(fileTransfer);
        }

        /// <summary>
        /// Start a transfer
        /// </summary>
        /// <param name="transfer"></param>
        public void StartFileTransfer(Transfer transfer)
        {
            var req = transfer.GenerateRequest();

            if (ConnectionSocket != null)
            {
                // Send transfer request
                TEBPProvider.Send(req, response =>
                {
                    if (response.Accepted)
                    {
                        Console.WriteLine("Server accepted FileTransferRequest.");
                        FileTransferResponse res = response as FileTransferResponse;

                        DataConnection = SingleTransferSender.ConnectTo(res.DataConnectionAddress, res.DataConnectionPort);
                        if (DataConnection == null)
                            throw new ConnectionException("Could not establish data connection.");
                        DataConnection.ParentConnection = this;

                        DataConnection.BeginSending(transfer, res.AesKey, res.AesIv);
                    }
                    else if (response is TrivialEntityBasedProtocol.DefaultEntities.NoResponse)
                    {
                        RaiseFileTransferEnded(transfer, false, false);
                    }
                });
            }
            else
            {
                // Special mode.
                DataConnection = new SingleTransferSenderSpecial();
                DataConnection.ParentConnection = this;
                DataConnection.BeginSending(transfer, null, null);
            }
        }

        /// <summary>
        /// Abort the current transfer
        /// </summary>
        /// <param name="sendAbort"></param>
        public void AbortFileTransfer(bool sendAbort = true)
        {
            if (sendAbort && ConnectionSocket != null)
                TEBPProvider.Send(new FileTransferAbortNotice());

            if (DataConnection != null)
                DataConnection.Abort();
        }

        public void RaiseFileTransferEnded(Transfer trans, bool success, bool aborted)
        {
            UIThreadSyncContext.Send(new System.Threading.SendOrPostCallback(state =>
            {
                if (FileTransferEnded != null)
                    FileTransferEnded(trans, success, aborted);
            }), null);
        }

        public override void Dispose()
        {
            if (TEBPProvider != null && !TEBPProvider.IsShutDown)
                TEBPProvider.Shutdown(false);

            if (DataConnection != null)
                DataConnection.Dispose();

            CurrentConnection = null;

            Console.WriteLine("SenderConnection terminated.");

            base.Dispose();
        }
    }
}
