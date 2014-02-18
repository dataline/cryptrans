using SecureFileTransfer.Network.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SecureFileTransfer.Network
{
    public class SingleTransferClient : Connection
    {
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

        public void BeginSending(FileTransferRequest request, byte[] aesKey, byte[] aesIv)
        {
            encCtx = new Security.EncryptionContext(this, aesKey, aesIv);
            SendAccept();
            byte[] ok = new byte[2];
            Get(ok);
            if (ASCII.GetString(ok) != CMD_OK)
                return;

            byte[] test = new byte[16];
            for (int i = 0; i < 10000; i++)
            {
                Write(test);
            }
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
