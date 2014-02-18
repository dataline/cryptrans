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

        FileStream file;

        public override void AppendData(byte[] buf)
        {
            file.Write(buf, 0, buf.Length);
        }

        public override byte[] GetData(int maxLen)
        {
            byte[] buf = new byte[maxLen];
            int n = file.Read(buf, 0, maxLen);

            if (n == 0)
                throw new NotSupportedException("Tried to read past end of ExistingFileTransfer buffer.");

            if (n == maxLen)
                return buf;

            byte[] fittingBuf = new byte[n];
            Array.Copy(buf, fittingBuf, n);
            return fittingBuf;
        }

        protected override void PrepareForReading()
        {
            FileName = Path.GetFileName(AbsoluteFilePath);
            
            var fInfo = new FileInfo(AbsoluteFilePath);
            FileLength = fInfo.Length;

            file = new FileStream(AbsoluteFilePath, FileMode.Open, FileAccess.Read);
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

                AbsoluteFilePath = Path.Combine(incomingPath, fileName + appendix + extension);

                appendNumber++;
            } while (File.Exists(AbsoluteFilePath));

            file = new FileStream(AbsoluteFilePath, FileMode.Create, FileAccess.Write);
        }

        public override void Close()
        {
            if (file != null)
                file.Close();
        }
    }
}
