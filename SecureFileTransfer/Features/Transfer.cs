using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public abstract class Transfer
    {
        public string FileName { get; set; }
        public long FileLength { get; set; }

        public abstract void AppendData(byte[] buf);

        public static Transfer GetForRequest(Network.Entities.FileTransferRequest req)
        {
            Transfer t = null;

            switch (req.FileType)
            {
                case "data":
                    t = new UnsavedBinaryTransfer();
                    break;
                default:
                    break;
            }

            if (t != null)
            {
                t.FileName = req.FileName;
                t.FileLength = req.FileLength;
            }

            return t;
        }
    }
}
