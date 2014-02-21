using Java.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public class VCardTransfer : UnsavedBinaryTransfer
    {
        public Stream VcardStream { get; set; }

        protected override void PrepareForReading()
        {
            // Die Content URI von Vcards liefert keine Dateigrößen zurück, da sie die Vcard
            // beim Lesen generiert, also in den Speicher lesen.
            int n;
            ByteArrayOutputStream byteBuffer = new ByteArrayOutputStream();
            const int bufSize = 0x100;
            byte[] buf = new byte[bufSize];

            while ((n = VcardStream.Read(buf, 0, bufSize)) > 0)
                byteBuffer.Write(buf, 0, n);

            buffer = byteBuffer.ToByteArray();
            FileLength = buffer.LongLength;

            base.PrepareForReading();
        }
    }
}
