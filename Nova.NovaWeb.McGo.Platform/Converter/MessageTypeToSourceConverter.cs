using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class MessageTypeToSourceConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            MessageType type = (MessageType)value;
            Uri uri;
            if (type== MessageType.Alarm)
            {
                uri = new Uri("/MC-go;component/Resources/Images/Warning.png", UriKind.Relative);
            }
            else if(type == MessageType.Error)
            {
                uri = new Uri("/MC-go;component/Resources/Images/Error.png", UriKind.Relative);
            }
            else
            {
                uri = new Uri("/MC-go;component/Resources/Images/Information.png", UriKind.Relative);
            }
            return uri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
