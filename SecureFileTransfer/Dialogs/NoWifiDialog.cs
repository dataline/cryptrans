using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Dialogs
{
    public class NoWifiDialog : AndroidDialog
    {
        public NoWifiDialog(Activity ctx)
            : base(ctx, Resource.String.Retry, Resource.String.OpenWifiSettings)
        { 
        }

        public override Android.App.Dialog OnCreateDialog(Android.OS.Bundle savedInstanceState)
        {
            var builder = BuildDialog();

            builder.SetMessage(Resource.String.NoWifiInfo);

            return BuildFinishedDialog(builder);
        }
    }
}
