using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public class UnsavedBinaryTransfer : Transfer
    {
        byte[] buffer = null;
        long curPtr = 0;


        public override void AppendData(byte[] buf)
        {
            if (buffer == null)
                buffer = new byte[FileLength];
            if (curPtr + buf.Length > buffer.Length)
                throw new NotSupportedException("Tried to write past end of UnsavedBinaryTransfer buffer.");

            Array.Copy(buf, 0, buffer, curPtr, buf.Length);
            curPtr += buf.Length;
        }

        public override byte[] GetData(int maxLen)
        {
            if (buffer == null)
                throw new NotSupportedException("Tried to read transfer without preparing it.");
            if (curPtr >= buffer.Length)
                throw new NotSupportedException("Tried to read past end of UnsavedBinaryTransfer buffer.");
            int len = maxLen;
            if (curPtr + len > buffer.Length)
                len = buffer.Length - (int)curPtr;

            byte[] buf = new byte[len];

            Array.Copy(buffer, curPtr, buf, 0, len);
            curPtr += len;

            return buf;
        }

        protected override void PrepareForReading()
        {
            Console.WriteLine("Writing " + FileLength + " test bytes.");

            buffer = new byte[FileLength];
        }

        protected override void PrepareForWriting()
        {
        }

        public override void CleanUpAfterWriteAbort()
        {
        }

        public override void Close()
        {
        }
    }
}
