using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Android.Content;

namespace SecureFileTransfer.Features
{
    public class ExistingFileTransfer : Transfer
    {
        public string AbsoluteFilePath { get; set; }

        public Stream FileStream { get; set; }

        public override void AppendData(byte[] buf, int length)
        {
            FileStream.Write(buf, 0, length);
        }

        long readBytes;

        public override int GetData(byte[] buf)
        {
            int n = FileStream.Read(buf, 0, buf.Length);

            if (n == 0)
                throw new ReadPastEndException();

            readBytes += n;

            return n;
        }

        protected override void PrepareForReading()
        {
            if (FileStream == null)
            {
                // Datei wird aus AbsoluteFilePath gelesen
                FileName = Path.GetFileName(AbsoluteFilePath);

                var fInfo = new FileInfo(AbsoluteFilePath);
                FileLength = fInfo.Length;

                FileStream = new FileStream(AbsoluteFilePath, FileMode.Open, FileAccess.Read);
            }
        }

        protected override void PrepareForWriting()
        {
            int appendNumber = 1;
            string fileName = Path.GetFileNameWithoutExtension(FileName);
            string extension = Path.GetExtension(FileName);
            do
            {
                string appendix = appendNumber > 1 ? "(" + appendNumber.ToString() + ")" : "";

                FileName = fileName + appendix + extension;
                AbsoluteFilePath = Path.Combine(IncomingPath, FileName);

                appendNumber++;
            } while (File.Exists(AbsoluteFilePath));

            FileStream = new FileStream(AbsoluteFilePath, FileMode.Create, FileAccess.Write);
        }

        public override void WriteAborted()
        {
            if (File.Exists(AbsoluteFilePath))
            {
                File.Delete(AbsoluteFilePath);
            }
        }

        public override void WriteSucceeded()
        {
            Intent mediaScanner = new Intent(Intent.ActionMediaScannerScanFile);
            var uri = Android.Net.Uri.Parse(AbsoluteFilePath);

            mediaScanner.SetData(uri);
            Context.SendBroadcast(mediaScanner);
        }

        public override void Close()
        {
            if (FileStream != null)
                FileStream.Close();
        }
    }

    public class ReadPastEndException : Exception
    { }
}
