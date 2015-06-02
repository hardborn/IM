using Nova.Globalization;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using Nova.NovaWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    class CommandTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CmdModeTypes cmdType = (CmdModeTypes)value;
            if (cmdType == CmdModeTypes.immediate)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_NowPublish", out errorInfo);
                return errorInfo;
            }
            else if (cmdType == CmdModeTypes.timing)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_TimePublish", out errorInfo);
                return errorInfo;
            }
            else
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_DefaultPublish", out errorInfo);
                return errorInfo;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
