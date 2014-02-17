﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        public bool GetConnection(Security.AES aes)
        {
            Listen();
            return DoInitialHandshake(aes);
        }

        private SingleTransferServer() { }

        public string Address { get; set; }
        public const int Port = LocalServer.Port + 1;

        public LocalServerConnection ParentConnection { get; set; }

        public FileTransferRequest CurrentRequest { get; private set; }

        Socket listenerSocket;

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

            listenerSocket.Close();
            listenerSocket.Dispose();
            listenerSocket = null;
        }

        public override bool DoInitialHandshake()
        {
            throw new NotImplementedException();
        }

        public bool DoInitialHandshake(Security.AES aes)
        {
            Write(CMD_CONN_MAGIC);

            encCtx = new Security.EncryptionContext(this, aes);

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

        public void BeginReceiving(FileTransferRequest request)
        {
            CurrentRequest = request;
            ParentConnection.RaiseFileTransferStarted(this);

            Task.Run(() =>
            {
                long toRead = request.FileLength;
                byte[] file = new byte[toRead];

                Console.WriteLine("Start receiving file.");

                while (toRead > 0)
                {
                    byte[] buf = new byte[toRead > Security.AES.BlockSize ? Security.AES.BlockSize : toRead];
                    Get(buf);
                    Array.Copy(buf, 0, file, file.Length - toRead, buf.Length);

                    toRead -= buf.Length;
                }

                Console.WriteLine("End receiving file.");

                ParentConnection.RaiseFileTransferEnded(this, true);
                CurrentRequest = null;
            });
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
