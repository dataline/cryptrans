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
            var cursor = contentResolver.Query(uri, new string[] { Android.Provider.OpenableColumns.Size, Android.Provider.OpenableColumns.DisplayName }, null, null, null);
            cursor.MoveToFirst();
            size = cursor.GetLong(0);
            fileName = cursor.GetString(1);
            cursor.Close();
        }

        public static Stream GetInputStreamFromContentURI(this Android.Net.Uri uri, ContentResolver contentResolver)
        {
            return contentResolver.OpenInputStream(uri);
        }
    }
}
