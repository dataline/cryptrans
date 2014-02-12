using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SecureFileTransfer.Security
{
    public class RNG
    {
        static RandomNumberGenerator Generator = null;

        public static void GetBytes(byte[] dest)
        {
            if (Generator == null)
            {
                Generator = RNGCryptoServiceProvider.Create();
            }
            Generator.GetBytes(dest);
        }
    }
}
