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
using Android.Provider;

namespace SecureFileTransfer.Features
{
    public struct AndroidContact
    {
        public long Id;

        public string DisplayName;

        public Android.Net.Uri PhotoThumbnailUri;
    }

    public static class ContactProvider
    {
        public static IEnumerable<AndroidContact> GetContactList(Activity context)
        {
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, 
                                      ContactsContract.Contacts.InterfaceConsts.DisplayName,
                                      ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri };

            var cursor = context.ManagedQuery(ContactsContract.Contacts.ContentUri, projection, null, null, null);

            if (cursor.MoveToFirst())
            {
                do
                {
                    var c = new AndroidContact();
                    c.Id = cursor.GetLong(0);
                    c.DisplayName = cursor.GetString(1);

                    var uri = cursor.GetString(2);
                    if (uri != null)
                        c.PhotoThumbnailUri = Android.Net.Uri.Parse(uri);

                    yield return c;
                } while (cursor.MoveToNext());
            }
        }
    }
}
