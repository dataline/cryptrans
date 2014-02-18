using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.Entities
{
    public class FileTransferRequest : TrivialEntityBasedProtocol.Request
    {
        public string FileName { get; set; }
        public long FileLength { get; set; }
        public string FileType { get; set; }
    }
}
