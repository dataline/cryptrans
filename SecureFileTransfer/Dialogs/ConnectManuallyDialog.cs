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


namespace SecureFileTransfer.Dialogs
{
    public class ConnectManuallyDialog : AndroidDialog
    {
        public delegate void ConnectManuallyDialogDidEndDelegate(AndroidDialogResult res, string ipAddress, string password);

        ConnectManuallyDialogDidEndDelegate dlgDidEndDelegate;


        public ConnectManuallyDialog(Activity ctx, int yesResId, int noResId, ConnectManuallyDialogDidEndDelegate d)
            : base(ctx, yesResId, noResId)
        {
            dlgDidEndDelegate = d;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var view = context.LayoutInflater.Inflate(Resource.Layout.ConnectManuallyDialog, null);

            var ipField = view.FindViewById<TextView>(Resource.Id.IPAddressField);
            var passField = view.FindViewById<TextView>(Resource.Id.PasswordField);

            var builder = BuildDialog(
                () => dlgDidEndDelegate(AndroidDialogResult.Yes, ipField.Text, passField.Text),
                () => dlgDidEndDelegate(AndroidDialogResult.No, ipField.Text, passField.Text)
                );

            builder.SetView(view);

            return builder.Create();
        }
    }
}
