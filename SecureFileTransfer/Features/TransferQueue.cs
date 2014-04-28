using SecureFileTransfer.Features.Transfers;
using SecureFileTransfer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SecureFileTransfer.Features
{
    public class TransferQueue
    {
        private SenderConnection _connection = null;

        public SenderConnection Connection
        {
            get { return _connection; }
            set {
                if (_connection != null)
                {
                    _connection.FileTransferEnded -= _connection_FileTransferEnded;
                }
                _connection = value;

                _connection.FileTransferEnded += _connection_FileTransferEnded;
            }
        }

        public delegate void FileTransferFailedDelegate(Transfer transfer);

        public event FileTransferFailedDelegate FileTransferFailed;

        private List<Transfer> Queue = new List<Transfer>();

        public Transfer CurrentTransfer { get; private set; }

        public bool HasQueuedTransfers
        {
            get { return Queue.Count > 1; }
        }

        public int Remaining
        {
            get { return Queue.Count == 0 ? 0 : Queue.Count - 1; }
        }

        void StartFileTransfer(Transfer trans)
        {
            try
            {
                Connection.StartFileTransfer(trans);
            }
            catch (Exception ex)
            {
                ex.Handle();
                _connection_FileTransferEnded(trans, false, false);
            }
        }

        void _connection_FileTransferEnded(Transfer trans, bool success, bool aborted)
        {
            if (Queue.Contains(trans))
                Queue.Remove(trans);

            if (!success)
            {
                if (!aborted)
                { 
                    // File Transfer Error
                    if (FileTransferFailed != null)
                        FileTransferFailed(trans);
                }

                CurrentTransfer = null;
                Abort();

                return;
            }

            if (Queue.Count > 0)
            {
                CurrentTransfer = Queue[0];
                StartFileTransfer(CurrentTransfer);
            }
            else
            {
                CurrentTransfer = null;
            }
        }

        public void Enqueue(Transfer trans)
        {
            Queue.Add(trans);

            if (CurrentTransfer == null)
            {
                CurrentTransfer = trans;
                StartFileTransfer(CurrentTransfer);
            }
        }

        public void Abort(bool sendAbort = true)
        {
            Queue.Clear();

            if (CurrentTransfer != null)
            {
                Connection.AbortFileTransfer(sendAbort);
            }

            CurrentTransfer = null;
        }

    }
}
