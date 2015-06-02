using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using Nova.Globalization;

namespace Nova.NovaWeb.McGo.Platform.Converter
{
    public class ProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //<Binding  Path="ProgressPercentage"/>
            //                               <Binding Path="PubInfo.CurrentDataItemIndex"/>
            //                               <Binding Path="PubInfo.DataItemCount"/>
            //                               <Binding  Path="PubInfo.PubResult.TransmiteResult"/>
            //                               <Binding  Path="PubInfo.PubResult.CommandSendResult"/>
            int percentage = (int)values[0];
            int currentItemIndex = (int)values[1];
            int itemCount = (int)values[2];
            ListTransmiteRes? transmissionResult = (ListTransmiteRes?)values[3];
            CommandSendResult? commandSendResult =(CommandSendResult?)values[4];

            if (transmissionResult !=null && transmissionResult.Value != ListTransmiteRes.OK)
            {
                string info = MultiLanguageUtils.GetLanguageString(transmissionResult.Value.ToString(), transmissionResult.Value.ToString());
                return string.Format("{0}% ({1}/{2}) {3}", percentage, currentItemIndex, itemCount, info);
            }
            else if (transmissionResult == ListTransmiteRes.OK && commandSendResult != null && commandSendResult != CommandSendResult.SendOk)
            {
               string info = MultiLanguageUtils.GetLanguageString(commandSendResult.ToString(),commandSendResult.ToString());

                return string.Format("{0}% ({1}/{2}) {3}", percentage, currentItemIndex, itemCount,
                    info);
            }
            else
                return string.Format("{0}% ({1}/{2})", percentage, currentItemIndex, itemCount);
            

        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
