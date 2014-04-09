using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SecureFileTransfer.Security;
using System.Threading;

namespace SecureFileTransfer.Network
{
    public abstract class Connection : IDisposable, TrivialEntityBasedProtocol.IConnection
    {
        const int MicroSecond = 1000000;

        public Socket ConnectionSocket { get; protected set; }


        public delegate void DisconnectedEventHandler();
        public event DisconnectedEventHandler Disconnected;

        public SynchronizationContext UIThreadSyncContext { get; set; }

        protected void RaiseDisconnected()
        {
            UIThreadSyncContext.Send(new SendOrPostCallback(state =>
            {
                if (Disconnected != null)
                    Disconnected();
            }), null);
        }

        protected Encoding Encoding = System.Text.Encoding.UTF8;

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

        /// <summary>
        /// Enables encryption for this connection
        /// </summary>
        /// <param name="type"></param>
        public void EnableEncryption(EncryptionContext.ConnectionType type)
        {
            encCtx = new EncryptionContext(this);
            encCtx.PerformEncryptionHandshake(type);
        }

        /// <summary>
        /// Get raw bytes from the connection
        /// </summary>
        /// <param name="buf"></param>
        public void GetRaw(byte[] buf)
        {
            int written = 0;
            while (written < buf.Length)
            {
                written += ConnectionSocket.Receive(buf, written, buf.Length - written, SocketFlags.None);
            }
        }

        /// <summary>
        /// Write raw bytes to the connection
        /// </summary>
        /// <param name="buf"></param>
        public void WriteRaw(byte[] buf)
        {
            ConnectionSocket.Send(buf);
        }

        /// <summary>
        /// Write raw bytes to the connection
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteRaw(string str)
        {
            WriteRaw(Encoding.GetBytes(str));
        }

        /// <summary>
        /// Get bytes from the connection.
        /// This uses encryption if it is enabled.
        /// </summary>
        /// <param name="buf"></param>
        public void Get(byte[] buf)
        {
            if (encCtx != null)
                encCtx.GetEncrypted(buf);
            else
                GetRaw(buf);
        }

        /// <summary>
        /// Get bytes with undefined length from the connection.
        /// This requires encryption to be enabled as the data length is determined
        /// from the padding.
        /// </summary>
        /// <returns></returns>
        public byte[] GetUndefinedLength()
        {
            if (encCtx == null)
                throw new NotSupportedException("Getting undefined lengths is only available after encryption was enabled.");
            return encCtx.GetEncryptedUndefinedLength();
        }

        /// <summary>
        /// Get bytes with undefined length from the connection.
        /// This requires encryption to be enabled as the data length is determined
        /// from the padding.
        /// </summary>
        /// <returns>A string with undefined length</returns>
        public string GetUndefinedLengthString()
        {
            return Encoding.GetString(GetUndefinedLength());
        }

        /// <summary>
        /// Writes bytes to the connection.
        /// This uses encryption if it is enabled.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="forceNullTermination">
        /// This will pad the data so that the receiver is able to determine its exact
        /// length. This is required for the undefined length methods to work.
        /// </param>
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

        /// <summary>
        /// Writes bytes to the connection.
        /// This uses encryption if it is enabled.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="forceNullTermination">
        /// This will pad the data so that the receiver is able to determine its exact
        /// length. This is required for the undefined length methods to work.
        /// </param>
        public void Write(string str, bool forceNullTermination = false)
        {
            Write(Encoding.GetBytes(str), forceNullTermination);
        }

        /// <summary>
        /// Writes the server information to the connection
        /// </summary>
        /// <param name="si"></param>
        public void Write(ServerInformation si)
        {
            byte[] version = BitConverter.GetBytes(si.version);
            byte[] flags = BitConverter.GetBytes(si.flags);
            Write(version);
            Write(flags);
        }

        /// <summary>
        /// Writes a single encrypted block without validation and padding (much faster than Write)
        /// </summary>
        /// <param name="singleBlock">Block to write (has to be of length Security.AES.BlockSize)</param>
        /// <param name="tempStorage">Temporary storage that should be reused (same size as block)</param>
        public void WriteSingleBlockFast(byte[] singleBlock, byte[] tempStorage)
        {
            encCtx.WriteEncryptedSingleBlockFast(singleBlock, tempStorage);
        }

        /// <summary>
        /// Gets a single encrypted block without validation and padding removal (much faster than Get)
        /// </summary>
        /// <param name="singleBlock">Destination (has to be of length Security.AES.BlockSize)</param>
        /// <param name="tempStorage">Temporary storage that should be reused (same size as block)</param>
        public void GetSingleBlockFast(byte[] singleBlock, byte[] tempStorage)
        {
            encCtx.GetEncryptedSingleBlockFast(singleBlock, tempStorage);
        }

        /// <summary>
        /// Sends an accept signal (non-TEBP)
        /// </summary>
        public void SendAccept()
        {
            Write(CMD_OK);
        }

        /// <summary>
        /// Sends a decline signal (non-TEBP)
        /// </summary>
        public void SendDecline()
        {
            Write(CMD_DECLINE);
        }

        /// <summary>
        /// Sends a shutdown signal (non-TEBP)
        /// </summary>
        public void SendShutdown()
        {
            Write(CMD_SHUTDOWN);
        }

        /// <summary>
        /// Gets a signal from the connection and checks for an accept signal
        /// </summary>
        /// <returns>Returns whether the client has sent an accept signal</returns>
        public virtual bool DoesAccept()
        {
            byte[] answer = new byte[2];
            Get(answer);
            return Encoding.GetString(answer) == CMD_OK;
        }

        /// <summary>
        /// Gets the server information from the connection
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Starts the receive loop
        /// </summary>
        public void BeginReceiving()
        {
            if (Receiving)
                return;
            if (ConnectionSocket == null)
                return;
            Receiving = true;
            InternalBeginReceiving();
        }

        #region IConnection

        // Implementation of the IConnection interface

        public void Send(string str)
        {
            Write(str, true);
        }

        public string Receive()
        {
            return GetUndefinedLengthString();
        }

        public bool CanSend()
        {
            if (ConnectionSocket == null ||
                !ConnectionSocket.Connected)
                return false;
            return ConnectionSocket.Poll(10 * MicroSecond, SelectMode.SelectWrite);
        }

        public abstract void Shutdown();

        #endregion

        public virtual void Dispose()
        {
            if (ConnectionSocket != null)
                ConnectionSocket.Close();
            if (encCtx != null)
                encCtx.Dispose();
        }

    }

    public class InvalidHandshakeException : Exception
    {
        public enum HandshakePhase
        {
            InitialHello, VersionExchange, FlagExchange, Authentication, RSAExchange, AESExchange, EncryptionChannelTest, End
        }

        public HandshakePhase Phase { get; set; }

        public InvalidHandshakeException(HandshakePhase phase)
        {
            Phase = phase;
        }
    }
}
