using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecureFileTransfer.Features;

namespace SecureFileTransfer.Features
{
    public static class ObjectExtensions
    {
        public static void HandleEx(this object obj, Exception ex, bool doNotThrow = false)
        {
            ex.Handle(doNotThrow);
        }
    }
}
