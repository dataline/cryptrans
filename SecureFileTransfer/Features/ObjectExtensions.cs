using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public static class ObjectExtensions
    {
        public static void HandleEx(this object obj, Exception ex, bool doNotThrow = false)
        { 
#if DEBUG
            if (!doNotThrow)
                throw ex;
            else
#endif
            Console.WriteLine("ERROR: " + ex.ToString() + "\n" + ex.Message + "\nStack Trace:\n" + ex.StackTrace);
        }
    }
}
