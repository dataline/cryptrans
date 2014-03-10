using Android.Graphics.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SecureFileTransfer.Features;
using Android.Graphics;

namespace SecureFileTransfer.Features
{
    public struct PathAndDrawable
	{
        public string path;
        public Drawable drawable;

        public PathAndDrawable(string p)
        {
            path = p;
            drawable = null;
        }
	}
    public static class Preview
    {

        const int MaxWidth = 80;
        const int MaxHeight = MaxWidth;

        public static void InitPreviewForUris(
            List<PathAndDrawable> list,
            CancellationToken ct,
            SynchronizationContext sync,
            Android.Content.Res.Resources res,
            Action callback)
        {
            var t = new Thread(() => __InitPreviewForUris(list, ct, sync, res, callback));
            t.Start();
        }

        static void __InitPreviewForUris(
            List<PathAndDrawable> list,
            CancellationToken ct,
            SynchronizationContext sync,
            Android.Content.Res.Resources res,
            Action callback)
        {
            for (int i = 0; i < list.Count && !ct.IsCancellationRequested; i++)
            {
                var uad = list[i];

                if (uad.path.IsImage())
                {
                    try
                    {
                        uad.drawable = GetResized(Drawable.CreateFromPath(uad.path), res);
                    }
                    catch (Exception ex)
                    {
                        ex.Handle(true);
                    }
                }

                list[i] = uad;

                if (!ct.IsCancellationRequested && callback != null && sync != null)
                    sync.Send(new SendOrPostCallback(state => callback()), null);
            }
        }

        static Drawable GetResized(Drawable d, Android.Content.Res.Resources res)
        {
            if (d == null)
                return null;

            var src = (BitmapDrawable)d;

            double width = (double)src.IntrinsicWidth;
            double height = (double)src.IntrinsicHeight;

            if (width <= MaxWidth && height <= MaxHeight)
                return d;

            if (width > height)
            {
                height = height / width * MaxWidth;
                width = MaxWidth;
            }
            else if (height > width)
            {
                width = width / height * MaxHeight;
                height = MaxHeight;
            }
            else
            {
                width = MaxWidth;
                height = MaxHeight;
            }

            var resized = Bitmap.CreateScaledBitmap(src.Bitmap, (int)width, (int)height, false);

            src.Dispose();

            return new BitmapDrawable(res, resized);
        }
    }
}
