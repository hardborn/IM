using Nova.Globalization;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
   public  class OnlineConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var _platformService = AppEnvionment.Default.Get<IPlatformService>();
            if (value != null)
            {
                if (value.GetType().Equals(typeof(TerminalType)))
                {
                    TerminalType terminalType = (TerminalType)value;
                    if (terminalType == TerminalType.Embedded)
                    {
                        string errorInfo;
                        MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_AsnyType", out errorInfo);
                        return errorInfo;
                    }
                    else
                    {
                        string errorInfo;
                        MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_SnyType", out errorInfo);
                        return errorInfo;
                    }
                }

                if (!value.GetType().Equals(typeof(bool)))
                {
                    return value;
                }

                bool boolValue = (bool)value;

                if (_platformService.GetPlatformLanguage().Equals("en", StringComparison.OrdinalIgnoreCase))
                {
                return boolValue ? "Online" : "Offline";
                }
                else if (_platformService.GetPlatformLanguage().Equals("zh-CN", StringComparison.OrdinalIgnoreCase))
                {
                    return boolValue ? "在线" : "离线";
                }
                else
                {
                    return value;
                }
            }
            else
                return value;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
