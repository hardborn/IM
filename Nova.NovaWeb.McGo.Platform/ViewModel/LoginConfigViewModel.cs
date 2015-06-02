using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Nova.Globalization;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class LoginConfigViewModel : DialogViewModelBase, IDataErrorInfo
    {
        private const string ServerIpPropertyName = "ServerIp";
        private const string CustomerIdPropertyName = "CustomerId";

        private string _serverIp;
        private string _customerId;
        private bool _canApply;

        private IPlatformService _platformService;

        private ICommand _confirmSettingCommand;

        public class IPAddress
        {
            private string _name;
            public string Name
            {
                set { _name = value; }
                get { return _name; }
            }

            private string _ip;
            public string Ip
            {
                set { _ip = value; }
                get { return _ip; }
            }
        }

        public LoginConfigViewModel(IPlatformService platformService)
        {
            _platformService = platformService;
            if (_platformService == null)
                throw new ArgumentNullException();

            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();
             if(serverInfo == null)
                return ;
            _serverIp = string.IsNullOrEmpty(serverInfo.ServerAddress) ? string.Empty: serverInfo.ServerAddress.Replace("https://",string.Empty);
            _customerId = serverInfo.CustomerId;
        }

        public string ServerIp
        {
            get { return _serverIp; }
            set
            {
                if (value == _serverIp)
                {
                    return;
                }
                _serverIp = value.TrimEnd();
                RaisePropertyChanged(ServerIpPropertyName);
            }
        }

        public string CustomerId
        {
            get { return _customerId; }
            set
            {
                if (value == _customerId)
                {
                    return;
                }
                _customerId = value.TrimEnd();
                RaisePropertyChanged(CustomerIdPropertyName);
            }
        }

        public bool CanApply
        {
            get { return _canApply; }
            set
            {
                if (_canApply != value)
                {
                    _canApply = value;
                    RaisePropertyChanged("CanApply");
                }
            }
        }

        public ICommand ConfirmSettingCommand
        {
            get
            {
                if (_confirmSettingCommand == null)
                {
                    _confirmSettingCommand = new RelayCommand(() =>
                        ConfirmSetting(),
                        () =>
                        {
                            bool result = IsIP(ServerIp) | IsUrl(ServerIp);

                            return result;
                        });
                }
                return _confirmSettingCommand;
            }
        }

        private void ConfirmSetting()
        {
            try
            {
                var platform = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
                platform.ServerInfo = new ServerInfo() { ServerAddress = string.Format("{0}{1}", "https://", ServerIp), CustomerId = CustomerId };
                platform.Save();

                Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshServerInfo"), "RefreshServerInfo");

                Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "OK"), "LoginConfig");
            }
            catch (Exception e)
            {                
                throw new Exception(e.StackTrace);
            }
           
        }

        public static bool IsIP(string ip)
        {
            //判断是否为IP
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        public static bool IsUrl(string url)
        {
            bool result;
            result = Regex.IsMatch(url, @"([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            return result;
        }

        #region Validate 验证

        static readonly string[] ValidatedProperties = 
        { 
            ServerIpPropertyName
        };

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get { return GetValidationError(columnName); }
        }

        protected string[] _error;

        private string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string result = null;

            if (_error == null)
                _error = new string[1];

            switch (propertyName)
            {
                case ServerIpPropertyName:
                    result = this.ValidateIP(this.ServerIp);
                    _error[0] = result;
                    break;
                default:
                    break;
            }

            CanApply = ValidateFields(_error);

            return result;
        }

        internal static bool ValidateFields(string[] value)
        {
            foreach (string s in value)
                if (!string.IsNullOrEmpty(s))
                    return false;
            return true;
        }

        private string ValidateIP(string ip)
        {
            if (!IsIP(ip) && !IsUrl(ip))
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_IpError", out errorInfo);
                return errorInfo;
            }
            return null;
        }


        #endregion
    }
}
