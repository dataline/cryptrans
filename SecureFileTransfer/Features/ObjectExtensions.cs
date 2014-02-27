using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public static class ObjectExtensions
    {
        public static void HandleEx(this object obj, Exception ex)
        { 
#if DEBUG
            throw ex;
#else
            Console.WriteLine("ERROR: " + ex.ToString() + "\n" + ex.Message + "\nStack Trace:\n" + ex.StackTrace);
#endif
        }
    }
}
