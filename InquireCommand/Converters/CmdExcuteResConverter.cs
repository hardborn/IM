using Nova.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.Windows.Converters
{
    public class CmdExcuteResConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            string itemResult;
            if (MultiLanguageUtils.GetLanguageString(str, out itemResult))
            {
                return itemResult;
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
