using Nova.NovaWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.Windows.Converters
{
    public class DictionaryToCommandConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Dictionary<string,CmdTypes> dicTable = value as Dictionary<string,CmdTypes>;
            if (dicTable == null)
            {
                return string.Empty;
            }
            if (dicTable.Keys.Count == 1)
            {
                return dicTable.Keys.ToList()[0];
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
