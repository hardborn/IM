using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class EnableScreenShotConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            bool enableCamera = (bool)value;
            Uri uri;
            if (enableCamera)
            {
                uri = new Uri("/Mc-go;component/Resources/Images/shot.png", UriKind.Relative);
            }
            else
            {
                uri = new Uri("/Mc-go;component/Resources/Images/shot2.png", UriKind.Relative);
            }
            return uri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
