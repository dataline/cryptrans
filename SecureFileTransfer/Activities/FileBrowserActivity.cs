using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecureFileTransfer.Features;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace SecureFileTransfer.Activities
{
    [Activity(Label = "",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class FileBrowserActivity : Activity
    {
        Adapters.FileBrowserAdapter adapter;

        public const string IE_PATH = "Path";

        CancellationTokenSource cts;

        string currentPath;

        SynchronizationContext mainUISyncCtx;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.FileBrowserActivity);

            ActionBar.SetDisplayHomeAsUpEnabled(true);

            adapter = new Adapters.FileBrowserAdapter(this);

            var listView = FindViewById<ListView>(Resource.Id.ListView);
            listView.Adapter = adapter;
            listView.ItemClick += listView_ItemClick;
            listView.ItemLongClick += listView_ItemLongClick;

            currentPath = Intent.GetStringExtra(IE_PATH);
            if (currentPath == null)
                currentPath = Features.Transfers.Transfer.IncomingPath;

            ActionBar.Title = Path.GetFileName(currentPath);

            mainUISyncCtx = SynchronizationContext.Current ?? new SynchronizationContext();

            await ReloadFileList();
        }

        async Task ReloadFileList()
        {
            if (Directory.Exists(currentPath))
            {
                await Task.Run(() => GetListing(mainUISyncCtx));
                adapter.NotifyDataSetChanged();
            }
        }

        void listView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if (cts != null)
                cts.Cancel();

            var path = adapter.Contents[e.Position].path;

            new Dialogs.YesNoDialog(
                this,
                string.Format(GetString(Resource.String.DeleteConfirmFormatStr), Path.GetFileName(path)),
                Resource.String.Yes,
                Resource.String.No,
                async result =>
                {
                    if (result == Dialogs.YesNoDialog.AndroidDialogResult.Yes)
                    {
                        PerformDelete(path);
                        await ReloadFileList();
                    }
                }).Show("deleteconf");
        }

        void PerformDelete(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                ex.Handle(true);
            }
        }

        void listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (cts != null)
                cts.Cancel();

            var path = adapter.Contents[e.Position].path;

            if (Directory.Exists(path))
            {
                var next = new Intent(this, typeof(FileBrowserActivity));
                next.PutExtra(IE_PATH, path);
                StartActivity(next);
            }
            else
            {
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(
                    Android.Net.Uri.Parse("file://" + path),
                    Java.Net.URLConnection.GuessContentTypeFromName(path));
                StartActivity(intent);
            }
        }

        protected override void OnStop()
        {
            if (cts != null)
                cts.Cancel();

            base.OnStop();
        }

        protected override void OnDestroy()
        {
            adapter.Dispose();
            
            base.OnDestroy();
        }

        void GetListing(SynchronizationContext syncCtx)
        {
            try
            {
                adapter.DisposeContent();

                adapter.Contents = Directory.GetFiles(currentPath)
                    .Concat(Directory.GetDirectories(currentPath))
                    .OrderBy(path => path, new FileFolderComparator())
                    .ThenBy(path => Path.GetFileName(path))
                    .Select(path => new PathAndDrawable(path))
                    .ToList();

                cts = new CancellationTokenSource();

                Preview.InitPreviewForUris(
                    adapter.Contents,
                    cts.Token,
                    syncCtx,
                    Resources,
                    () => adapter.NotifyDataSetChanged());
            }
            catch (Exception ex)
            {
                ex.Handle();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }

    public class FileFolderComparator : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            bool isDirX = Directory.Exists(x);
            bool isDirY = Directory.Exists(y);

            if (isDirX && isDirY || !isDirX && !isDirY)
                return 0;
            if (isDirX && !isDirY)
                return -1;
            return 1;
        }
    }
}