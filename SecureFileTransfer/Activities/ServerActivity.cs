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

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "@string/ApplicationName")]
    public class ServerActivity : Activity
    {
        ImageView qrContainerView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ServerActivity);

            ActionBar.SetDisplayHomeAsUpEnabled(true);

            qrContainerView = FindViewById<ImageView>(Resource.Id.QRContainer);
            qrContainerView.SetImageBitmap(null);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}