using Nova.Globalization;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class TerminalTypeConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TerminalType terminalType = (TerminalType)value;
            if (terminalType == TerminalType.Embedded)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_AsnyType", out errorInfo);
                return errorInfo;
            }
            else
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_SnyType", out errorInfo);
                return errorInfo;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
