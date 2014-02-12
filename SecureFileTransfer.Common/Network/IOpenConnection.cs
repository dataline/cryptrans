using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network
{
    public interface IOpenConnection
    {
        void GetRaw(byte[] buf);
        void WriteRaw(byte[] buf);
        void WriteRaw(string str);
        void Get(byte[] buf);
        void Write(byte[] buf);
        void Write(string str);
        void Close();
    }
}