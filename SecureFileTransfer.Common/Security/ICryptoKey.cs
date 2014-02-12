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

namespace SecureFileTransfer.Security
{
    public interface ICryptoKey
    {
        void Generate();

        byte[] Encrypt(byte[] buf);
        byte[] Decrypt(byte[] buf);
    }
}