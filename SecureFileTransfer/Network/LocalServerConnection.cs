﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.Security;
using System.Threading.Tasks;
using SecureFileTransfer.Network.Entities;
using SecureFileTransfer.Features;

namespace SecureFileTransfer.Network
{
    public class LocalServerConnection : Connection
    {
        public static LocalServerConnection CurrentConnection { get; set; }

        public SingleTransferServer DataConnection { get; set; }

        public TrivialEntityBasedProtocol.TEBPProvider TEBPProvider;

        public delegate void FileTransferStartedEventHandler(SingleTransferServer srv);
        public event FileTransferStartedEventHandler FileTransferStarted;

        public delegate void FileTransferEndedEventHandler(SingleTransferServer srv, bool success);
        public event FileTransferEndedEventHandler FileTransferEnded;


        public LocalServerConnection(Socket sock) : base(sock) {
            CurrentConnection = this;
        }

        public void RaiseFileTransferStarted(SingleTransferServer srv)
        {
            UIThreadSyncContext.Send(new System.Threading.SendOrPostCallback(state =>
            {
                if (FileTransferStarted != null)
                    FileTransferStarted(srv);
            }), null);
        }

        public void RaiseFileTransferEnded(SingleTransferServer srv, bool success)
        {
            UIThreadSyncContext.Send(new System.Threading.SendOrPostCallback(state =>
            {
                if (FileTransferEnded != null)
                    FileTransferEnded(srv, success);
            }), null);
        }

        public override bool DoInitialHandshake()
        {
            Write(CMD_CONN_MAGIC);
            Write(CreateServerInformation());

            byte[] answer = new byte[2];
            Get(answer);
            if (ASCII.GetString(answer) != CMD_OK)
                return false;

            EnableEncryption(EncryptionContext.ConnectionType.Server);

            string password = ASCII.GetString(GetUndefinedLength());
            if (password != LocalServer.PublicConnectionPassword)
                return false;
            RemoteName = ASCII.GetString(GetUndefinedLength());

            SendAccept();

            Write(Android.OS.Build.Model, true);

            //AES dataConnectionAES = new AES();
            //dataConnectionAES.Generate();
            
            //Write(SingleTransferServer.Port.ToString(), true);
            //Write(DataConnection.Address, true);
            //Write(dataConnectionAES.aesKey);
            //Write(dataConnectionAES.aesIV);

            return true;
        }

        protected override void InternalBeginReceiving()
        {
            TEBPProvider = new TrivialEntityBasedProtocol.TEBPProvider(this);
            TEBPProvider.ReceivedRequest += TEBPProvider_ReceivedRequest;
            TEBPProvider.Init();
        }

        void TEBPProvider_ReceivedRequest(TrivialEntityBasedProtocol.Request req)
        {
            if (req is FileTransferRequest)
            {
                Console.WriteLine("Got FileTransferRequest");

                AES fileAES = new AES();
                fileAES.Generate();

                Transfer transfer = Transfer.GetForRequest(req as FileTransferRequest);

                DataConnection = SingleTransferServer.GetServer();
                DataConnection.ParentConnection = this;

                DataConnection.BeginReceiving(transfer, fileAES);

                FileTransferResponse resp = new FileTransferResponse()
                {
                    AesKey = fileAES.aesKey,
                    AesIv = fileAES.aesIV,
                    DataConnectionAddress = DataConnection.Address,
                    DataConnectionPort = SingleTransferServer.Port
                };
                req.Respond(resp);

                if (DataConnection.GetConnection())
                {
                    DataConnection.BeginReceiving();
                }
                else
                {
                    DataConnection.Dispose();
                    DataConnection = null;
                }
            }
            else
            {
                req.Decline();
            }
        }

        public override void Shutdown()
        {
            RaiseDisconnected();
        }

        public override void Dispose()
        {
            if (TEBPProvider != null && !TEBPProvider.IsShutDown)
                TEBPProvider.Shutdown(false);

            CurrentConnection = null;

            if (DataConnection != null)
                DataConnection.Dispose();

            Console.WriteLine("LocalServerConnection terminated.");

            base.Dispose();
        }
    }
}
