using Nova.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class BadConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
           string str =  value as string;
           if (string.IsNullOrEmpty(str))
           {
               return string.Empty;
           }
           if (str == "ManangementCenter_UI_Preview")
           {
               string result;
               MultiLanguageUtils.GetLanguageString(str, out result);
               return result;
           }
           if (str == "ManangementCenter_UI_StopPreview")
           {
               string result;
               MultiLanguageUtils.GetLanguageString(str, out result);
               return result;
           }
           return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
