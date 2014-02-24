using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public static class FileSizeExtensions
    {

        static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB" };

        public static string HumanReadableSize(this int bytes)
        {
            int i = 0;
            double bytesDouble = (double)bytes;

            while (bytesDouble > 1000.0 && i < SizeSuffixes.Count())
            {
                bytesDouble /= 1000.0;
                i++;
            }

            return Math.Round(bytesDouble, 1).ToString() + " " + SizeSuffixes[i];
        }

        public static string HumanReadableSizePerSecond(this int bytes)
        {
            return HumanReadableSize(bytes) + "/s";
        }
    }
}
