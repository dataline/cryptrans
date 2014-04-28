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
using Android.Webkit;

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
                fileName = GetFixedFileName(cursor.GetString(0), contentResolver.GetType(uri));
            }
        }

        private static string GetFixedFileName(string rawName, string type)
        {
            if (Path.GetExtension(rawName).Length == 0)
            {
                foreach (char inval in Path.GetInvalidFileNameChars())
                    rawName = rawName.Replace(inval, '_');
                rawName = rawName.Replace(' ', '_');

                rawName += "." + GetExtensionForType(type);
            }

            return rawName;
        }

        private static string GetExtensionForType(string type)
        {
            if (type == "audio/mp4")
                return "m4a"; // ?!

            return MimeTypeMap.Singleton.GetExtensionFromMimeType(type);
        }

        public static Stream GetInputStreamFromContentURI(this Android.Net.Uri uri, ContentResolver contentResolver)
        {
            return contentResolver.OpenInputStream(uri);
        }

        public static bool IsImage(this Android.Net.Uri uri)
        { 
            string ext = MimeTypeMap.GetFileExtensionFromUrl(uri.Path);
            string mtype;
            return (ext != null && (mtype = MimeTypeMap.Singleton.GetMimeTypeFromExtension(ext)) != null &&
                mtype.StartsWith("image/"));
        }

        public static bool IsImage(this string path)
        { 
            string mtype;
            return (mtype = Java.Net.URLConnection.GuessContentTypeFromName(path)) != null && mtype.StartsWith("image/");
        }
    }
}
