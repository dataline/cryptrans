﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SecureFileTransfer.Features;
using SecureFileTransfer.Features.Transfers;

namespace SecureFileTransfer.Adapters
{
    public class TransfersListAdapter : BaseAdapter
    {
        Activity parentActivity;

        public List<Transfer> CompletedTransfers = new List<Transfer>();

        public Transfer CurrentTransfer { get; set; }
        public int CurrentProgress { get; set; }
        public string CurrentStatusString { get; set; }

        public TransfersListAdapter(Activity activity)
        {
            parentActivity = activity;
        }

        public Transfer GetCompletedTransferForIndex(int index)
        {
            if (CurrentTransfer != null)
                index--;
            var trans = CompletedTransfers[index];
            //if (trans == null)
            //    throw new Exception();

            return trans;
        }

        public bool IsCurrentItem(int position)
        {
            return CurrentTransfer != null && position == 0;
        }

        public override int Count
        {
            get { return CurrentTransfer == null ? CompletedTransfers.Count : CompletedTransfers.Count + 1; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int ViewTypeCount
        {
            get
            {
                return 2;
            }
        }

        public override int GetItemViewType(int position)
        {
            return IsCurrentItem(position) ? 0 : 1;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            bool isCurrentItem = IsCurrentItem(position);

            if (view == null)
            {
                if (isCurrentItem)
                    view = parentActivity.LayoutInflater.Inflate(Resource.Layout.TransferListItem, null);
                else
                    view = parentActivity.LayoutInflater.Inflate(Resource.Layout.TransferListCompletedItem, null);
            }

            Transfer transfer = isCurrentItem ? CurrentTransfer : GetCompletedTransferForIndex(position);

            view.FindViewById<TextView>(Resource.Id.FileNameLabel).Text = transfer.FileName;

            if (isCurrentItem)
            {
                view.FindViewById<TextView>(Resource.Id.StatusLabel).Text = CurrentStatusString;
                view.FindViewById<ProgressBar>(Resource.Id.ProgressBar).Progress = CurrentProgress;
            }
            else
            {
                var thumbnail = view.FindViewById<ImageView>(Resource.Id.ThumbnailView);

                if (transfer.Thumbnail != null)
                    thumbnail.SetImageDrawable(transfer.Thumbnail);
                else
                    thumbnail.SetImageResource(
                        transfer is ContactTransfer ?
                        Resource.Drawable.contact :
                        Resource.Drawable.file
                        );
                
            }

            return view;
        }
    }
}
