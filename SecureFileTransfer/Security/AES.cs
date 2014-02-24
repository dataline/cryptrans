using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SecureFileTransfer.Security
{
    public class AES : ICryptoKey
    {
        public const int KeySize = 32; // 256 bits
        public const int BlockSize = 16; // 128 bits

        ICryptoTransform encryptor;
        ICryptoTransform decryptor;

        public byte[] aesKey;
        public byte[] aesIV;

        public AES() { }

        public AES(byte[] key, byte[] iv)
        {
            Initialize(key, iv);
        }

        public void Generate()
        {
            AesManaged aes = new AesManaged()
            {
                KeySize = KeySize * 8,
                BlockSize = BlockSize * 8,
                Padding = PaddingMode.None
            };
            encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            aesKey = aes.Key;
            aesIV = aes.IV;
        }

        public void Initialize(byte[] key, byte[] iv)
        {
            aesKey = key;
            aesIV = iv;

            AesManaged aes = new AesManaged()
            {
                KeySize = KeySize * 8,
                BlockSize = BlockSize * 8,
                Padding = PaddingMode.None
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

        public void Encrypt(byte[] bufIn, byte[] bufOut)
        {
            TransformBlocksFast(bufIn, bufOut, encryptor);
        }

        public void Decrypt(byte[] bufIn, byte[] bufOut)
        {
            TransformBlocksFast(bufIn, bufOut, decryptor);
        }

        byte[] TransformBlocks(byte[] buf, ICryptoTransform transformer)
        {
            if (buf.Length % BlockSize != 0)
                throw new Exception("Buffer is not divisible by Key Size.");
            byte[] output = new byte[buf.Length];

            TransformBlocksFast(buf, output, transformer);

            return output;
        }

        void TransformBlocksFast(byte[] bufIn, byte[] bufOut, ICryptoTransform transformer)
        {
            for (int i = 0; i < bufIn.Length / BlockSize; i++)
                transformer.TransformBlock(bufIn, i * BlockSize, BlockSize, bufOut, i * BlockSize);
        }
    }
}
