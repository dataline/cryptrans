using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

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