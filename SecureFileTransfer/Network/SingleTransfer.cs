using SecureFileTransfer.Features.Transfers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SecureFileTransfer.Network
{
    /// <summary>
    /// A single data transfer.
    /// </summary>
    /// <typeparam name="ParentConnectionType"></typeparam>
    public abstract class SingleTransfer<ParentConnectionType> : Connection
        where ParentConnectionType : Connection
    {
        protected SingleTransfer() { }

        public Transfer CurrentTransfer { get; protected set; }

        public ParentConnectionType ParentConnection { get; set; }

        protected long CurrentTransferDataLeft;

        protected Thread TransferThread;

        public int Progress
        { 
            get
            {
                if (CurrentTransfer == null)
                    return 0;

                return (int)((1.0f - ((float)CurrentTransferDataLeft / (float)CurrentTransfer.FileLength)) * 100.0f);
            }
        }

        public int BytesPerSecond { get; private set; }

        long PreviousDataWritten = 0;
        public void ReloadBytesPerSecond()
        {
            if (CurrentTransfer == null)
                return;

            long dataWritten = CurrentTransfer.FileLength - CurrentTransferDataLeft;
            BytesPerSecond = (int)(dataWritten - PreviousDataWritten);
            PreviousDataWritten = dataWritten;
        }

        public bool AbortCurrentTransfer { get; protected set; }
        public void Abort()
        {
            AbortCurrentTransfer = true;
            if (ConnectionSocket != null)
            {
                // Free Get/Send Thread by closing the socket.
                ConnectionSocket.Close();
                ConnectionSocket = null;
            }
        }

        public override void Dispose()
        {
            Abort();

            base.Dispose();
        }
    }
}
