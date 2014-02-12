using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.Security;

namespace SecureFileTransfer.Network
{
    public class LocalServerConnection : IDisposable, IOpenConnection
    {
        public Socket Connection { get; private set; }

        private Encoding ASCII = new ASCIIEncoding();

        private EncryptionContext encCtx = null;

        public LocalServerConnection(Socket sock)
        {
            Connection = sock;
        }

        public bool DoInitialHandshake()
        {
            Write("DLP2P");
            byte[] answer = new byte[2];
            Get(answer);
            if (ASCII.GetString(answer) != "OK")
                return false;

            EnableEncryption();

            Write("OK");

            return true;
        }

        public void EnableEncryption()
        {
            encCtx = new EncryptionContext(this);
            encCtx.PerformEncryptionHandshake(EncryptionContext.ConnectionType.Server);
        }

        public void GetRaw(byte[] buf)
        {
            int written = 0;
            while (written < buf.Length)
            {
                written += Connection.Receive(buf, written, buf.Length - written, SocketFlags.None);
            }
        }

        public void WriteRaw(byte[] buf)
        {
            Connection.Send(buf);
        }

        public void WriteRaw(string str)
        {
            WriteRaw(ASCII.GetBytes(str));
        }

        public void Get(byte[] buf)
        {
            if (encCtx != null)
                encCtx.GetEncrypted(buf);
            else
                GetRaw(buf);
        }

        public void Write(byte[] buf)
        {
            if (encCtx != null)
                encCtx.WriteEncrypted(buf);
            else
                WriteRaw(buf);
        }

        public void Write(string str)
        {
            Write(ASCII.GetBytes(str));
        }

        public void Close()
        {
            Connection.Close();
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
