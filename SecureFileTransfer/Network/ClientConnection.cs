using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace SecureFileTransfer.Network
{
    public class ClientConnection : Connection
    {
        public static ClientConnection CurrentConnection { get; private set; }

        public SingleTransferClient DataConnection { get; set; }

        public TrivialEntityBasedProtocol.TEBPProvider TEBPProvider;

        public string ConnectionPassword { get; set; }

        private Action RunAfterAcceptAction = null;

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

            string dcAddress = GetUndefinedLengthString();
            int dcPort = Convert.ToInt32(GetUndefinedLengthString());

            //byte[] dcAesKey = new byte[Security.AES.KeySize];
            //byte[] dcAesIv = new byte[Security.AES.BlockSize];
            //Get(dcAesKey);
            //Get(dcAesIv);

            DataConnection = SingleTransferClient.ConnectTo(dcAddress, dcPort);

            return DataConnection != null;
        }

        protected override void InternalBeginReceiving()
        {
            TEBPProvider = new TrivialEntityBasedProtocol.TEBPProvider(this);
            TEBPProvider.ReceivedRequest += TEBPProvider_ReceivedRequest;
            TEBPProvider.Init();
        }

        void TEBPProvider_ReceivedRequest(TrivialEntityBasedProtocol.Request req)
        {
            throw new NotImplementedException();
        }

        public override void Shutdown()
        {
            RaiseDisconnected();
        }

        public void FileTransferTest()
        {
            FileTransferRequest req = new FileTransferRequest()
            {
                FileName = "Testdatei.txt",
                FileLength = 1600,
                FileType = "data"
            };
            req.Perform(this);
        }

        public override void Dispose()
        {
            if (TEBPProvider != null && !TEBPProvider.IsShutDown)
                TEBPProvider.Shutdown();

            CurrentConnection = null;

            if (DataConnection != null)
                DataConnection.Dispose();

            Console.WriteLine("ClientConnection terminated.");

            base.Dispose();
        }
    }
}
