﻿using Android.App;
using Android.Webkit;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SecureFileTransfer.Features;

namespace SecureFileTransfer.Adapters
{
    public class FileBrowserAdapter : BaseAdapter
    {
        Activity parentActivity;

        public List<PathAndDrawable> Contents { get; set; }

        public FileBrowserAdapter(Activity activity)
        {
            parentActivity = activity;
        }

        public override int Count
        {
            get
            {
                if (Contents == null)
                    return 0;
                return Contents.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            var view = convertView ?? parentActivity.LayoutInflater.Inflate(Resource.Layout.FileItem, null);
            var cururi = Contents[position];

            view.FindViewById<TextView>(Resource.Id.FileName).Text = Path.GetFileName(cururi.path);

            view.FindViewById<ImageView>(Resource.Id.ThumbnailView).SetImageDrawable(cururi.drawable);

            return view;
        }

        public new void Dispose()
        {
            Contents.Clear();

            base.Dispose();
        }
    }
}
