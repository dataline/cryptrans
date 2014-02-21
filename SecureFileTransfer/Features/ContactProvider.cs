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
using Android.Database;
using Newtonsoft.Json;

namespace SecureFileTransfer.Features
{
    public struct AndroidContactDataEntry
    {
        public string Data;
        public string Type;

        public AndroidContactDataEntry(string data, string type)
        {
            Data = data;
            Type = type;
        }
    }
    public struct AndroidContact
    {
        [JsonIgnore]
        public long Id;
        [JsonIgnore]
        public Android.Net.Uri PhotoThumbnailUri;

        public string DisplayName;
        public string DisplayNameAlternative;

        public AndroidContactDataEntry[] PhoneNumbers;
        public AndroidContactDataEntry[] EmailAddresses;
        public AndroidContactDataEntry[] PostalAddresses;
    }

    public static class ContactProvider
    {
        public static IEnumerable<AndroidContact> GetContactList(Activity context)
        {
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, 
                                      ContactsContract.Contacts.InterfaceConsts.DisplayName,
                                      ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri };

            var loader = new CursorLoader(context, ContactsContract.Contacts.ContentUri, projection, null, null, null);
            var cursor = (ICursor)loader.LoadInBackground();

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

            cursor.Close();
        }

        public static IEnumerable<Tuple<Android.Net.Uri, string>> GetVcardUrisFromContactIds(Activity context, long[] ids)
        {
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id,
                                      ContactsContract.Contacts.InterfaceConsts.LookupKey, 
                                      ContactsContract.Contacts.InterfaceConsts.DisplayName };

            var loader = new CursorLoader(context, ContactsContract.Contacts.ContentUri, projection, null, null, null);
            var cursor = (ICursor)loader.LoadInBackground();

            if (cursor.MoveToFirst())
            {
                do
                {
                    if (ids.Contains(cursor.GetLong(0)))
                    {
                        yield return new Tuple<Android.Net.Uri, string>(
                            Android.Net.Uri.WithAppendedPath(ContactsContract.Contacts.ContentVcardUri, cursor.GetString(1)),
                            cursor.GetString(2));
                    }
                } while (cursor.MoveToNext());
            }

            cursor.Close();
        }


        public static AndroidContact GetContactInformation(Context ctx, string contactId)
        {
            var contact = new AndroidContact();

            var generalProjection = new string[] { ContactsContract.Contacts.InterfaceConsts.Id,
                                                    ContactsContract.Contacts.InterfaceConsts.DisplayName,
                                                    ContactsContract.Contacts.InterfaceConsts.DisplayNameAlternative,
                                                    ContactsContract.Contacts.InterfaceConsts.PhotoUri };
            var phoneProjection = new string[] { ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId,
                                                    ContactsContract.CommonDataKinds.Phone.Number,
                                                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type };
            var emailProjection = new string[] { ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId,
                                                    ContactsContract.CommonDataKinds.Email.Address,
                                                    ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type };
            var addressProjection = new string[] { ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Id,
                                                    ContactsContract.CommonDataKinds.StructuredPostal.FormattedAddress,
                                                    ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type };

            var generalCursor =
                (ICursor)new CursorLoader(ctx, ContactsContract.Contacts.ContentUri,
                    generalProjection,
                    ContactsContract.Contacts.InterfaceConsts.Id + " = ?",
                    new string[] { contactId },
                    null).LoadInBackground();
            var phoneCursor =
                (ICursor)new CursorLoader(ctx, ContactsContract.CommonDataKinds.Phone.ContentUri,
                    phoneProjection,
                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + " = ?",
                    new string[] { contactId },
                    null).LoadInBackground();
            var emailCursor =
                (ICursor)new CursorLoader(ctx, ContactsContract.CommonDataKinds.Email.ContentUri,
                    emailProjection,
                    ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId + " = ?",
                    new string[] { contactId },
                    null).LoadInBackground();
            var addressCursor =
                (ICursor)new CursorLoader(ctx, ContactsContract.CommonDataKinds.StructuredPostal.ContentUri,
                    addressProjection,
                    ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.ContactId + " = ?",
                    new string[] { contactId },
                    null).LoadInBackground();

            if (!generalCursor.MoveToFirst() || !generalCursor.IsLast)
                throw new NotSupportedException("Could not find contact or found multiple instances.");

            contact.DisplayName = generalCursor.GetString(1);
            contact.DisplayNameAlternative = generalCursor.GetString(2);
            //TODO: photo

            List<AndroidContactDataEntry> phoneNumbers = new List<AndroidContactDataEntry>();
            List<AndroidContactDataEntry> emailAddresses = new List<AndroidContactDataEntry>();
            List<AndroidContactDataEntry> postalAddresses = new List<AndroidContactDataEntry>();

            if (phoneCursor.MoveToFirst())
            {
                do
                {
                    phoneNumbers.Add(new AndroidContactDataEntry(phoneCursor.GetString(1), phoneCursor.GetString(2)));
                } while (phoneCursor.MoveToNext());
            }
            if (emailCursor.MoveToFirst())
            {
                do
                {
                    emailAddresses.Add(new AndroidContactDataEntry(emailCursor.GetString(1), emailCursor.GetString(2)));
                } while (emailCursor.MoveToNext());
            }
            if (addressCursor.MoveToFirst())
            {
                do
                {
                    postalAddresses.Add(new AndroidContactDataEntry(addressCursor.GetString(1), addressCursor.GetString(2)));
                } while (addressCursor.MoveToNext());
            }

            if (phoneNumbers.Count > 0)
                contact.PhoneNumbers = phoneNumbers.ToArray();
            if (emailAddresses.Count > 0)
                contact.EmailAddresses = emailAddresses.ToArray();
            if (postalAddresses.Count > 0)
                contact.PostalAddresses = postalAddresses.ToArray();

            generalCursor.Close();
            phoneCursor.Close();
            emailCursor.Close();
            addressCursor.Close();

            return contact;
        }
    }
}
