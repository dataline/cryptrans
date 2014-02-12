using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Security
{
    public interface ICryptoKey
    {
        void Generate();

        byte[] Encrypt(byte[] buf);
        byte[] Decrypt(byte[] buf);
    }
}