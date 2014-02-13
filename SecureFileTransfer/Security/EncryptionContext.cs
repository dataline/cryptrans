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

            byte[] aeskey = answer.Take(Security.AES.KeySize).ToArray();
            byte[] aesivec = answer.Skip(Security.AES.KeySize).Take(Security.AES.BlockSize).ToArray();

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

        public void WriteEncrypted(byte[] buf)
        {
            Connection.WriteRaw(aes.Encrypt(Security.Padding.GetSecurelyPaddedData(buf, Security.AES.BlockSize)));
        }

        public void GetEncrypted(byte[] buf)
        {
            int toRead = buf.Length;
            byte[] block = new byte[Security.AES.BlockSize];
            while (toRead > 0)
            {
                Connection.GetRaw(block);
                byte[] data = Security.Padding.RemovePaddingFromData(aes.Decrypt(block));
                toRead -= data.Length;
                if (toRead < 0)
                    throw new Exception("Contents of block are larger than expected.");
                Array.Copy(data, 0, buf, buf.Length - toRead, data.Length);
            }
        }
    }
}
