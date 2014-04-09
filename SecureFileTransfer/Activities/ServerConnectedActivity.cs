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
using SecureFileTransfer.Dialogs;
using SecureFileTransfer.Features;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class ServerConnectedActivity : Activity
    {
        Adapters.TransfersListAdapter transfersListAdapter;

        YesNoDialog abortCurrentTransferDialog = null;

        bool statusReloadingHandlerEnabled = false;

        SynchronizationContext mainUISyncCtx;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ServerConnectedActivity);

            if (Network.ReceiverConnection.CurrentConnection == null)
            {
                Finish();
                return;
            }

            var connectedToLabel = FindViewById<TextView>(Resource.Id.ConnectedToField);
            connectedToLabel.Text = string.Format(GetString(Resource.String.ConnectedToFormatStr), Network.ReceiverConnection.CurrentConnection.RemoteName);

            var disconnectButton = FindViewById<Button>(Resource.Id.DisconnectButton);
            disconnectButton.Click += (s, e) =>
            {
                Disconnect();
                Finish();
            };

            transfersListAdapter = new Adapters.TransfersListAdapter(this);

            var transfersListView = FindViewById<ListView>(Resource.Id.TransfersListView);
            transfersListView.Adapter = transfersListAdapter;
            transfersListView.ItemClick += transfersListView_ItemClick;

            mainUISyncCtx = SynchronizationContext.Current ?? new SynchronizationContext();

            var currentConnection = Network.ReceiverConnection.CurrentConnection;
            currentConnection.UIThreadSyncContext = mainUISyncCtx;
            currentConnection.Disconnected += CurrentConnection_Disconnected;
            currentConnection.FileTransferStarted += CurrentConnection_FileTransferStarted;
            currentConnection.FileTransferEnded += CurrentConnection_FileTransferEnded;
            currentConnection.BeginReceiving();
        }

        void transfersListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (transfersListAdapter.IsCurrentItem(e.Position))
            {
                abortCurrentTransferDialog = new YesNoDialog(this,
                    GetString(Resource.String.AbortAreYouSure),
                    Resource.String.Yes,
                    Resource.String.No,
                    result =>
                    {
                        if (result == YesNoDialog.AndroidDialogResult.Yes)
                        {
                            Network.ReceiverConnection.CurrentConnection.AbortFileTransfer();
                        }

                        abortCurrentTransferDialog = null;
                    });
                abortCurrentTransferDialog.Show("act");
            }
            else
            {
                var transfer = transfersListAdapter.GetCompletedTransferForIndex(e.Position);
                transfer.OpenPreview(this);
            }
        }

        void CurrentConnection_FileTransferEnded(Network.SingleTransferReceiver srv, bool success)
        {
            if (success)
                transfersListAdapter.CompletedTransfers.Insert(0, srv.CurrentTransfer);
            transfersListAdapter.CurrentTransfer = null;

            transfersListAdapter.NotifyDataSetChanged();

            if (abortCurrentTransferDialog != null)
            {
                abortCurrentTransferDialog.Dismiss();
            }

        }

        void CurrentConnection_FileTransferStarted(Network.SingleTransferReceiver srv)
        {
            srv.CurrentTransfer.Context = this;
            srv.CurrentTransfer.SetThumbnailCallback(
                () => transfersListAdapter.NotifyDataSetChanged(),
                mainUISyncCtx
                );

            transfersListAdapter.CurrentTransfer = srv.CurrentTransfer;
            transfersListAdapter.CurrentProgress = 0;

            transfersListAdapter.NotifyDataSetChanged();

            ReloadList();
        }

        void ReloadList(Handler handler = null, int invokeCount = 1)
        {
            if (statusReloadingHandlerEnabled && handler == null)
                return;
            if (Network.ReceiverConnection.CurrentConnection == null)
                return;

            statusReloadingHandlerEnabled = true;

            if (handler == null)
                handler = new Handler();

            var dc = Network.ReceiverConnection.CurrentConnection.DataConnection;

            if (dc != null && dc.CurrentTransfer != null)
            {
                if (invokeCount % 5 == 0)
                    dc.ReloadBytesPerSecond();

                transfersListAdapter.CurrentProgress = dc.Progress;
                transfersListAdapter.CurrentStatusString = dc.BytesPerSecond.HumanReadableSizePerSecond();
                transfersListAdapter.NotifyDataSetChanged();

                handler.PostDelayed(() => ReloadList(handler, invokeCount + 1), 200);
            }
            else
            {
                statusReloadingHandlerEnabled = false;
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
            if (Network.ReceiverConnection.CurrentConnection != null)
                Network.ReceiverConnection.CurrentConnection.Dispose();
        }
    }
}