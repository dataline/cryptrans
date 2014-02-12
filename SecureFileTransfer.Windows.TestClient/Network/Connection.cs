using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SecureFileTransfer.Security;

namespace SecureFileTransfer.Network
{
    public abstract class Connection : IDisposable
    {
        public Socket ConnectionSocket { get; protected set; }

        protected Encoding ASCII = new ASCIIEncoding();

        protected EncryptionContext encCtx = null;

        public Connection() { }
        public Connection(Socket sock)
        {
            ConnectionSocket = sock;
        }

        public abstract bool DoInitialHandshake();

        public void EnableEncryption(EncryptionContext.ConnectionType type)
        {
            encCtx = new EncryptionContext(this);
            encCtx.PerformEncryptionHandshake(type);
        }
        public void GetRaw(byte[] buf)
        {
            int written = 0;
            while (written < buf.Length)
            {
                written += ConnectionSocket.Receive(buf, written, buf.Length - written, SocketFlags.None);
            }
        }

        public void WriteRaw(byte[] buf)
        {
            ConnectionSocket.Send(buf);
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
            ConnectionSocket.Close();
        }

        public void Dispose()
        {
            ConnectionSocket.Close();
        }

    }
}
