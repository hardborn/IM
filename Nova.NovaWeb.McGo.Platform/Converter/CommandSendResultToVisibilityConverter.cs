using Nova.NovaWeb.McGo.Platform.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class CommandSendResultToVisibilityConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //CommandSendResult sendResult = (CommandSendResult)value;
            //if (sendResult == CommandSendResult.SendOk)
            //{
            //    return System.Windows.Visibility.Collapsed;
            //}
            //else
            //{
            //    return System.Windows.Visibility.Visible;
            //}
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
