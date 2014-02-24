using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public class ContactTransfer : UnsavedBinaryTransfer
    {
        public string ContactId { get; set; }
        public Android.Content.Context Context { get; set; }

        protected override void PrepareForReading()
        {
            var contact = ContactProvider.GetContactInformation(Context, ContactId);
            var contactJson = JsonConvert.SerializeObject(contact);

            buffer = new System.Text.UTF8Encoding().GetBytes(contactJson);
            FileLength = buffer.LongLength;
            FileName = string.Format(Context.GetString(Resource.String.ContactFormatStr), contact.DisplayName);

            base.PrepareForReading();
        }
    }
}
