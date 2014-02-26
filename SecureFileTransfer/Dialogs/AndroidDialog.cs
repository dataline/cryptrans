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
    public abstract class AndroidDialog : DialogFragment
    {
        protected Activity context;
        protected int dlgYesResID, dlgNoResID;

        protected const int NoValue = -1;

        public enum AndroidDialogResult
        {
            Yes, No
        }

        public AndroidDialog(Activity ctx, int yesResId, int noResId)
        {
            context = ctx;
            dlgYesResID = yesResId;
            dlgNoResID = noResId;
        }

        protected AlertDialog.Builder BuildDialog(Action positiveAction, Action negativeAction)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);

            if (dlgYesResID != NoValue)
                builder.SetPositiveButton(dlgYesResID, (s, e) => positiveAction());
            if (dlgNoResID != NoValue)
                builder.SetNegativeButton(dlgNoResID, (s, e) => negativeAction());

            return builder;
        }

        public virtual void Show(string tag)
        {
            Show(context.FragmentManager, tag);
        }
    }
}
