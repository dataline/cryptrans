using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Dialogs
{
    public class ConnectionFailedDialog : AndroidDialog
    {
        public ConnectionFailedDialog(Activity ctx, DialogDidEndDelegate didEnd)
            : base(ctx, Resource.String.OK, Resource.String.OpenHotspotSettings, didEnd)
        { }

        public override Dialog OnCreateDialog(Android.OS.Bundle savedInstanceState)
        {
            var builder = BuildDialog();

            builder.SetMessage(Resource.String.ErrEstablishClient);

            return BuildFinishedDialog(builder);
        }
    }
}
