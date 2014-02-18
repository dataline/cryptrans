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

namespace SecureFileTransfer.Features
{
    public static class AndroidURIResolver
    {
        public static string GetFilePathFromContentURI(this Android.Net.Uri uri, ContentResolver contentResolver)
        {
            var cursor = contentResolver.Query(uri, new string[] { Android.Provider.MediaStore.Images.ImageColumns.Data }, null, null, null);
            cursor.MoveToFirst();
            var filepath = cursor.GetString(0);
            cursor.Close();

            return filepath;
        }
    }
}
