using Nova.NovaWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.Windows.Converters
{
    public class PhaseTypeToBackgroudConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Type type = value.GetType();
            if (typeof(CmdPhaseTypes).Equals(type))
            {
                CmdPhaseTypes cmdType = (CmdPhaseTypes)value;
                if (cmdType == CmdPhaseTypes.finish_failed)
                {
                    return System.Windows.Media.Brushes.Red;
                }
            }
            return System.Windows.Media.Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
