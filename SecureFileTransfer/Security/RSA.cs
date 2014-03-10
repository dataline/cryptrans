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
        public const int ExponentSize = 4;

        public RSA() { }

        public RSA(byte[] pubkey, byte[] exponent)
        {
            RSAParameters rsaParams = new RSAParameters()
            {
                Exponent = GetCorrectExponent(exponent),
                Modulus = pubkey
            };

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

        public byte[] Exponent
        {
            get
            {
                if (rsaParameters == null)
                    return null;
                byte[] rsaExp = rsaParameters.Value.Exponent;
                if (rsaExp.Length > 4)
                    throw new NotSupportedException("RSA exponent is too big. Are you living in the far future?");

                byte[] exp = new byte[ExponentSize];
                Array.Copy(rsaExp, 0, exp, exp.Length - rsaExp.Length, rsaExp.Length);

                return exp;
            }
        }

        private byte[] GetCorrectExponent(byte[] exp)
        {
            var realExp = exp.SkipWhile(b => b == 0x0).ToArray();
            return realExp;
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
