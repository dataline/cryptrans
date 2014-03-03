using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecureFileTransfer.Features;

namespace SecureFileTransfer.Dialogs
{
    public class InvalidHandshakeDialog : MessageDialog
    {
        public InvalidHandshakeDialog(Activity ctx, SecureFileTransfer.Network.InvalidHandshakeException ex, DialogDidEndDelegate didEnd)
            : base(ctx, ex.GetDescriptionResource(), didEnd)
        {
        }
    }
}
