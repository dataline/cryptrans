using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFileTransfer.Network
{
    public class LocalServer : IDisposable
    {
        public static string PublicConnectionPassword = Security.PasswordGenerator.Generate(8);

        public static LocalServer CurrentServer { get; private set; }

        public static async Task<LocalServer> GetServerAsync()
        {
            if (CurrentServer != null)
                return CurrentServer;

            return await CreateServerAsync();
        }

        public static async Task<LocalServer> CreateServerAsync()
        {
            if (LocalServerConnection.CurrentConnection != null)
                throw new NotSupportedException("There is already a server connection available.");

            var srv = new LocalServer();
            await Task.Run(() => srv.EstablishSocket());

            CurrentServer = srv;

            return srv;
        }

        public async Task<LocalServerConnection> WaitForConnectionAsync(CancellationToken ct)
        {
            if (LocalServerConnection.CurrentConnection != null)
                throw new NotSupportedException("There is already a server connection available.");

            return await Task.Run<LocalServerConnection>(() =>
            {
                ct.Register(() => sock.Close());
                LocalServerConnection conn = null;

                try
                {
                    do
                    {
                        Socket socket = ListenForConnection();
                        if (socket == null)
                            return null;
                        conn = new LocalServerConnection(socket);
                    } while (!conn.DoInitialHandshake());
                }
                catch (SocketException)
                {
                    ct.ThrowIfCancellationRequested();
                }

                return conn;
            }, ct);
        }

        /*public static async Task<LocalServerConnection> WaitForConnectionAsync()
        {
            if (Instance != null)
                throw new Exception("Server is already listening.");

            Instance = new LocalServer();
            Instance.EstablishSocket();

            LocalServerConnection connection = await Task.Run<LocalServerConnection>(() =>
            {
                LocalServerConnection conn = null;
                do
                {
                    Socket socket = Instance.ListenForConnection();
                    if (socket == null)
                        return null;
                    conn = new LocalServerConnection(socket);
                } while (!conn.DoInitialHandshake());

                return conn;
            });

            Instance.Dispose();
            Instance = null;

            return connection;
        }*/

        Socket sock = null;

        public string Address { get; set; }
        public const int Port = 23956;

        private LocalServer() { }

        void EstablishSocket()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress addr = host.AddressList[0];
            IPEndPoint local = new IPEndPoint(addr, Port);

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(local);

            Address = addr.ToString();

            Console.WriteLine("Local Server started.");
        }

        Socket ListenForConnection()
        {
            sock.Listen(100);
            Socket accepted;
            try
            {
                accepted = sock.Accept();
            }
            catch (ObjectDisposedException)
            {
                accepted = null;
            }
            return accepted;
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
