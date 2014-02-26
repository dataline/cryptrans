using Android.App;
using Android.Graphics.Drawables;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Dialogs
{
    public class ConnectingDialog : AndroidDialog
    {
        public ConnectingDialog(Activity ctx)
            : base(ctx, NoValue, NoValue)
        { }

        public override Dialog OnCreateDialog(Android.OS.Bundle savedInstanceState)
        {
            var view = context.LayoutInflater.Inflate(Resource.Layout.ConnectingDialog, null);

            var animationView = view.FindViewById<ImageView>(Resource.Id.ImageView);
            animationView.SetImageResource(Resource.Drawable.wlan_anim);

            var builder = BuildDialog(null, null);
            builder.SetView(view);

            var dialog = builder.Create();
            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);

            ((AnimationDrawable)animationView.Drawable).Start();

            return dialog;
        }
    }
}
