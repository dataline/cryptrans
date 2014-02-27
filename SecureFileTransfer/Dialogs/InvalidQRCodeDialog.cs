using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Dialogs
{
    public class InvalidQRCodeDialog : AndroidDialog
    {
        public InvalidQRCodeDialog(Activity ctx, DialogDidEndDelegate didEnd)
            : base(ctx, Resource.String.OK, NoValue, didEnd)
        { }

        public override Dialog OnCreateDialog(Android.OS.Bundle savedInstanceState)
        {
            var builder = BuildDialog();

            builder.SetMessage(Resource.String.QRCodeInvalid);

            return BuildFinishedDialog(builder);
        }
    }
}
