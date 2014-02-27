using Android.App;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Activities
{
    public static class ActivityExtensions
    {
        public static void ShowToast(this Activity ac, int resId)
        {
            Toast.MakeText(ac, resId, ToastLength.Long).Show();
        }
        public static void ShowToast(this Activity ac, string message)
        {
            Toast.MakeText(ac, message, ToastLength.Long).Show();
        }
    }
}
