using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.DAL;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {

        public AboutViewModel()
        {
            LoadProductInfo();
        }

        public string CopyRightInfo { get; set; }
        public string VersionNumber { get; set; }


        private void LoadProductInfo()
        {
        
            try
            {
                if (File.Exists(PlatformConfig.CopyRightFilePath))
                {
                    DataSet cpyDataSet = new DataSet();
                    cpyDataSet.ReadXml(PlatformConfig.CopyRightFilePath);

                    string formTitle = (string)(cpyDataSet.Tables["CopyRightInfo"].Rows[0].ItemArray[0]);
                    string productCompany = (string)(cpyDataSet.Tables["CopyRightInfo"].Rows[0].ItemArray[1]);
                    string productCopyRight = (string)(cpyDataSet.Tables["CopyRightInfo"].Rows[0].ItemArray[2]);
                   
                    CopyRightInfo = string.Format("{0} {1}", productCompany, productCopyRight);
                    VersionNumber = formTitle;
                    
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
