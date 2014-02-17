using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Network
{
    public class FileTransferRequest : Request
    {
        public string FileName { get; set; }
        public Int64 FileLength { get; set; }
        public string FileType { get; set; }

        public static string RequestIdentifier
        {
            get { return "ft"; }
        }

        public override void Process(LocalServerConnection conn)
        {
            FileName = conn.GetUndefinedLengthString();
            FileLength = Convert.ToInt64(conn.GetUndefinedLengthString());
            FileType = conn.GetUndefinedLengthString();
            
            if (!conn.DoesAcceptRequest(this))
            {
                conn.SendDecline();
                return;
            }
            
            Security.AES fileAES = new Security.AES();
            fileAES.Generate();
            
            conn.SendAccept();
            conn.Write(fileAES.aesKey);
            conn.Write(fileAES.aesIV);

            conn.DataConnection.BeginReceiving(this, fileAES);
        }

        public override bool Perform(ClientConnection conn)
        {
            conn.Write(RequestIdentifier, true);
            if (!conn.DoesAccept())
                return false;
            
            conn.Write(FileName, true);
            conn.Write(FileLength.ToString(), true);
            conn.Write(FileType, true);

            if (!conn.DoesAccept())
                return false;

            byte[] aesKey = new byte[Security.AES.KeySize];
            byte[] aesIv = new byte[Security.AES.BlockSize];
            conn.Get(aesKey);
            conn.Get(aesIv);

            conn.DataConnection.BeginSending(this, aesKey, aesIv);

            return true;
        }
    }
}
