using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.Model;
using Nova.NovaWeb.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class ModifyPasswordViewModel : ViewModelBase, IDataErrorInfo
    {
        private IPlatformService _platformService;

        public ModifyPasswordViewModel(IPlatformService service)
        {
            _platformService = service;
            UserName = _platformService.GetPlatformAccount().Name;
        }

        private string _userName;

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        private string _oldPassword;

        public string OldPassword
        {
            get
            {
                return _oldPassword;
            }
            set
            {
                _oldPassword = value;
                RaisePropertyChanged("OldPassword");
            }
        }

        private string _newPassword;

        public string NewPassword
        {
            get
            {
                return _newPassword;
            }
            set
            {
                _newPassword = value;
                RaisePropertyChanged("NewPassword");
                RaisePropertyChanged("ConfirmPassword");
            }
        }

        private string _confirmPassword;

        public string ConfirmPassword
        {
            get
            {
                return _confirmPassword;
            }
            set
            {
                _confirmPassword = value;
                RaisePropertyChanged("ConfirmPassword");
            }
        }

        private RelayCommand<object> _confirmSettingCommand;

        public RelayCommand<object> ConfirmSettingCommand
        {
            get
            {
                if (_confirmSettingCommand == null)
                {
                    _confirmSettingCommand = new RelayCommand<object>((t) =>
                    {
                        ConfirmSetting(t);
                    },
                    (t) =>
                    {
                        return CanApply;
                    });
                }
                return _confirmSettingCommand;
            }
        }


        private RelayCommand<object> _cancelSettingCommand;

        public RelayCommand<object> CancelSettingCommand
        {
            get
            {
                if (_cancelSettingCommand == null)
                {
                    _cancelSettingCommand = new RelayCommand<object>((t) =>
                    {
                        CancelSetting(t);
                    },
                    null);
                }
                return _cancelSettingCommand;
            }
        }


        private void ConfirmSetting(object t)
        {
            var modalWindow = t as IModalWindow;

            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();

            string userPwdModifyServiceUrl = ServiceHelper.GetCurrentPhpServiceURL(ProtocolID.userPwdModify);

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.userPwdModify,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(SystemError),
                RequestDataObj = ServiceHelper.GetModifyAccRequest(OldPassword, NewPassword)
            };

            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                string errorInfo;
                MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, errorInfo);
            }
            else
                if (responseData.WebRequestRes == WebRequestRes.OK)
                {
                    Debug.WriteLine("ModifyPassword: --- OK ---");
                    App.GetPlatformConfig().PlatformAccount = new Common.Account(UserName, NewPassword);
                    App.GetPlatformConfig().Save();
                    modalWindow.Close();
                }
                else
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                    NotificationMessage<string> notification = new NotificationMessage<string>("001", errorInfo);
                    Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, errorInfo);
                }
        }

        private void CancelSetting(object t)
        {
            var modalWindow = t as IModalWindow;
            modalWindow.Close();
        }


        private bool _canApply;
        /// <summary>
        /// 是否通过验证并使能确定按钮
        /// </summary>
        public bool CanApply
        {
            get
            {
                return _canApply;
            }
            set
            {
                if (_canApply != value)
                {
                    _canApply = value;
                    RaisePropertyChanged("CanApply");
                }
            }
        }

        #region Validate 验证

        static readonly string[] ValidatedProperties = { "OldPassword",
            "NewPassword",
            "ConfirmPassword"
        };

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                return GetValidationError(columnName);
            }
        }

        protected string[] _error;

        private string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string result = null;

            if (_error == null)
                _error = new string[3];

            switch (propertyName)
            {
                case "OldPassword":
                    result = this.ValidateUserPassword(this.OldPassword);
                    _error[0] = result;
                    break;
                case "NewPassword":
                    result = this.ValidateUserNewPassword(this.NewPassword);
                    _error[1] = result;
                    break;
                case "ConfirmPassword":
                    result = this.ValidateUserConfirmPassword(this.ConfirmPassword);
                    _error[2] = result;
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

        private string ValidateUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_UserNameError1", out errorInfo);
                return errorInfo;
            }
            return null;
        }

        private string ValidateUserPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_PasswordError1", out errorInfo);
                return errorInfo;
            }
            else
                if (password != _platformService.GetPlatformAccount().Password)
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_PasswordError2", out errorInfo);
                    return errorInfo;
                }
            return null;
        }

        private string ValidateUserNewPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_PasswordError4", out errorInfo);
                return errorInfo;
            }
            Regex rg = new Regex(@"^[0-9a-zA-Z`~!@#$%\^&*()_+-={}|\[\]:"";'<>?,.]{6,16}$");
            Match mat = rg.Match(password);
            if (!mat.Success)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_PasswordError4", out errorInfo);
                return errorInfo;
            }

            return null;
        }

        private string ValidateUserConfirmPassword(string password)
        {
            if (password != this.NewPassword)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_PasswordError3", out errorInfo);
                return errorInfo;
            }
            return null;
        }
        #endregion

    }
}
