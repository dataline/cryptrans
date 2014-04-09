using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Security
{
    /// <summary>
    /// This generates the connection passwords to ensure the client that scanned
    /// the QR code is the client that has connected to our server.
    /// They are NOT used for any encryption purposes.
    /// </summary>
    public static class PasswordGenerator
    {
        const string PasswordChars = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        static Random rnd = new Random();

        public static string Generate(int len)
        {
            StringBuilder res = new StringBuilder();

            for (int i = 0; i < len; i++)
                res.Append(PasswordChars[rnd.Next(PasswordChars.Length)]);

            return res.ToString();
        }
    }
}
