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
using SecureFileTransfer.Adapters;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "@string/SelectContacts")]
    public class ContactListActivity : Activity
    {
        ContactsListAdapter listAdapter;

        public const string IE_RESULT_SELECTED_CONTACT_IDS = "scids";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ContactListActivity);

            ActionBar.SetDisplayHomeAsUpEnabled(true);

            listAdapter = new ContactsListAdapter(this);

            var listView = FindViewById<ListView>(Resource.Id.ListView);
            listView.Adapter = listAdapter;
            listAdapter.ListView = listView;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ContactListMenu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                SetResult(Result.Canceled);
                Finish();
                return true;
            }
            else if (item.ItemId == Resource.Id.Send)
            {
                var contactIds = from c in listAdapter.SelectedContacts
                                 select c.Id;

                Intent resultIntent = new Intent();
                resultIntent.PutExtra(IE_RESULT_SELECTED_CONTACT_IDS, contactIds.ToArray());
                SetResult(Result.Ok, resultIntent);

                Finish();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}