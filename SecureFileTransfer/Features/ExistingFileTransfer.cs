using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SecureFileTransfer.Features
{
    public class ExistingFileTransfer : Transfer
    {
        public string AbsoluteFilePath { get; set; }

        public Stream FileStream { get; set; }

        public override void AppendData(byte[] buf)
        {
            FileStream.Write(buf, 0, buf.Length);
        }

        long readBytes;

        public override byte[] GetData(int maxLen)
        {
            byte[] buf = new byte[maxLen];
            int n = FileStream.Read(buf, 0, maxLen);

            //if (n == 0)
            //    throw new ReadPastEndException();

            if (n == 0)
            {
                // fix this stupid file size bug
                n = maxLen;
                //FIXME
            }

            readBytes += n;

            if (n == maxLen)
                return buf;

            byte[] fittingBuf = new byte[n];
            Array.Copy(buf, fittingBuf, n);

            return fittingBuf;
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
            string storagePath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            string incomingPath = Path.Combine(storagePath, "SecureFileTransfer");

            if (!Directory.Exists(incomingPath))
            {
                Directory.CreateDirectory(incomingPath);
            }

            int appendNumber = 1;
            string fileName = Path.GetFileNameWithoutExtension(FileName);
            string extension = Path.GetExtension(FileName);
            do
            {
                string appendix = appendNumber > 1 ? "(" + appendNumber.ToString() + ")" : "";

                FileName = fileName + appendix + extension;
                AbsoluteFilePath = Path.Combine(incomingPath, FileName);

                appendNumber++;
            } while (File.Exists(AbsoluteFilePath));

            FileStream = new FileStream(AbsoluteFilePath, FileMode.Create, FileAccess.Write);
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
