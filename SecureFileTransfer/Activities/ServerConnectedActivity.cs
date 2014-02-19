using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "")]
    public class ServerConnectedActivity : Activity
    {
        Adapters.TransfersListAdapter transfersListAdapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ServerConnectedActivity);

            if (Network.LocalServerConnection.CurrentConnection == null)
            {
                Finish();
                return;
            }

            var connectedToLabel = FindViewById<TextView>(Resource.Id.ConnectedToField);
            connectedToLabel.Text = string.Format(GetString(Resource.String.ConnectedToFormatStr), Network.LocalServerConnection.CurrentConnection.RemoteName);

            var disconnectButton = FindViewById<Button>(Resource.Id.DisconnectButton);
            disconnectButton.Click += (s, e) =>
            {
                Disconnect();
                Finish();
            };

            transfersListAdapter = new Adapters.TransfersListAdapter(this);

            var transfersListView = FindViewById<ListView>(Resource.Id.TransfersListView);
            transfersListView.Adapter = transfersListAdapter;

            var currentConnection = Network.LocalServerConnection.CurrentConnection;
            currentConnection.UIThreadSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            currentConnection.Disconnected += CurrentConnection_Disconnected;
            currentConnection.FileTransferStarted += CurrentConnection_FileTransferStarted;
            currentConnection.FileTransferEnded += CurrentConnection_FileTransferEnded;
            currentConnection.BeginReceiving();
        }

        void CurrentConnection_FileTransferEnded(Network.SingleTransferServer srv, bool success)
        {
            transfersListAdapter.CompletedTransfers.Add(srv.CurrentTransfer);
            transfersListAdapter.CurrentTransfer = null;

            transfersListAdapter.NotifyDataSetChanged();
        }

        void CurrentConnection_FileTransferStarted(Network.SingleTransferServer srv)
        {
            transfersListAdapter.CurrentTransfer = srv.CurrentTransfer;
            transfersListAdapter.CurrentProgress = 0;

            transfersListAdapter.NotifyDataSetChanged();

            Handler refreshHandler = new Handler();
            refreshHandler.PostDelayed(() => ReloadList(refreshHandler), 200);
        }

        void ReloadList(Handler handler)
        {
            var dc = Network.LocalServerConnection.CurrentConnection.DataConnection;

            if (dc.CurrentTransfer != null)
            {
                transfersListAdapter.CurrentProgress = dc.Progress;
                transfersListAdapter.NotifyDataSetChanged();

                handler.PostDelayed(() => ReloadList(handler), 200);
            }
        }

        void CurrentConnection_Disconnected()
        {
            Disconnect();

            Finish();
        }

        public override void OnBackPressed()
        {
            Disconnect();

            base.OnBackPressed();
        }

        public void Disconnect()
        {
            if (Network.LocalServerConnection.CurrentConnection != null)
                Network.LocalServerConnection.CurrentConnection.Dispose();
        }
    }
}