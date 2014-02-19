using SecureFileTransfer.Features;
using SecureFileTransfer.Network.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SecureFileTransfer.Network
{
    public class SingleTransferClient : Connection
    {
        public Transfer CurrentTransfer { get; private set; }

        public ClientConnection ParentConnection { get; set; }

        long currentTransferDataLeft;

        Thread sendThread;

        public int Progress
        {
            get
            {
                if (CurrentTransfer == null)
                    return 0;

                return (int)((1.0f - ((float)currentTransferDataLeft / (float)CurrentTransfer.FileLength)) * 100.0f);
            }
        }

        public bool AbortCurrentTransfer { get; set; }


        public static SingleTransferClient ConnectTo(string hostName, int port)
        {
            var client = new SingleTransferClient();
            client.Connect(hostName, port);

            if (!client.DoInitialHandshake())
                return null;

            Console.WriteLine("SingleTransferClient established.");

            return client;
        }

        protected SingleTransferClient() { }

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

            SendAccept();

            return true;
        }

        protected override void InternalBeginReceiving()
        {
            throw new NotImplementedException();
        }

        public void BeginSending(Transfer transfer, byte[] aesKey, byte[] aesIv)
        {
            CurrentTransfer = transfer;
            AbortCurrentTransfer = false;

            encCtx = new Security.EncryptionContext(this, aesKey, aesIv);

            sendThread = new Thread(() => TransferSend());
            sendThread.Start();
        }

        private void TransferSend()
        {
            SendAccept();
            byte[] ok = new byte[2];
            Get(ok);
            if (ASCII.GetString(ok) != CMD_OK)
                return;

            currentTransferDataLeft = CurrentTransfer.FileLength;

            while (currentTransferDataLeft > 0)
            {
                byte[] buf = CurrentTransfer.GetData(Security.AES.BlockSize);

                Write(buf);

                currentTransferDataLeft -= buf.Length;
            }

            CurrentTransfer.Close();

            ConnectionSocket.Close();
            ConnectionSocket = null;


            ParentConnection.RaiseFileTransferEnded(this, true);

            CurrentTransfer = null;

            this.Dispose();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            base.Dispose();

            Console.WriteLine("SingleTransferClient terminated.");
        }
    }
}
