using SecureFileTransfer.Features;
using SecureFileTransfer.Features.Transfers;
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

        long currentTransferDataLeft;

        public int Progress
        {
            get
            {
                if (CurrentTransfer == null)
                    return 0;

                return (int)((1.0f - ((float)currentTransferDataLeft / (float)CurrentTransfer.FileLength)) * 100.0f);
            }
        }

        public int BytesPerSecond { get; private set; }

        long previousDataRead = 0;
        public void ReloadBytesPerSecond()
        {
            long dataRead = CurrentTransfer.FileLength - currentTransferDataLeft;
            BytesPerSecond = (int)(dataRead - previousDataRead);
            previousDataRead = dataRead;
        }

        public LocalServerConnection ParentConnection { get; set; }

        public Transfer CurrentTransfer { get; private set; }

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

        public void Abort()
        {
            AbortCurrentTransfer = true;
            if (ConnectionSocket != null)
            {
                // Eventuell im Get festhängenden Receive-Thread befreien:
                ConnectionSocket.Close();
                ConnectionSocket = null;
            }
        }

        public void BeginReceiving(Transfer transfer, SecureFileTransfer.Security.AES aes)
        {
            CurrentTransfer = transfer;
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

            currentTransferDataLeft = CurrentTransfer.FileLength;

            ParentConnection.RaiseFileTransferStarted(this);

            Console.WriteLine("Start receiving file.");

            byte[] buf = new byte[Security.AES.BlockSize];
            byte[] getTemp = new byte[Security.AES.BlockSize];
            int len;

            while (currentTransferDataLeft > 0 && !AbortCurrentTransfer)
            {
                len = currentTransferDataLeft > Security.AES.BlockSize ? Security.AES.BlockSize : (int)currentTransferDataLeft;

                try
                {
                    GetSingleBlockFast(buf, getTemp);
                }
                catch (Exception)
                {
                    if (AbortCurrentTransfer)
                    { 
                        // Transfer von Gegenstelle abgebrochen.
                        AbortCurrentTransfer = true;
                        
                        break;
                    }
                    throw;
                }

                CurrentTransfer.AppendData(buf, len);

                currentTransferDataLeft -= len;
            }

            Console.WriteLine("End receiving file.");

            CurrentTransfer.Close();
            if (AbortCurrentTransfer)
                CurrentTransfer.WriteAborted();
            else
                CurrentTransfer.WriteSucceeded();

            if (ConnectionSocket != null)
            {
                ConnectionSocket.Close();
                ConnectionSocket = null;
            }

            ParentConnection.RaiseFileTransferEnded(this, !AbortCurrentTransfer);
            CurrentTransfer = null;

            this.Dispose();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            Abort();

            if (listenerSocket != null)
                listenerSocket.Close();

            Console.WriteLine("SingleTransferServer terminated.");

            base.Dispose();
        }
    }
}
