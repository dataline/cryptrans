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

using SecureFileTransfer.Features;
using Android.Text;

namespace SecureFileTransfer.Adapters
{
    public class ContactsListAdapter : CellClickManagingAdapter
    {
        Activity parentActivity;

        public List<AndroidContact> Contacts;

        public List<int> SelectedIndices = new List<int>();

        public ContactsListAdapter(Activity activity)
        {
            parentActivity = activity;

            ReloadContactsList();
        }

        public void ReloadContactsList()
        {
            Contacts = ContactProvider.GetContactList(parentActivity)
                .OrderBy(contact => contact.DisplayName, new FullNameComparer())
                .ToList();
        }

        public IEnumerable<AndroidContact> SelectedContacts
        {
            get
            {
                return from ind in SelectedIndices
                       select Contacts[ind];
            }
        }

        public override int Count
        {
            get { return Contacts.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? parentActivity.LayoutInflater.Inflate(Resource.Layout.ContactItem, null);

            var contact = Contacts[position];

            view.FindViewById<TextView>(Resource.Id.ContactName).SetText(
                Html.FromHtml(contact.DisplayName.HtmlStringWithBoldLastName()),
                TextView.BufferType.Normal);

            var photoView = view.FindViewById<ImageView>(Resource.Id.ContactPhoto);
            if (contact.PhotoThumbnailUri != null)
            {
                photoView.SetImageURI(contact.PhotoThumbnailUri);
                photoView.Visibility = ViewStates.Visible;
            }
            else
            {
                photoView.Visibility = ViewStates.Invisible;
            }

            ReloadChecked(view, position);

            return view;
        }

        void ReloadChecked(View view, int position)
        {
            view.FindViewById<CheckBox>(Resource.Id.CheckBox).Checked = SelectedIndices.Contains(position);
        }

        public override void ListViewItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (SelectedIndices.Contains(e.Position))
                SelectedIndices.Remove(e.Position);
            else
                SelectedIndices.Add(e.Position);

            PerformCellUpdate(e.Position, view => ReloadChecked(view, e.Position));
        }
    }
}
