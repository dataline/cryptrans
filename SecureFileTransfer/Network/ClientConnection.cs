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
    public class ClientConnection : Connection
    {
        public static ClientConnection CurrentConnection { get; private set; }

        public SingleTransferClient DataConnection { get; set; }

        public TrivialEntityBasedProtocol.TEBPProvider TEBPProvider;

        public string ConnectionPassword { get; set; }

        public delegate void FileTransferEndedEventHandler(SingleTransferClient cli, bool success, bool aborted);
        public event FileTransferEndedEventHandler FileTransferEnded;

        public void RaiseFileTransferEnded(SingleTransferClient cli, bool success, bool aborted)
        {
            UIThreadSyncContext.Send(new System.Threading.SendOrPostCallback(state =>
            {
                if (FileTransferEnded != null)
                    FileTransferEnded(cli, success, aborted);
            }), null);
        }

        public static ClientConnection ConnectTo(string hostName, int port, string connectionPassword)
        {
            if (CurrentConnection != null)
                throw new NotSupportedException("There is already a client connection available.");

            ClientConnection c = new ClientConnection();
            c.ConnectionPassword = connectionPassword;
            c.Connect(hostName, port);

            if (!c.DoInitialHandshake())
                return null;

            CurrentConnection = c;

            return c;
        }

        public static async Task<ClientConnection> ConnectToAsync(string hostName, int port, string connectionPassword)
        {
            return await Task.Run<ClientConnection>(() =>
            {
                return ConnectTo(hostName, port, connectionPassword);
            });
        }

        private ClientConnection() { }

        private void Connect(string host, int port)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            IPAddress addr = hostEntry.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(addr, port);

            ConnectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectionSocket.NoDelay = true;
            ConnectionSocket.Connect(endPoint);
        }

        public override bool DoInitialHandshake()
        {
            byte[] hello = new byte[5];
            Get(hello);
            if (ASCII.GetString(hello) != CMD_CONN_MAGIC)
                return false;

            ServerInformation si = GetServerInformation();
            if (si.version != CurrentVersion)
                return false;

            WriteRaw(CMD_OK);

            EnableEncryption(Security.EncryptionContext.ConnectionType.Client);

            Write(ConnectionPassword, true);
            Write(Android.OS.Build.Model, true);

            byte[] ok = new byte[2];
            Get(ok);
            if (ASCII.GetString(ok) != CMD_OK)
                return false;

            RemoteName = ASCII.GetString(GetUndefinedLength());

            return true;

            //string dcAddress = GetUndefinedLengthString();
            //int dcPort = Convert.ToInt32(GetUndefinedLengthString());
            //
            ////byte[] dcAesKey = new byte[Security.AES.KeySize];
            ////byte[] dcAesIv = new byte[Security.AES.BlockSize];
            ////Get(dcAesKey);
            ////Get(dcAesIv);
            //
            //DataConnection = SingleTransferClient.ConnectTo(dcAddress, dcPort);
            //
            //return DataConnection != null;
        }

        protected override void InternalBeginReceiving()
        {
            TEBPProvider = new TrivialEntityBasedProtocol.TEBPProvider(this);
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

        //public void FileTransferTest()
        //{
        //    var testTransfer = new UnsavedBinaryTransfer()
        //    {
        //        FileName = "Testdaten.txt",
        //        FileLength = 200000
        //    };
        //
        //    TEBPProvider.Send(testTransfer.GenerateRequest(), response =>
        //    {
        //        if (response.Accepted)
        //        {
        //            Console.WriteLine("Server accepted FileTransferRequest.");
        //            FileTransferResponse res = response as FileTransferResponse;
        //
        //            DataConnection.BeginSending(testTransfer, res.AesKey, res.AesIv);
        //        }
        //    });
        //}

        public void StartFileTransfer(string localFilePath)
        {
            var fileTransfer = new ExistingFileTransfer()
            {
                AbsoluteFilePath = localFilePath
            };

            StartFileTransfer(fileTransfer);
        }

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

        public void StartFileTransfer(Transfer transfer)
        {
            TEBPProvider.Send(transfer.GenerateRequest(), response =>
            {
                if (response.Accepted)
                {
                    Console.WriteLine("Server accepted FileTransferRequest.");
                    FileTransferResponse res = response as FileTransferResponse;

                    DataConnection = SingleTransferClient.ConnectTo(res.DataConnectionAddress, res.DataConnectionPort);
                    if (DataConnection == null)
                        throw new ConnectionException("Could not establish data connection.");
                    DataConnection.ParentConnection = this;

                    DataConnection.BeginSending(transfer, res.AesKey, res.AesIv);
                }
            });
        }

        public void AbortFileTransfer(bool sendAbort = true)
        {
            if (sendAbort)
                TEBPProvider.Send(new FileTransferAbortNotice());

            if (DataConnection != null)
                DataConnection.Abort();
        }

        public override void Dispose()
        {
            if (TEBPProvider != null && !TEBPProvider.IsShutDown)
                TEBPProvider.Shutdown(false);

            if (DataConnection != null)
                DataConnection.Dispose();

            CurrentConnection = null;

            Console.WriteLine("ClientConnection terminated.");

            base.Dispose();
        }
    }
}
