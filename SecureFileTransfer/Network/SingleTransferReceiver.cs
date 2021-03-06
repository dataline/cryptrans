﻿using SecureFileTransfer.Features;
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
    /// <summary>
    /// A single data transfer on the receiving device.
    /// </summary>
    public class SingleTransferReceiver : SingleTransfer<ReceiverConnection>
    {
        public static SingleTransferReceiver GetServer()
        {
            var srv = new SingleTransferReceiver();

            srv.EstablishSocket();

            return srv;
        }

        public bool GetConnection()
        {
            Listen();
            return DoInitialHandshake();
        }

        public string Address { get; set; }
        public const int Port = LocalServer.Port + 1;

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

            if (Encoding.GetString(ok) != CMD_OK)
                return false;

            Console.WriteLine("SingleTransferServer Connection established.");

            return true;
        }

        protected override void InternalBeginReceiving()
        {
            throw new NotImplementedException();
        }

        public void BeginReceiving(Transfer transfer, SecureFileTransfer.Security.AES aes)
        {
            CurrentTransfer = transfer;
            AbortCurrentTransfer = false;

            encCtx = new Security.EncryptionContext(this, aes);

            TransferThread = new Thread(obj => Receive(obj as Security.AES));
            TransferThread.Start(aes);
        }

        private void Receive(Security.AES aes)
        {
            byte[] ok = new byte[2];
            Get(ok);
            if (Encoding.GetString(ok) != CMD_OK)
            {
                ParentConnection.SendDecline();
                return;
            }
            SendAccept();

            CurrentTransferDataLeft = CurrentTransfer.FileLength;

            ParentConnection.RaiseFileTransferStarted(this);

            Console.WriteLine("Start receiving file.");

            byte[] buf = new byte[Security.AES.BlockSize];
            byte[] getTemp = new byte[Security.AES.BlockSize];
            int len;

            while (CurrentTransferDataLeft > 0 && !AbortCurrentTransfer)
            {
                len = CurrentTransferDataLeft > Security.AES.BlockSize ? Security.AES.BlockSize : (int)CurrentTransferDataLeft;

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

                CurrentTransferDataLeft -= len;
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
            if (listenerSocket != null)
                listenerSocket.Close();

            base.Dispose();

            Console.WriteLine("SingleTransferServer terminated.");
        }
    }
}
