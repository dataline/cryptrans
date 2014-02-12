using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SecureFileTransfer.Security
{
    public class RSA : ICryptoKey
    {
        RSACryptoServiceProvider rsaProvider = null;
        RSAParameters? rsaParameters = null;


        public const int KeySize = 128; // 1024 bits

        const int Exponent = 17;

        public RSA() { }

        public RSA(byte[] pubkey)
        {
            RSAParameters rsaParams = new RSAParameters();
            rsaParams.Exponent = new byte[] { (byte)Exponent };
            rsaParams.Modulus = pubkey;

            rsaProvider = new RSACryptoServiceProvider(KeySize * 8);
            rsaProvider.ImportParameters(rsaParams);

            rsaParameters = rsaParams;
        }

        public void Generate()
        {
            Console.WriteLine("Generating RSA Keys ...");
            rsaProvider = new RSACryptoServiceProvider(KeySize * 8);
            rsaParameters = rsaProvider.ExportParameters(true);
            Console.WriteLine("Finished generating RSA Keys.");
        }

        public byte[] PrivateKey
        {
            get
            {
                if (rsaParameters == null)
                    return null;
                return rsaParameters.Value.D;
            }
        }

        public byte[] PublicKey
        {
            get
            {
                if (rsaParameters == null)
                    return null;
                return rsaParameters.Value.Modulus;
            }
        }

        public byte[] Encrypt(byte[] buf)
        {
            if (rsaProvider == null)
                return null;
            return rsaProvider.Encrypt(buf, true);
        }

        public byte[] Decrypt(byte[] buf)
        {
            if (buf.Length % KeySize != 0)
                throw new Exception("Buffer Length is not divisible by Key Size.");
            if (rsaProvider == null)
                return null;
            return rsaProvider.Decrypt(buf, true);
        }
    }
}
