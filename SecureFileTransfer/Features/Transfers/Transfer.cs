﻿using Android.Graphics.Drawables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SecureFileTransfer.Features.Transfers
{
    public abstract class Transfer
    {
        public const string FolderName = "cryptrans";

        public string FileName { get; set; }
        public long FileLength { get; set; }

        public Drawable Thumbnail { get; set; }
        public Action ThumbnailChangedCallback { get; set; }
        public SynchronizationContext MainUISyncContext { get; set; }

        public void SetThumbnailCallback(Action callback, SynchronizationContext ctx)
        {
            ThumbnailChangedCallback = callback;
            MainUISyncContext = ctx;
        }

        protected void NotifyThumbnailChanged()
        {
            if (ThumbnailChangedCallback != null &&
                MainUISyncContext != null)
                MainUISyncContext.Send(
                    new SendOrPostCallback(state => ThumbnailChangedCallback()),
                    null);
        }

        public Android.Content.Context Context { get; set; }

        public bool IsReading { get; set; }

        public abstract void AppendData(byte[] buf, int length);
        public abstract int GetData(byte[] buf);
        protected abstract void PrepareForReading();
        protected abstract void PrepareForWriting();
        public abstract void WriteAborted();
        public abstract void WriteSucceeded();
        public abstract void Close();

        public abstract void OpenPreview(Android.App.Activity androidActivity);

        public virtual void PrepareThumbnail()
        {
        }

        public static Transfer GetForRequest(Network.Entities.FileTransferRequest req)
        {
            Transfer t = null;

            switch (req.FileType)
            {
                case "data":
                    t = new UnsavedBinaryTransfer();
                    break;
                case "file":
                    t = new ExistingFileTransfer();
                    break;
                case "cont":
                    t = new ContactTransfer();
                    break;
                default:
                    break;
            }

            if (t != null)
            {
                t.FileName = req.FileName;
                t.FileLength = req.FileLength;
            }

            t.IsReading = false;
            t.PrepareForWriting();

            return t;
        }

        public Network.Entities.FileTransferRequest GenerateRequest()
        {
            string fileType = null;

            if (this is ExistingFileTransfer)
                fileType = "file";
            else if (this is ContactTransfer)
                fileType = "cont";
            else if (this is UnsavedBinaryTransfer)
                fileType = "data";

            if (fileType == null)
                throw new NotSupportedException("Could not find type of transfer.");

            this.IsReading = true;
            PrepareForReading();

            return new Network.Entities.FileTransferRequest()
            {
                FileName = FileName,
                FileLength = FileLength,
                FileType = fileType
            };
        }

        public static string IncomingPath
        {
            get
            {
                string storagePath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                string incomingPath = Path.Combine(storagePath, FolderName);

                if (!Directory.Exists(incomingPath))
                {
                    Directory.CreateDirectory(incomingPath);
                }

                return incomingPath;
            }
        }
    }
}
