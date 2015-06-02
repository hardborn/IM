using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using DevComponents.DotNetBar;
using Nova.NovaWeb.McGo.Platform.ViewModel;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class CollectionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? result = null;
            var items = value as System.Collections.ObjectModel.ReadOnlyObservableCollection<Object>;
            if (items != null)
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    var item = items[i] as TerminalViewModel;
                    if(item == null)
                        continue;
                    bool? current = item.IsSelected;
                    if (i == 0)
                    {
                        result = current;
                    }
                    else if (result != current)
                    {
                        result = null;
                        break;
                    }
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
