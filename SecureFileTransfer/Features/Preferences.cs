using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public static class Preferences
    {
        public static readonly string PrefInfoActivityShown = "InfoActivityShown";


        public static bool GetBool(ISharedPreferences prefs, string key, bool def = false)
        {
            return prefs.GetBoolean(key, def);
        }

        public static void SetBool(ISharedPreferences prefs, string key, bool val)
        {
            prefs.Edit().PutBoolean(key, val).Commit();
        }
    }
}
