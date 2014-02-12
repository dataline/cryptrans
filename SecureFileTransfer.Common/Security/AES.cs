using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SecureFileTransfer.Security
{
    class AES : ICryptoKey
    {
        public const int KeySize = 32; // 256 bits
        public const int BlockSize = 16; // 128 bits

        ICryptoTransform encryptor;
        ICryptoTransform decryptor;

        public AES() { }

        public AES(byte[] key, byte[] iv)
        {
            Initialize(key, iv);
        }

        public void Generate()
        {
            throw new NotImplementedException();
        }

        public void Initialize(byte[] key, byte[] iv)
        {
            AesManaged aes = new AesManaged()
            {
                KeySize = KeySize * 8,
                BlockSize = BlockSize * 8
            };
            encryptor = aes.CreateEncryptor(key, iv);
            decryptor = aes.CreateDecryptor(key, iv);
        }

        public byte[] Encrypt(byte[] buf)
        {
            return TransformBlocks(buf, encryptor);
        }

        public byte[] Decrypt(byte[] buf)
        {
            return TransformBlocks(buf, decryptor);
        }

        byte[] TransformBlocks(byte[] buf, ICryptoTransform transformer)
        {
            if (buf.Length % BlockSize != 0)
                throw new Exception("Buffer is not divisible by Key Size.");
            int blocks = buf.Length / BlockSize;
            byte[] output = new byte[buf.Length];

            for (int i = 0; i < blocks; i++)
            {
                transformer.TransformBlock(buf, i * BlockSize, BlockSize, output, i * BlockSize);
            }

            return output;
        }
    }
}
