﻿using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Provider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SecureFileTransfer.Features.Transfers
{
    public class ContactTransfer : UnsavedBinaryTransfer
    {
        public string ContactId { get; set; }
        public AndroidContact ResultingContact { get; set; }

        JsonSerializerSettings JsonSettings
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
            }
        }

        protected override void PrepareForReading()
        {
            var contact = ContactProvider.GetContactInformation(Context, ContactId);
            var contactJson = JsonConvert.SerializeObject(contact, JsonSettings);

            buffer = new System.Text.UTF8Encoding().GetBytes(contactJson);
            FileLength = buffer.LongLength;
            FileName = string.Format(Context.GetString(Resource.String.ContactFormatStr), contact.DisplayName);

            base.PrepareForReading();
        }

        public override void WriteSucceeded()
        {
            if (Context == null)
                throw new NotSupportedException("Context must not be null.");

            var contactJson = new System.Text.UTF8Encoding().GetString(buffer);

            ResultingContact = JsonConvert.DeserializeObject<AndroidContact>(contactJson, JsonSettings);

            ContactId = ContactProvider.ImportContact(Context, ResultingContact);

            if (ResultingContact.Photo != null)
            {
                var list = new List<ByteArrayAndDrawable>();
                list.Add(new ByteArrayAndDrawable(ResultingContact.Photo));

                Preview.InitPreviewForByteArrays(list, new CancellationTokenSource().Token,
                    MainUISyncContext, Context.Resources,
                    () =>
                    {
                        Thumbnail = list[0].drawable;
                        NotifyThumbnailChanged();
                    });
            }
        }

        public override void OpenPreview(Android.App.Activity androidActivity)
        {
            if (ContactId != null)
            {
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(
                    Android.Net.Uri.WithAppendedPath(ContactsContract.Contacts.ContentUri, ContactId),
                    ContactsContract.Contacts.ContentType);

                androidActivity.StartActivity(intent);
            }
        }
    }
}
