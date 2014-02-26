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

namespace SecureFileTransfer.Network
{
    public class SingleTransferClient : SingleTransfer<ClientConnection>
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

            TransferThread = new Thread(() => TransferSend());
            TransferThread.Start();
        }

        private void TransferSend()
        {
            SendAccept();
            byte[] ok = new byte[2];
            Get(ok);
            if (ASCII.GetString(ok) != CMD_OK)
                return;

            CurrentTransferDataLeft = CurrentTransfer.FileLength;

            byte[] buf = new byte[Security.AES.BlockSize];
            byte[] writeTemp = new byte[Security.AES.BlockSize];
            int n;

            while (CurrentTransferDataLeft > 0 && !AbortCurrentTransfer)
            {
                n = CurrentTransfer.GetData(buf);
                if (n < Security.AES.BlockSize)
                    Security.Padding.SecurelyPadBufferFromPosition(buf, n);

                try
                {
                    WriteSingleBlockFast(buf, writeTemp);
                }
                catch (Exception ex)
                {
                    if (AbortCurrentTransfer || (ex is SocketException &&
                        ((ex as SocketException).SocketErrorCode == SocketError.ConnectionReset || (ex as SocketException).SocketErrorCode == SocketError.Shutdown)))
                    {
                        // Transfer von Gegenstelle abgebrochen.
                        AbortCurrentTransfer = true;

                        break;
                    }
                    throw;
                }

                CurrentTransferDataLeft -= buf.Length;
            }

            CurrentTransfer.Close();

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
            base.Dispose();

            Console.WriteLine("SingleTransferClient terminated.");
        }
    }
}
