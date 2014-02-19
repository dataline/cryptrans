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
    public class YesNoDialog : DialogFragment
    {
        Activity context;
        string dlgMessage;
        int dlgYesResID, dlgNoResID;

        DialogDidEndDelegate dlgDidEndDelegate;

        public enum AndroidDialogResult
        {
            Yes, No
        }

        public delegate void DialogDidEndDelegate(AndroidDialogResult dialogResult);

        public YesNoDialog(Activity activity, string message, int yesResID, int noResID, DialogDidEndDelegate didEndDelegate)
            : base()
        {
            context = activity;
            dlgMessage = message;
            dlgYesResID = yesResID;
            dlgNoResID = noResID;
            dlgDidEndDelegate = didEndDelegate;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);

            builder.SetMessage(dlgMessage);
            builder.SetPositiveButton(dlgYesResID, (s, e) => dlgDidEndDelegate(AndroidDialogResult.Yes));
            builder.SetNegativeButton(dlgNoResID, (s, e) => dlgDidEndDelegate(AndroidDialogResult.No));
            return builder.Create();
        }

        public void Show(string tag)
        {
            Show(context.FragmentManager, tag);
        }
    }
}
