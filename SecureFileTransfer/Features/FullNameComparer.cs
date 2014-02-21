using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public class FullNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return x.LastName().CompareTo(y.LastName());
        }
    }
}
