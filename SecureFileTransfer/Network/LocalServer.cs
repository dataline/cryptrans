﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFileTransfer.Network
{
    /// <summary>
    /// A server that waits for a connection from a client.
    /// 
    /// This is active as long as the QR code is shown in the main activity.
    /// </summary>
    public class LocalServer : IDisposable
    {
        public string PublicConnectionPassword;

        public SynchronizationContext UIThreadSyncContext { get; set; }

        public static LocalServer CurrentServer { get; private set; }

        public delegate void GotConnectionEventHandler(ReceiverConnection connection);
        public event GotConnectionEventHandler GotConnection;

        public delegate void FailedConnectionAttemptEventHandler(Exception ex);
        public event FailedConnectionAttemptEventHandler FailedConnectionAttempt;

        public static async Task<LocalServer> GetServerAsync(CancellationToken ct)
        {
            if (CurrentServer != null)
                return CurrentServer;

            return await CreateServerAsync(ct);
        }

        public static async Task<LocalServer> CreateServerAsync(CancellationToken ct)
        {
            if (ReceiverConnection.CurrentConnection != null)
                throw new NotSupportedException("There is already a server connection available.");

            var srv = new LocalServer();
            await Task.Run(() => srv.EstablishSocket());

            if (ct.IsCancellationRequested)
            {
                srv.Dispose();
                return null;
            }

            CurrentServer = srv;

            return srv;
        }

        Socket sock = null;

        public string Address { get; set; }
        public const int Port = 23956;

        private LocalServer() 
        {
            PublicConnectionPassword = Security.PasswordGenerator.Generate(8);
        }

        void EstablishSocket()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress addr = host.AddressList[0];
            IPEndPoint local = new IPEndPoint(addr, Port);

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //sock.NoDelay = true;
            sock.Bind(local);

            Address = addr.ToString();

            sock.Listen(100);
            sock.BeginAccept(new AsyncCallback(AcceptCallback), sock);

            Console.WriteLine("Local Server started.");
        }

        void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket accepted = (ar.AsyncState as Socket).EndAccept(ar);

                if (!CreateConnection(accepted))
                    sock.BeginAccept(new AsyncCallback(AcceptCallback), sock); // try again.
            }
            catch (ObjectDisposedException)
            {
            }
        }

        bool CreateConnection(Socket connection)
        {
            var conn = new ReceiverConnection(connection, new[] { PublicConnectionPassword });

            bool accepted = false;
            try
            {
                accepted = conn.DoInitialHandshake();
            }
            catch (InvalidHandshakeException ex)
            {
                RaiseFailedConnectionAttempt(ex);
            }

            if (accepted)
                RaiseGotConnection(conn);
            else
                conn.Dispose();

            return accepted;
        }
            


        void RaiseGotConnection(ReceiverConnection conn)
        {
            UIThreadSyncContext.Send(new SendOrPostCallback(state =>
            {
                if (GotConnection != null)
                    GotConnection(conn);
            }), null);
        }

        void RaiseFailedConnectionAttempt(Exception ex)
        {
            UIThreadSyncContext.Send(new SendOrPostCallback(state =>
            {
                if (FailedConnectionAttempt != null)
                    FailedConnectionAttempt(ex);
            }), null);
        }

        public void Dispose()
        {
            if (sock != null)
                sock.Close();

            Console.WriteLine("Local Server terminated.");

            CurrentServer = null;
        }
    }
}
