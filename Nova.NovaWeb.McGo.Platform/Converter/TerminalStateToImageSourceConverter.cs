using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class TerminalStateToImageSourceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var isOnline = (bool)values[0];
            var isEnableAlarm = (bool)values[1];
            var isAlarm = (bool)values[2];

            Uri uri;

            if (!isOnline)
            {
                uri = new Uri("/MC-go;component/Resources/Images/OffLine.png", UriKind.Relative);
            }
            else
            {
                if (!isEnableAlarm)
                {
                    uri = new Uri("/MC-go;component/Resources/Images/OnLine.png", UriKind.Relative);
                }
                else
                {
                    if (isAlarm)
                        uri = new Uri("/MC-go;component/Resources/Images/AlarmState.png", UriKind.Relative);
                    else
                        uri = new Uri("/MC-go;component/Resources/Images/OnLine.png", UriKind.Relative);
                }
            }

            return new BitmapImage(uri);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
