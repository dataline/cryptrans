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

        public override void Close()
        {
            if (!IsReading)
            {
                if (Context == null)
                    throw new NotSupportedException("Context must not be null.");
                
                var contactJson = new System.Text.UTF8Encoding().GetString(buffer);

                ResultingContact = JsonConvert.DeserializeObject<AndroidContact>(contactJson, JsonSettings);

                ContactProvider.ImportContact(Context, ResultingContact);
            }

            base.Close();
        }
    }
}
