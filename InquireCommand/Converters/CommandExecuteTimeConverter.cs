using Nova.NovaWeb.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nova.NovaWeb.Windows.Converters
{
    public class CommandExecuteTimeConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var replayCommand = value as InquireReplyCommand;
            if (replayCommand == null)
            {
                return null;
            }
            else
            {
                if (replayCommand.CommandModeType == Common.CmdModeTypes.immediate)
                {
                    return "--";
                }
                else
                {
                    return replayCommand.ExecuteDateTime.ToString();
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
