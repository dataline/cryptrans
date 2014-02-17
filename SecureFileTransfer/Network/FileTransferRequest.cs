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
            //FileName = conn.GetUndefinedLengthString();
            //
            //byte[] filelength = new byte[8];
            //conn.Get(filelength)
            //FileLength = BitConverter.ToInt64(filelength, 0);
            //
            //FileType = conn.GetUndefinedLengthString();
            //
            //if (!conn.DoesAcceptRequest(this))
            //{
            //    conn.SendDecline();
            //    return;
            //}
            //
            //Security.AES fileAES = new Security.AES();
            //fileAES.Generate();
            //
            //SingleTransferServer srv = SingleTransferServer.GetServer();
            //srv.ParentRequest = this;
            //srv.ParentConnection = conn;
            //
            //conn.SendAccept();
            //conn.Write(srv.Address, true);
            //conn.Write(BitConverter.GetBytes((Int32)SingleTransferServer.Port));
            //conn.Write(fileAES.aesKey);
            //conn.Write(fileAES.aesIV);
            //
            //if (!srv.GetConnection(fileAES))
            //{
            //    conn.SendDecline();
            //    return;
            //}
            //
            //conn.CurrentFileTransfer = srv;
            //
            //srv.BeginReceiving();
        }

        public override void Perform(ClientConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}
