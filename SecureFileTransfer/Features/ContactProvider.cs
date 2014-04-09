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
using System.IO;
using Java.IO;

namespace SecureFileTransfer.Features
{
    public class AndroidContactDataEntry
    {
        public string Data;
        public string Type;

        public AndroidContactDataEntry(string data, string type)
        {
            Data = data;
            Type = type;
        }
    }
    public class AndroidContactImEntry : AndroidContactDataEntry
    {
        public string Protocol;
        public string CustomProtocol;

        public AndroidContactImEntry(string data, string type, string protocol, string customProtocol)
            : base(data, type)
        {
            Protocol = protocol;
            CustomProtocol = customProtocol;
        }
    }
    public struct AndroidContact
    {
        [JsonIgnore]
        public string Id;
        [JsonIgnore]
        public Android.Net.Uri PhotoThumbnailUri;

        public string DisplayName;
        public string Nickname;
        public string Note;

        public byte[] Photo;

        public AndroidContactDataEntry[] PhoneNumbers;
        public AndroidContactDataEntry[] EmailAddresses;
        public AndroidContactDataEntry[] PostalAddresses;

        public AndroidContactImEntry[] Ims;
        public AndroidContactDataEntry[] Websites;
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
                    c.Id = cursor.GetString(0);
                    c.DisplayName = cursor.GetString(1);

                    var uri = cursor.GetString(2);
                    if (uri != null)
                        c.PhotoThumbnailUri = Android.Net.Uri.Parse(uri);

