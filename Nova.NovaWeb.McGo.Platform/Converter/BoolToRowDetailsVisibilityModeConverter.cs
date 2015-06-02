using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class BoolToRowDetailsVisibilityModeConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolValue = (bool)value;
            if (boolValue)
            {
                return System.Windows.Controls.DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }
            else
            {
                return System.Windows.Controls.DataGridRowDetailsVisibilityMode.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
