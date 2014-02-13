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
        public static LocalServer Instance { get; private set; }

        public static string PublicConnectionPassword = Security.PasswordGenerator.Generate(8);

        public static async Task<LocalServerConnection> WaitForConnectionAsync()
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
        }

        public int MaxConnections { get; set; }

        Socket sock = null;

        public const int Port = 23956;

        private LocalServer() : this(1) { }
        private LocalServer(int maxConnections)
        {
            MaxConnections = maxConnections;
        }

        void EstablishSocket()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress addr = host.AddressList[0];
            IPEndPoint local = new IPEndPoint(addr, Port);

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(local);
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
        }
    }
}
