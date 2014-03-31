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
    [Activity(Label = "@string/About",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class AboutActivity : Activity
    {

        const string URL = "http://dataline.de";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.AboutActivity);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            var pInfo = PackageManager.GetPackageInfo(PackageName, 0);
            FindViewById<TextView>(Resource.Id.VersionInfo).Text =
                string.Format(GetString(Resource.String.VersionFormatStr), pInfo.VersionName);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.AboutMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }
            else if (item.ItemId == Resource.Id.Web)
            {
                NavigateToUrl(URL);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        void NavigateToUrl(string url)
        {
            var browser = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
            try
            {
                StartActivity(browser);
            }
            catch (ActivityNotFoundException)
            {
                Toast.MakeText(this, "No web browser installed.", ToastLength.Long).Show();
            }
        }
    }
}