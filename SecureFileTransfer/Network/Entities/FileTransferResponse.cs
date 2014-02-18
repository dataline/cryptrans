using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network.Entities
{
    public class FileTransferResponse : TrivialEntityBasedProtocol.Response
    {
        public byte[] AesKey { get; set; }
        public byte[] AesIv { get; set; }
    }
}
