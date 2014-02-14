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
        public static ClientConnection CurrentConnection { get; private set; }

        public string ConnectionPassword { get; set; }

        public static ClientConnection ConnectTo(string hostName, int port, string connectionPassword)
        {
            if (CurrentConnection != null)
                throw new NotSupportedException("There is already a client connection available.");

            ClientConnection c = new ClientConnection();
            c.ConnectionPassword = connectionPassword;
            c.Connect(hostName, port);

            if (!c.DoInitialHandshake())
                return null;

            CurrentConnection = c;

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
            Write(Android.OS.Build.Model, true);

            byte[] ok = new byte[2];
            Get(ok);
            if (ASCII.GetString(ok) != "OK")
                return false;

            RemoteName = ASCII.GetString(GetUndefinedLength());

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            CurrentConnection = null;
        }
    }
}
