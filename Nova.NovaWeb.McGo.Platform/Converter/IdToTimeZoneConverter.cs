using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Data;


namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class IdToTimeZoneConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var timeZoneId = value as string;

            if (string.IsNullOrEmpty(timeZoneId))
            {
                Debug.WriteLine("时区为空");
                return string.Empty;
            }
            TimeZoneTable timeZoneTable = AppEnvionment.Default.Get<IPlatformService>().GetTimeZoneTable();
            if(timeZoneTable == null)
                return timeZoneId.ToString();
            var timeZoneInfo = timeZoneTable.TimeZoneInfoList.FirstOrDefault(t => t.Id == timeZoneId);

            /***************/
            if(string.IsNullOrEmpty(timeZoneInfo.DisplayName))
            {
                Debug.WriteLine(timeZoneId.ToString() + "在时区表里不存在");
            }
            /**************/

            if(timeZoneTable == null)
                return timeZoneId.ToString();

            return timeZoneInfo.DisplayName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
