using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features.Transfers
{
    public class UnsavedBinaryTransfer : Transfer
    {
        protected byte[] buffer = null;
        long curPtr = 0;


        public override void AppendData(byte[] buf, int length)
        {
            if (buffer == null)
                buffer = new byte[FileLength];
            if (curPtr + length > buffer.Length)
                throw new NotSupportedException("Tried to write past end of UnsavedBinaryTransfer buffer.");
            Array.Copy(buf, 0, buffer, curPtr, length);
            curPtr += length;
        }

        public override int GetData(byte[] buf)
        {
            if (buffer == null)
                throw new NotSupportedException("Tried to read transfer without preparing it.");
            if (curPtr >= buffer.Length)
                throw new NotSupportedException("Tried to read past end of UnsavedBinaryTransfer buffer.");

            int len = buf.Length;
            if (curPtr + len > buffer.Length)
                len = buffer.Length - (int)curPtr;

            Array.Copy(buffer, curPtr, buf, 0, len);
            curPtr += len;

            return len;
        }

        protected override void PrepareForReading()
        {
            if (buffer == null)
            {
                // TEST BYTES

                Console.WriteLine("Writing " + FileLength + " test bytes.");

                buffer = new byte[FileLength];
            }
        }

        protected override void PrepareForWriting()
        {
        }

        public override void WriteAborted()
        {
        }

        public override void WriteSucceeded()
        {
        }

        public override void Close()
        {
        }
    }
}
