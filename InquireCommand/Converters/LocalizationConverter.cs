using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Nova.NovaWeb.Common;
using System.Collections;
using Nova.NovaWeb.Windows.ViewModels;
using Nova.Globalization;

namespace Nova.NovaWeb.Windows.Converters
{
    public class LocalizationConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Type type = value.GetType();

            if (typeof(ObservableCollection<CmdTypes>).Equals(type))
            {
                ObservableCollection<string> results = new ObservableCollection<string>();

                ObservableCollection<CmdTypes> commandlist = value as ObservableCollection<CmdTypes>;
                foreach (var item in commandlist)
                {
                    string itemResult;
                    if (MultiLanguageUtils.GetLanguageString(item.ToString(), out itemResult))
                    {
                        results.Add(itemResult);
                    }
                    else
                    {
                        results.Add(item.ToString());
                    }
                }
                return results;
            }

            if (typeof(CmdTypes).Equals(type))
            {
                CmdTypes cmdType = (CmdTypes)value;
                string itemResult;
                if (MultiLanguageUtils.GetLanguageString(cmdType.ToString(), out itemResult))
                {
                    return itemResult;
                }
                else
                {
                    return string.Empty;
                }
            }

            if (typeof(CmdPhaseTypes).Equals(type))
            {
                CmdPhaseTypes cmdType = (CmdPhaseTypes)value;
               
                string itemResult;
                if (MultiLanguageUtils.GetLanguageString(cmdType.ToString(), out itemResult))
                {
                    return itemResult;
                }
                else
                {
                    return string.Empty;
                }
            }

            if (typeof(CmdModeTypes).Equals(type))
            {
                CmdModeTypes cmdType = (CmdModeTypes)value;
                string itemResult;
                if (MultiLanguageUtils.GetLanguageString(cmdType.ToString(), out itemResult))
                {
                    return itemResult;
                }
                else
                {
                    return string.Empty;
                }
            }

            if (typeof(cmdOperateCodeTypes).Equals(type))
            {
                cmdOperateCodeTypes cmdType = (cmdOperateCodeTypes)value;
                string itemResult;
                if (MultiLanguageUtils.GetLanguageString(cmdType.ToString(), out itemResult))
                {
                    return itemResult;
                }
                else
                {
                    return string.Empty;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
