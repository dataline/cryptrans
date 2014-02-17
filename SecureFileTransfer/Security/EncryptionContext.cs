using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using SecureFileTransfer.Network;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SecureFileTransfer.Security
{
    public class EncryptionContext
    {
        public Connection Connection { get; private set; }

        Security.RSA rsa;
        Security.AES aes;
        RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();

        public EncryptionContext(Connection conn)
        {
            Connection = conn;
        }

        public EncryptionContext(Connection conn, byte[] aesKey, byte[] aesIvec)
            : this(conn)
        {
            aes = new Security.AES(aesKey, aesIvec);
        }

        public EncryptionContext(Connection conn, AES aes)
            : this(conn)
        {
            this.aes = aes;
        }

        public enum ConnectionType
        {
            Server, Client
        }

        public void PerformEncryptionHandshake(ConnectionType type)
        {
            if (type == ConnectionType.Client)
                PerformEncryptionHandshakeClient();
            else
                PerformEncryptionHandshakeServer();
        }

        void PerformEncryptionHandshakeServer()
        {
            rsa = Security.KeyProvider.GetRSA();

            // Send Public Modulus
            Connection.WriteRaw(rsa.PublicKey);

            // Receive 64 bytes (padded to RSA key size) containing AES Key and Ivec
            byte[] answer = new byte[Security.RSA.KeySize];
            Connection.GetRaw(answer);

            byte[] decryptedKeyInfo = rsa.Decrypt(answer);

            byte[] aeskey = decryptedKeyInfo.Take(Security.AES.KeySize).ToArray();
            byte[] aesivec = decryptedKeyInfo.Skip(Security.AES.KeySize).Take(Security.AES.BlockSize).ToArray();

            aes = new Security.AES(aeskey, aesivec);
        }

        void PerformEncryptionHandshakeClient()
        {
            byte[] pubkey = new byte[Security.RSA.KeySize];
            Connection.GetRaw(pubkey);

            rsa = new Security.RSA(pubkey);

            aes = Security.KeyProvider.GetAES();

            byte[] aesAndIV = new byte[Security.AES.KeySize + Security.AES.BlockSize];

            Array.Copy(aes.aesKey, 0, aesAndIV, 0, Security.AES.KeySize);
            Array.Copy(aes.aesIV, 0, aesAndIV, Security.AES.KeySize, Security.AES.BlockSize);

            Connection.WriteRaw(rsa.Encrypt(aesAndIV));
        }

        public void WriteEncrypted(byte[] buf, bool forceNullTermination = false)
        {
            Connection.WriteRaw(aes.Encrypt(Security.Padding.GetSecurelyPaddedData(buf, Security.AES.BlockSize, forceNullTermination)));
        }

        public void GetEncrypted(byte[] buf)
        {
            int toRead = buf.Length;
            byte[] block = new byte[Security.AES.BlockSize];
            while (toRead > 0)
            {
                Connection.GetRaw(block);
                byte[] data = aes.Decrypt(block);
                int validLen = toRead > data.Length ? data.Length : toRead;

                Array.Copy(data, 0, buf, buf.Length - toRead, validLen);

                toRead -= validLen;
            }
        }

        /// <summary>
        /// Hierfür müssen die übertragenen Daten null-terminiert sein und dürfen selber kein Null-Byte enthalten.
        /// </summary>
        /// <returns></returns>
        public byte[] GetEncryptedUndefinedLength()
        {
            List<byte[]> blocks = new List<byte[]>();
            byte[] block = new byte[Security.AES.BlockSize];
            while (true)
            {
                Connection.GetRaw(block);
                byte[] data = Security.Padding.RemovePaddingFromData(aes.Decrypt(block));
                if (data.Length > 0)
                    blocks.Add(data);
                if (data.Length < Security.AES.BlockSize)
                    break;
            }
            return blocks.Aggregate((a, b) => a.Concat(b).ToArray());
        }
    }
}
