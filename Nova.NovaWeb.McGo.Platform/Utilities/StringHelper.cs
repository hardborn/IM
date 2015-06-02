using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.Utilities
{
    public static class StringHelper
    {
        public static string TrimMD5String(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            
            string[] splitStr = str.Split(new Char[] { '.' });
            if(splitStr != null && splitStr.Length > 2)
            {
                return String.Format("{0}.{1}", splitStr[0], splitStr[splitStr.Length - 1]);
            }
            else
            {
                return str;
            }
            //if (splitStr[splitStr.Length - 2] != null && splitStr[splitStr.Length - 2].Length == 32)
            //{
            //    return str.Remove(str.LastIndexOf('.') - 33, 33);
            //}
            //else
            //{
            //    return str;
            //}
        }
    }
}
