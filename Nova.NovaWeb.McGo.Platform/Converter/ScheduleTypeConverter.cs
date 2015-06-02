using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class ScheduleTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ScheduleType scheduleType = (ScheduleType)value;
            if (scheduleType == ScheduleType.Common)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_CommonScheduleType", out errorInfo);
                return errorInfo;
            }
            else
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_PcScheduleType", out errorInfo);
                return errorInfo;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
