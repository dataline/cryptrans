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
    }
}
