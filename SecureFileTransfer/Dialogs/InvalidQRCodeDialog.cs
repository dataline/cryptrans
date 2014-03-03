using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Dialogs
{
    public class InvalidQRCodeDialog : MessageDialog
    {
        public InvalidQRCodeDialog(Activity ctx, DialogDidEndDelegate didEnd)
            : base(ctx, Resource.String.QRCodeInvalid, didEnd)
        { }
    }
}
