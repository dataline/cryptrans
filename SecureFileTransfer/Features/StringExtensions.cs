using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public static class StringExtensions
    {
        public static string LastName(this string str)
        {
            var lastIndexOfSpace = str.LastIndexOf(' ');

            return str.Substring(lastIndexOfSpace == -1 ? 0 : lastIndexOfSpace + 1);
        }

        public static string HtmlStringWithBoldLastName(this string str)
        {
            var lastIndexOfSpace = str.LastIndexOf(' ');

            var stringBefore = str.Substring(0, lastIndexOfSpace + 1);
            var stringAfter = str.Substring(lastIndexOfSpace + 1);

            return stringBefore + "<b>" + stringAfter + "</b>";
        }
    }
}
