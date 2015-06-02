using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class OnlineToImageSourceConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            bool isOnline = (bool)value;
            Uri uri;
            if (isOnline)
            {
                uri = new Uri("/MC-go;component/Resources/Images/OnLine.png", UriKind.Relative);
            }
            else
            {
                uri = new Uri("/MC-go;component/Resources/Images/OffLine.png", UriKind.Relative);
            }
            return uri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
