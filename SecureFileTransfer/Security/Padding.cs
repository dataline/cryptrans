using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Security
{
    /// <summary>
    /// This is the cryptrans padding algorithm.
    /// 
    /// As we know the size of encrypted AES blocks at all time (using null-termination
    /// or because it is sent in the transfer request), we do not use standard padding
    /// but instead pad with random data.
    /// </summary>
    public class Padding
    {
        public static byte[] GetSecurelyPaddedData(byte[] buf, int blockSize, bool forceNullTermination = false)
        {
            int lastBlockOverflowBytes = buf.Length % blockSize;
            if (lastBlockOverflowBytes == 0 && !forceNullTermination)
                return buf;

            int toPad = blockSize - lastBlockOverflowBytes;
            byte[] result = new byte[buf.Length + toPad];

            Array.Copy(buf, result, buf.Length);

            // End Data with null byte
            result[buf.Length] = 0;
            toPad--;

            if (toPad > 0)
            {
                byte[] randomFilling = new byte[toPad];
                RNG.GetBytes(randomFilling);
                
                Array.Copy(randomFilling, 0, result, buf.Length + 1, toPad);
            }

            return result;
        }

        public static void SecurelyPadBufferFromPosition(byte[] buf, int startPosition)
        {
            int padLen = buf.Length - startPosition;
            byte[] randomFilling = new byte[padLen];

            RNG.GetBytes(randomFilling);

            Array.Copy(randomFilling, 0, buf, startPosition, padLen);
        }

        public static byte[] RemovePaddingFromData(byte[] buf)
        {
            int validDataLen = 0;
            while (validDataLen < buf.Length && buf[validDataLen] != 0)
            {
                validDataLen++;
            }
            byte[] result = new byte[validDataLen];

            Array.Copy(buf, result, validDataLen);

            return result;
        }
    }
}
