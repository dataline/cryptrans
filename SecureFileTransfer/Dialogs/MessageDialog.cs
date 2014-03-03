using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Dialogs
{
    public abstract class MessageDialog : AndroidDialog
    {
        public string Message { get; set; }

        public MessageDialog(Activity ctx, string message, DialogDidEndDelegate didEnd)
            : base(ctx, Resource.String.OK, NoValue, didEnd)
        {
            Message = message;
        }

        public MessageDialog(Activity ctx, int messageRefId, DialogDidEndDelegate didEnd)
            : base(ctx, Resource.String.OK, NoValue, didEnd)
        {
            Message = ctx.GetString(messageRefId);
        }

        public override Dialog OnCreateDialog(Android.OS.Bundle savedInstanceState)
        {
            var builder = BuildDialog();

            builder.SetMessage(Message);

            return BuildFinishedDialog(builder);
        }
    }
}
