using SecureFileTransfer.Network.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFileTransfer.Network
{
    public class SingleTransferServer : Connection
    {
        public static SingleTransferServer GetServer()
        {
            var srv = new SingleTransferServer();

            srv.EstablishSocket();

            return srv;
        }

        public bool GetConnection()
        {
            Listen();
            return DoInitialHandshake();
        }

        private SingleTransferServer() { }

        public string Address { get; set; }
        public const int Port = LocalServer.Port + 1;

        public LocalServerConnection ParentConnection { get; set; }

        public FileTransferRequest CurrentRequest { get; private set; }

        public bool AbortCurrentTransfer { get; set; }

        Socket listenerSocket;

        Thread receiveThread;

        void EstablishSocket()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress addr = host.AddressList[0];
            IPEndPoint local = new IPEndPoint(addr, Port);

            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenerSocket.Bind(local);

            Address = addr.ToString();

            listenerSocket.Listen(100);

            Console.WriteLine("SingleTransferServer started.");
        }

        void Listen()
        {
            ConnectionSocket = listenerSocket.Accept();
            //ConnectionSocket.ReceiveTimeout = 1000;

            listenerSocket.Close();
            listenerSocket.Dispose();
            listenerSocket = null;
        }

        public override bool DoInitialHandshake()
        {
            Write(CMD_CONN_MAGIC);

            byte[] ok = new byte[2];
            Get(ok);

            if (ASCII.GetString(ok) != CMD_OK)
                return false;

            Console.WriteLine("SingleTransferServer Connection established.");

            return true;
        }

        protected override void InternalBeginReceiving()
        {
            throw new NotImplementedException();
        }

        public void BeginReceiving(FileTransferRequest request, SecureFileTransfer.Security.AES aes)
        {
            CurrentRequest = request;
            AbortCurrentTransfer = false;

            encCtx = new Security.EncryptionContext(this, aes);

            receiveThread = new Thread(obj => Receive(obj as Security.AES));
            receiveThread.Start(aes);
        }

        private void Receive(Security.AES aes)
        {
            byte[] ok = new byte[2];
            Get(ok);
            if (ASCII.GetString(ok) != CMD_OK)
            {
                ParentConnection.SendDecline();
                return;
            }
            SendAccept();
            ParentConnection.RaiseFileTransferStarted(this);

            long toRead = CurrentRequest.FileLength;
            byte[] file = new byte[toRead];

            Console.WriteLine("Start receiving file.");

            while (toRead > 0)
            {
                byte[] buf = new byte[toRead > Security.AES.BlockSize ? Security.AES.BlockSize : toRead];
                try
                {
                    Get(buf);
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.TimedOut && !AbortCurrentTransfer)
                        continue; // noch mal versuchen

                    throw;
                }
                Array.Copy(buf, 0, file, file.Length - toRead, buf.Length);

                toRead -= buf.Length;
            }

            Console.WriteLine("End receiving file.");

            ParentConnection.RaiseFileTransferEnded(this, true);
            CurrentRequest = null;
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            if (listenerSocket != null)
                listenerSocket.Close();

            Console.WriteLine("SingleTransferServer terminated.");

            base.Dispose();
        }
    }
}
