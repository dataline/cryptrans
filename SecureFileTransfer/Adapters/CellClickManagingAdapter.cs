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

namespace SecureFileTransfer.Adapters
{
    public abstract class CellClickManagingAdapter : BaseAdapter
    {
        private ListView _listView;
        public ListView ListView
        {
            get { return _listView; }
            set
            {
                if (_listView != null)
                    _listView.ItemClick -= ListViewItemClick;

                _listView = value;
                _listView.ItemClick += ListViewItemClick;
            }
        }

        public abstract void ListViewItemClick(object sender, AdapterView.ItemClickEventArgs e);

        public void PerformCellUpdate(int position, Action<View> updateAction)
        {
            var updateView = ListView.GetChildAt(position - ListView.FirstVisiblePosition);
            if (updateView != null)
            {
                updateAction(updateView);
            }
        }
    }
}
