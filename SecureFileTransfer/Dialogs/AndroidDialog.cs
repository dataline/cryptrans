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

        public delegate void DialogDidEndDelegate(AndroidDialogResult dialogResult);

        public DialogDidEndDelegate DidEnd { get; set; }

        public enum AndroidDialogResult
        {
            Yes, No
        }

        public AndroidDialog(Activity ctx, int yesResId, int noResId, DialogDidEndDelegate didEnd = null)
        {
            context = ctx;
            dlgYesResID = yesResId;
            dlgNoResID = noResId;
            DidEnd = didEnd;
        }

        protected AlertDialog.Builder BuildDialog(Action positiveAction, Action negativeAction)
        {
            this.Cancelable = false;

            AlertDialog.Builder builder = new AlertDialog.Builder(context)
                .SetCancelable(false);

            if (dlgYesResID != NoValue)
                builder.SetPositiveButton(dlgYesResID, (s, e) => positiveAction());
            if (dlgNoResID != NoValue)
                builder.SetNegativeButton(dlgNoResID, (s, e) => negativeAction());

            return builder;
        }

        protected AlertDialog.Builder BuildDialog()
        {
            return BuildDialog(PositiveResultInvocation, NegativeResultInvocation);
        }

        protected AlertDialog BuildFinishedDialog(AlertDialog.Builder builder)
        {
            var dia = builder.Create();

            dia.SetCancelable(false);
            dia.SetCanceledOnTouchOutside(false);

            return dia;
        }

        protected Action PositiveResultInvocation
        {
            get
            {
                return () => DidEnd(AndroidDialogResult.Yes);
            }
        }

        protected Action NegativeResultInvocation
        {
            get
            {
                return () => DidEnd(AndroidDialogResult.No);
            }
        }

        public virtual void Show(string tag)
        {
            var transaction = context.FragmentManager.BeginTransaction();
            transaction.Add(this, tag);
            transaction.CommitAllowingStateLoss();
        }

        public override void Dismiss()
        {
            DismissAllowingStateLoss();
        }

        public void ShowUntil(Func<bool> untilFunc, Action<bool> positiveAction, Action<bool> negativeAction, Action endAction, bool retryOnNegativeAction, string tag)
        {
            DidEnd = res =>
            {
                Dismiss();

                bool ufRes = untilFunc();

                if (res == AndroidDialogResult.Yes && positiveAction != null)
                    positiveAction(ufRes);
                else if (res == AndroidDialogResult.No && negativeAction != null)
                    negativeAction(ufRes);

                if (!ufRes && (res != AndroidDialogResult.No || retryOnNegativeAction))
                {
                    ShowUntil(untilFunc, positiveAction, negativeAction, endAction, retryOnNegativeAction, tag);
                }
                else
                {
                    if (endAction != null)
                        endAction();
                }    
            };

            Show(tag);
        }
    }
}
