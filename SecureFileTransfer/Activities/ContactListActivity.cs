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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ContactListActivity);

            listAdapter = new ContactsListAdapter(this);

            var listView = FindViewById<ListView>(Resource.Id.ListView);
            listView.Adapter = listAdapter;
            listAdapter.ListView = listView;
        }
    }
}