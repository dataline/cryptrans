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
using System.IO;

namespace SecureFileTransfer.Features
{
    public static class AndroidURIResolver
    {
        public static void GetMetadataFromContentURI(this Android.Net.Uri uri, ContentResolver contentResolver, out long size, out string fileName)
        {
            using (var fd = contentResolver.OpenFileDescriptor(uri, "r"))
                size = fd.StatSize;
            using (var cursor = contentResolver.Query(uri, new string[] {
                Android.Provider.OpenableColumns.DisplayName 
            }, null, null, null))
            {
                cursor.MoveToFirst();
                fileName = cursor.GetString(0);
            }
        }

        public static Stream GetInputStreamFromContentURI(this Android.Net.Uri uri, ContentResolver contentResolver)
        {
            return contentResolver.OpenInputStream(uri);
        }
    }
}
