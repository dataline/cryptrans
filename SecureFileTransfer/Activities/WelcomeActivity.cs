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
using Android.Preferences;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "@string/ApplicationName",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class WelcomeActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.WelcomeActivity);

            FindViewById<Button>(Resource.Id.TapHereToBeginButton).Click += (e, s) =>
            {
                Features.Preferences.SetBool(
                    PreferenceManager.GetDefaultSharedPreferences(this),
                    Features.Preferences.PrefInfoActivityShown,
                    true);

                StartActivity(typeof(HomeActivity));
                Finish();
            };
        }
    }
}