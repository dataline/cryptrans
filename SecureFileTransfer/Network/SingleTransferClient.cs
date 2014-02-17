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
        public static SingleTransferClient ConnectTo(string hostName, int port, byte[] aesKey, byte[] aesIv)
        {
            var client = new SingleTransferClient();
            client.Connect(hostName, port);

            if (!client.DoInitialHandshake(aesKey, aesIv))
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
            throw new NotImplementedException();
        }

        public bool DoInitialHandshake(byte[] aesKey, byte[] aesIv)
        {
            byte[] hello = new byte[5];
            Get(hello);
            if (ASCII.GetString(hello) != CMD_CONN_MAGIC)
                return false;

            encCtx = new Security.EncryptionContext(this, aesKey, aesIv);

            SendAccept();

            return true;
        }

        protected override void InternalBeginReceiving()
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
