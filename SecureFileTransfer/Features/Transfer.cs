using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public abstract class Transfer
    {
        public string FileName { get; set; }
        public long FileLength { get; set; }

        public abstract void AppendData(byte[] buf);
        public abstract byte[] GetData(int maxLen);
        protected abstract void PrepareForReading();
        protected abstract void PrepareForWriting();
        public abstract void CleanUpAfterWriteAbort();
        public abstract void Close();

        public static Transfer GetForRequest(Network.Entities.FileTransferRequest req)
        {
            Transfer t = null;

            switch (req.FileType)
            {
                case "data":
                    t = new UnsavedBinaryTransfer();
                    break;
                case "file":
                    t = new ExistingFileTransfer();
                    break;
                case "cont":
                    t = new ContactTransfer();
                    break;
                default:
                    break;
            }

            if (t != null)
            {
                t.FileName = req.FileName;
                t.FileLength = req.FileLength;
            }

            t.PrepareForWriting();

            return t;
        }

        public Network.Entities.FileTransferRequest GenerateRequest()
        {
            string fileType = null;

            if (this is UnsavedBinaryTransfer)
                fileType = "data";
            else if (this is ExistingFileTransfer)
                fileType = "file";
            else if (this is ContactTransfer)
                fileType = "cont";

            if (fileType == null)
                throw new NotSupportedException("Could not find type of transfer.");

            PrepareForReading();

            return new Network.Entities.FileTransferRequest()
            {
                FileName = FileName,
                FileLength = FileLength,
                FileType = fileType
            };
        }

        protected string IncomingPath
        {
            get
            {
                string storagePath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                string incomingPath = Path.Combine(storagePath, "SecureFileTransfer");

                if (!Directory.Exists(incomingPath))
                {
                    Directory.CreateDirectory(incomingPath);
                }

                return incomingPath;
            }
        }
    }
}