                    yield return c;
                } while (cursor.MoveToNext());
            }

            cursor.Close();
        }

        public static string GetContactIDFromLookupKey(Context ctx, string lookupKey)
        {
            var loader = new CursorLoader(ctx, ContactsContract.Contacts.ContentUri,
                new string[] { ContactsContract.Contacts.InterfaceConsts.Id },
                ContactsContract.Contacts.InterfaceConsts.LookupKey + " = ?",
                new string[] { lookupKey },
                null);
            var cursor = (ICursor)loader.LoadInBackground();

            string id = null;
            if (cursor.MoveToFirst())
            {
                id = cursor.GetString(0);
            }
            cursor.Close();

            return id;
        }

        public static AndroidContact GetContactInformation(Context ctx, string contactId)
        {
            var contact = new AndroidContact();

            var generalProjection = new string[] { ContactsContract.Contacts.InterfaceConsts.Id,
                                                    ContactsContract.Contacts.InterfaceConsts.DisplayName,
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
            var nicknameProjection = new string[] { ContactsContract.Data.InterfaceConsts.ContactId,
                                                    ContactsContract.Data.InterfaceConsts.Mimetype,
                                                    ContactsContract.CommonDataKinds.Nickname.Name };
            var imProjection = new string[] { ContactsContract.Data.InterfaceConsts.ContactId,
                                                    ContactsContract.Data.InterfaceConsts.Mimetype,
                                                    ContactsContract.CommonDataKinds.Im.InterfaceConsts.Data,
                                                    ContactsContract.CommonDataKinds.Im.InterfaceConsts.Type,
                                                    ContactsContract.CommonDataKinds.Im.Protocol,
                                                    ContactsContract.CommonDataKinds.Im.CustomProtocol };
            var websiteProjection = new string[] { ContactsContract.Data.InterfaceConsts.ContactId,
                                                    ContactsContract.Data.InterfaceConsts.Mimetype,
                                                    ContactsContract.CommonDataKinds.Website.Url,
                                                    ContactsContract.CommonDataKinds.Website.InterfaceConsts.Type };
            var noteProjection = new string[] { ContactsContract.Data.InterfaceConsts.ContactId,
                                                    ContactsContract.Data.InterfaceConsts.Mimetype,
                                                    ContactsContract.CommonDataKinds.Note.NoteColumnId };

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
            var nicknameCursor =
                (ICursor)new CursorLoader(ctx, ContactsContract.Data.ContentUri,
                    nicknameProjection,
                    ContactsContract.Data.InterfaceConsts.ContactId + " = ? AND " +
                        ContactsContract.Data.InterfaceConsts.Mimetype + " = ?",
                    new string[] { contactId, ContactsContract.CommonDataKinds.Nickname.ContentItemType },
                    null).LoadInBackground();
            var imCursor =
                (ICursor)new CursorLoader(ctx, ContactsContract.Data.ContentUri,
                    imProjection,
                    ContactsContract.Data.InterfaceConsts.ContactId + " = ? AND " +
                        ContactsContract.Data.InterfaceConsts.Mimetype + " = ?",
                    new string[] { contactId, ContactsContract.CommonDataKinds.Im.ContentItemType },
                    null).LoadInBackground();
            var websiteCursor =
                (ICursor)new CursorLoader(ctx, ContactsContract.Data.ContentUri,
                    websiteProjection,
                    ContactsContract.Data.InterfaceConsts.ContactId + " = ? AND " +
                        ContactsContract.Data.InterfaceConsts.Mimetype + " = ?",
                    new string[] { contactId, ContactsContract.CommonDataKinds.Website.ContentItemType },
                    null).LoadInBackground();
            var noteCursor =
                (ICursor)new CursorLoader(ctx, ContactsContract.Data.ContentUri,
                    noteProjection,
                    ContactsContract.Data.InterfaceConsts.ContactId + " = ? AND " +
                        ContactsContract.Data.InterfaceConsts.Mimetype + " = ?",
                    new string[] { contactId, ContactsContract.CommonDataKinds.Note.ContentItemType },
                    null).LoadInBackground();

            if (!generalCursor.MoveToFirst() || !generalCursor.IsLast)
                throw new NotSupportedException("Could not find contact or found multiple instances.");

            contact.DisplayName = generalCursor.GetString(1);

            var photoUri = generalCursor.GetString(2);
            if (photoUri != null)
                GetPhoto(ctx, ref contact, Android.Net.Uri.Parse(photoUri));

            List<AndroidContactDataEntry> phoneNumbers = new List<AndroidContactDataEntry>();
            List<AndroidContactDataEntry> emailAddresses = new List<AndroidContactDataEntry>();
            List<AndroidContactDataEntry> postalAddresses = new List<AndroidContactDataEntry>();
            List<AndroidContactImEntry> ims = new List<AndroidContactImEntry>();
            List<AndroidContactDataEntry> websites = new List<AndroidContactDataEntry>();

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
            if (nicknameCursor.MoveToFirst())
                contact.Nickname = nicknameCursor.GetString(2);
            if (imCursor.MoveToFirst())
            {
                do
                {
                    ims.Add(new AndroidContactImEntry(imCursor.GetString(2), imCursor.GetString(3), imCursor.GetString(4), imCursor.GetString(5)));
                } while (imCursor.MoveToNext());
            }
            if (websiteCursor.MoveToFirst())
            {
                do
                {
                    websites.Add(new AndroidContactDataEntry(websiteCursor.GetString(2), websiteCursor.GetString(3)));
                } while (websiteCursor.MoveToNext());
            }
            if (noteCursor.MoveToFirst())
                contact.Note = noteCursor.GetString(2);

            if (phoneNumbers.Count > 0)
                contact.PhoneNumbers = phoneNumbers.ToArray();
            if (emailAddresses.Count > 0)
                contact.EmailAddresses = emailAddresses.ToArray();
            if (postalAddresses.Count > 0)
                contact.PostalAddresses = postalAddresses.ToArray();
            if (ims.Count > 0)
                contact.Ims = ims.ToArray();
            if (websites.Count > 0)
                contact.Websites = websites.ToArray();

            generalCursor.Close();
            phoneCursor.Close();
            emailCursor.Close();
            addressCursor.Close();
            nicknameCursor.Close();
            imCursor.Close();
            websiteCursor.Close();
            noteCursor.Close();

            return contact;
        }

        static void GetPhoto(Context ctx, ref AndroidContact destination, Android.Net.Uri photoUri)
        {
            ByteArrayOutputStream byteBuffer = new ByteArrayOutputStream();
            const int bufSize = 0x100;
            byte[] buf = new byte[bufSize];
            int n;
            using (Stream fileStream = ctx.ContentResolver.OpenInputStream(photoUri))
            {
                while ((n = fileStream.Read(buf, 0, bufSize)) > 0)
                    byteBuffer.Write(buf, 0, n);
            }

            destination.Photo = byteBuffer.ToByteArray();
        }

        public static string ImportContact(Context ctx, AndroidContact contact)
        {
            List<ContentProviderOperation> ops = new List<ContentProviderOperation>();

            ops.Add(ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri)
                .WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null)
                .WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null)
                .Build());

            if (contact.DisplayName != null)
            {
                ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                    .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                    .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.StructuredName.ContentItemType)
                    .WithValue(ContactsContract.CommonDataKinds.StructuredName.DisplayName, contact.DisplayName)
                    .Build());
            }
            if (contact.Photo != null)
            {
                ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                    .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                    .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Photo.ContentItemType)
                    .WithValue(ContactsContract.CommonDataKinds.Photo.PhotoColumnId, contact.Photo)
                    .Build());
            }
            if (contact.PhoneNumbers != null)
            {
                foreach (var phone in contact.PhoneNumbers)
                {
                    ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                        .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                        .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Phone.ContentItemType)
                        .WithValue(ContactsContract.CommonDataKinds.Phone.Number, phone.Data)
                        .WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type, phone.Type)
                        .Build());
                }
            }
            if (contact.EmailAddresses != null)
            {
                foreach (var email in contact.EmailAddresses)
                {
                    ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                        .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                        .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Email.ContentItemType)
                        .WithValue(ContactsContract.CommonDataKinds.Email.Address, email.Data)
                        .WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type, email.Type)
                        .Build());
                }
            }
            if (contact.PostalAddresses != null)
            {
                foreach (var postal in contact.PostalAddresses)
                {
                    ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                        .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                        .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.StructuredPostal.ContentItemType)
                        .WithValue(ContactsContract.CommonDataKinds.StructuredPostal.FormattedAddress, postal.Data)
                        .WithValue(ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type, postal.Type)
                        .Build());
                }
            }
            if (contact.Nickname != null)
            {
                ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                    .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                    .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Nickname.ContentItemType)
                    .WithValue(ContactsContract.CommonDataKinds.Nickname.Name, contact.Nickname)
                    .Build());
            }
            if (contact.Ims != null)
            {
                foreach (var im in contact.Ims)
                {
                    ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                        .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                        .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Im.ContentItemType)
                        .WithValue(ContactsContract.CommonDataKinds.Im.InterfaceConsts.Data, im.Data)
                        .WithValue(ContactsContract.CommonDataKinds.Im.InterfaceConsts.Type, im.Type)
                        .WithValue(ContactsContract.CommonDataKinds.Im.Protocol, im.Protocol)
                        .WithValue(ContactsContract.CommonDataKinds.Im.CustomProtocol, im.CustomProtocol)
                        .Build());
                }
            }
            if (contact.Websites != null)
            {
                foreach (var website in contact.Websites)
                {
                    ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                        .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                        .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Website.ContentItemType)
                        .WithValue(ContactsContract.CommonDataKinds.Website.Url, website.Data)
                        .WithValue(ContactsContract.CommonDataKinds.Website.InterfaceConsts.Type, website.Type)
                        .Build());
                }
            }
            if (contact.Note != null)
            {
                ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                    .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                    .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Note.ContentItemType)
                    .WithValue(ContactsContract.CommonDataKinds.Note.NoteColumnId, contact.Note)
                    .Build());
            }

            var res = ctx.ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
            return res[0].Uri.LastPathSegment;
        }
    }
}
