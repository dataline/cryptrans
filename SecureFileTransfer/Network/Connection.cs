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

        public byte[] GetUndefinedLength()
        {
            if (encCtx == null)
                throw new NotSupportedException("Getting undefined lengths is only available after encryption was enabled.");
            return encCtx.GetEncryptedUndefinedLength();
        }

        public void Write(byte[] buf, bool forceNullTermination = false)
        {
            if (encCtx != null)
            {
                encCtx.WriteEncrypted(buf, forceNullTermination);
            }
            else
            {
                if (forceNullTermination)
                    throw new NotSupportedException("Null termination is only available after encryption was enabled.");
                WriteRaw(buf);
            }
        }

        public void Write(string str, bool forceNullTermination = false)
        {
            Write(ASCII.GetBytes(str), forceNullTermination);
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
