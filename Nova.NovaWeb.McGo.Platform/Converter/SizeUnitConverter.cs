using Nova.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class SizeUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            long size = (long)value;
            if (size <= 0)
            {
                string result;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_MediaNoExist", out result);
                return result;
            }
            else
            {
                return SizeSuffix((long)value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(long value)
        {
            try
            {
                int mag = (int)Math.Log(value, 1024);
                if (mag > 7)
                {
                    MessageBox.Show(value.ToString());
                    // return value.ToString();
                }
                decimal adjustedSize = (decimal)value / (1 << (mag * 10));
                return string.Format("{0:n2} {1}", adjustedSize, SizeSuffixes[mag]);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.StackTrace);
                return "Erro:" + value.ToString();
            }
          
        }
    }
}
