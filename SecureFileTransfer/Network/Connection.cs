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


        public delegate void DisconnectedEventHandler();
        public event DisconnectedEventHandler Disconnected;

        protected void RaiseDisconnected()
        {
            if (Disconnected != null)
                Disconnected();
        }

        protected Encoding ASCII = new ASCIIEncoding();

        protected EncryptionContext encCtx = null;

        public const string CMD_OK = "OK";
        public const string CMD_DECLINE = "..";
        public const string CMD_SHUTDOWN = "qq";

        public const string CMD_CONN_MAGIC = "DLP2P";

        public const Int16 CurrentVersion = 1;

        private bool Receiving = false;


        public string RemoteName { get; set; }

        public struct ServerInformation
        {
            public Int16 version;
            public Int32 flags;
        }

        public Connection() { }
        public Connection(Socket sock)
        {
            ConnectionSocket = sock;
        }

        public abstract bool DoInitialHandshake();

        protected abstract void InternalBeginReceiving();

        protected ServerInformation CreateServerInformation()
        {
            return new ServerInformation()
            {
                version = CurrentVersion,
                flags = 0
            };
        }

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

        public void SendAccept()
        {
            Write(CMD_OK);
        }

        public void SendDecline()
        {
            Write(CMD_DECLINE);
        }

        public void SendShutdown()
        {
            Write(CMD_SHUTDOWN);
        }

        public bool DoesAccept()
        {
            byte[] answer = new byte[2];
            Get(answer);
            return ASCII.GetString(answer) == CMD_OK;
        }

        public byte[] GetUndefinedLength()
        {
            if (encCtx == null)
                throw new NotSupportedException("Getting undefined lengths is only available after encryption was enabled.");
            return encCtx.GetEncryptedUndefinedLength();
        }

        public string GetUndefinedLengthString()
        {
            return ASCII.GetString(GetUndefinedLength());
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

        public void Write(ServerInformation si)
        {
            byte[] version = BitConverter.GetBytes(si.version);
            byte[] flags = BitConverter.GetBytes(si.flags);
            Write(version);
            Write(flags);
        }

        public ServerInformation GetServerInformation()
        {
            byte[] version = new byte[2];
            byte[] flags = new byte[4];
            Get(version);
            Get(flags);

            return new ServerInformation()
            {
                version = BitConverter.ToInt16(version, 0),
                flags = BitConverter.ToInt32(flags, 0)
            };
        }

        public void BeginReceiving()
        {
            if (Receiving)
                return;
            Receiving = true;
            InternalBeginReceiving();
        }

        public void Close()
        {
            ConnectionSocket.Close();
        }

        public virtual void Dispose()
        {
            ConnectionSocket.Close();
        }

    }
}
