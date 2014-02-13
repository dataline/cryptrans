using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SecureFileTransfer.Network
{
    public class ClientConnection : Connection
    {
        public string ConnectionPassword { get; set; }

        public static ClientConnection ConnectTo(string hostName, int port, string connectionPassword)
        {
            ClientConnection c = new ClientConnection();
            c.ConnectionPassword = connectionPassword;
            c.Connect(hostName, port);

            if (!c.DoInitialHandshake())
                return null;

            return c;
        }

        public static async Task<ClientConnection> ConnectToAsync(string hostName, int port, string connectionPassword)
        {
            return await Task.Run<ClientConnection>(() =>
            {
                return ConnectTo(hostName, port, connectionPassword);
            });
        }

        private ClientConnection() { }

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
            if (ASCII.GetString(hello) != "DLP2P")
                return false;

            WriteRaw("OK");

            EnableEncryption(Security.EncryptionContext.ConnectionType.Client);

            Write(ConnectionPassword, true);

            byte[] ok = new byte[2];
            Get(ok);
            return ASCII.GetString(ok) == "OK";
        }
    }
}
