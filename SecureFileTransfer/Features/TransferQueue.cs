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
        private ClientConnection _connection = null;

        public ClientConnection Connection
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

        void _connection_FileTransferEnded(SingleTransferClient cli, bool success)
        {
            var trans = cli.CurrentTransfer;
            if (Queue.Contains(trans))
                Queue.Remove(trans);

            if (!success)
            {
                //FIXME
                CurrentTransfer = null;
                Abort();

                return;
            }

            if (Queue.Count > 0)
            {
                CurrentTransfer = Queue[0];
                Connection.StartFileTransfer(CurrentTransfer);
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
                Connection.StartFileTransfer(CurrentTransfer);
            }
        }

        public void Abort()
        {
            Queue.Clear();

            if (CurrentTransfer != null)
            {
                Connection.AbortFileTransfer();
            }

            CurrentTransfer = null;
        }

    }
}
