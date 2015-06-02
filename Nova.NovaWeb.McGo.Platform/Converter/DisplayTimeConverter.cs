using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class DisplayTimeConverter:IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime? excuteTime = (DateTime?)values[0];
            TimeZoneInformation timeZone = values[1] as TimeZoneInformation;
            if(excuteTime == null)
                return null;
            if(timeZone == null)
                return null;
            return timeZone.GetLocalTime(excuteTime.Value, true).ToString("yyyy/MM/dd HH:mm:ss");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
