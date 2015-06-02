using System.IO;
using System.Runtime.Serialization.Json;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.Model;
using Nova.NovaWeb.McGo.Platform.Service;
using Nova.NovaWeb.McGo.Platform.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Nova.NovaWeb.Protocol;
using Nova.Security;
using Nova.NovaWeb.McGo.Utilities;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class LoginViewModel : ViewModelBase, IDataErrorInfo
    {
        private const string UserNamePropertyName = "UserName";
        private const string UserPasswordPropertyName = "UserPassword";
        private const string AccountPropertyName = "Account";
        private const string IsBusyPropertyName = "IsBusy";

        private string _userName = string.Empty;
        private string _userPassword = string.Empty;
        private Account _account;
        private bool _isBusy;
        private bool _isLogon;

        private BackgroundWorker _worker;

        private IAccountAuthenticationService _accountAuthenticationService;
        private IModalDialogService _modalDialogService;
        private IPlatformService _platformService;

        private RelayCommand<object> _loginCommand;
        private RelayCommand _cancelLoginCommand;
        private ICommand _configLoginCommand;
        private ICommand _closeLoginCommand;
        private ICommand _minimizeCommand;

        public event EventHandler LoginCompleted;

        //internal delegate void HttpPostDelegate(string postData, out string replyData, out string errorData);

        public LoginViewModel(IAccountAuthenticationService accountAuthenticationService,
            IModalDialogService modalDialogService)
        {
            

            _accountAuthenticationService = accountAuthenticationService;
            _modalDialogService = modalDialogService;
            _platformService = AppEnvionment.Default.Get<IPlatformService>();

            checkServerIP(_platformService);

            UserName = ((PlatformConfig)AppEnvionment.Default["PlatformConfig"]).PlatformAccount.Name;

            Messenger.Default.Register<NotificationMessage<string>>(this, ViewModelLocator.CLEANUP_NOTIFICATION,
            message =>
            {
                this.Cleanup();
            });
        }

        public void checkServerIP(IPlatformService platformService)
        {
            String CHServer = null;
            String USServer = null;
            String DEServer = null;
            String serverIp = null;
            String customerId = null;
            String errorInfo = null;

            MultiLanguageUtils.GetLanguageString("Editable_ServerIP_CH", out CHServer);
            MultiLanguageUtils.GetLanguageString("Editable_ServerIP_US", out USServer);
            MultiLanguageUtils.GetLanguageString("Editable_ServerIP_DE", out DEServer);

            MultiLanguageUtils.GetLanguageString("IP_Not_Match", out errorInfo);

            ServerInfo serverInfo = platformService.GetPlatformServerInfo();
            serverIp = string.IsNullOrEmpty(serverInfo.ServerAddress) ? string.Empty: serverInfo.ServerAddress.Replace("https://",string.Empty);
            customerId = serverInfo.CustomerId;
            if(!(serverIp == CHServer || serverIp == USServer || serverIp == DEServer))
            {
                if (!string.IsNullOrEmpty(serverIp))
                {
                    MessageBox.Show(serverIp + " " + errorInfo);
                }
                serverIp = CHServer;
                try
                {
                    var platform = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
                    platform.ServerInfo = new ServerInfo() { ServerAddress = string.Format("{0}{1}", "https://", serverIp), CustomerId = customerId };
                    platform.Save();
                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshServerInfo"), "RefreshServerInfo");
                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "OK"), "LoginConfig");
                }
                catch (Exception e)
                {                
                    throw new Exception(e.StackTrace);
                }
            }

            if (string.IsNullOrEmpty(customerId))
            {
                MultiLanguageUtils.GetLanguageString("Editable_customerID", out customerId);
                try
                {
                    var platform = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
                    platform.ServerInfo = new ServerInfo() { ServerAddress = string.Format("{0}{1}", "https://", serverIp), CustomerId = customerId };
                    platform.Save();
                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshServerInfo"), "RefreshServerInfo");
                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "OK"), "LoginConfig");
                }
                catch (Exception e)
                {
                    throw new Exception(e.StackTrace);
                }
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Messenger.Default.Unregister(this);
            if (_worker != null)
            {
                _worker.Dispose();
                _worker = null;
            }
        }

        public Account Account
        {
            get
            {
                return _account;
            }
            set
            {
                if (value == _account)
                {
                    return;
                }
                _account = value;
                RaisePropertyChanged(AccountPropertyName);
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    RaisePropertyChanged(UserNamePropertyName);
                }
            }
        }


        public string UserPassword
        {
            get
            {
                return _userPassword;
            }
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    RaisePropertyChanged(UserPasswordPropertyName);
                }
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (_isBusy == value)
                {
                    return;
                }
                _isBusy = value;
                RaisePropertyChanged(IsBusyPropertyName);
            }
        }

        public RelayCommand<object> LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand<object>(
                        (t) =>
                    {
                        Login(t);
                    });
                }
                return _loginCommand;
            }
        }

        public RelayCommand CancelLoginCommand
        {
            get
            {
                if (_cancelLoginCommand == null)
                {
                    _cancelLoginCommand = new RelayCommand(
                        () =>
                    {
                        CancelLogin();
                    });
                }
                return _cancelLoginCommand;
            }
        }

        private void Login(object sender)
        {
            LoginView loginWindow = sender as LoginView;

            if (loginWindow == null)
                return;

            _userPasswordHasChanged = true;
            _userNameHasChanged = true;

            BindingExpression nameBE = loginWindow.userNameTextBox.GetBindingExpression(TextBox.TextProperty);
            if (nameBE != null)
            {
                nameBE.UpdateSource();
            }
            BindingExpression passwordBE = loginWindow.passwordBox.GetBindingExpression(PasswordHelper.PasswordProperty);
            if (passwordBE != null)
            {
                passwordBE.UpdateSource();
            }

            if (nameBE.HasError || passwordBE.HasError)
            {
                return;
            }

            _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += (o, ea) =>
            {

                Messenger.Default.Register<NotificationMessage<string>>(
                this,
                "Login",
                message =>
                {
                    if (message.Content == "Exception")
                        DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                        {
                            Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,message.Notification);
                        }));
                    Messenger.Default.Unregister(this);
                });


                //DateTime time =  System.Convert.ToDateTime(new DateTime(2013,11,14,16,20,46,DateTimeKind.Utc)).ToLocalTime();
                _isLogon = _accountAuthenticationService.VerifyPlatformAccount(new Account(_userName, _userPassword));
                //_isLogon = true;


                if (_isLogon)
                {
                    //System.Threading.Thread.Sleep(300);
                    App.GetPlatformConfig().PlatformAccount = new Common.Account(UserName, UserPassword);
                    App.GetPlatformConfig().Save();

                    SynchronizePlatformTimeTime();
                    // Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshTerminal"), "RefreshTerminal");
                    //Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "Heartbeat"), "Heartbeat");
                }

                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                {
                    if (_worker != null && !_worker.CancellationPending)
                        Messenger.Default.Send<bool?>(_isLogon);
                }));

            };

            _worker.RunWorkerCompleted += (o, ea) =>
            {
                IsBusy = false;

                if (_worker != null)
                {
                    _worker.Dispose();
                    _worker = null;
                }
                Messenger.Default.Unregister(this);
            };

            IsBusy = true;
            _worker.RunWorkerAsync();
        }

        private void SynchronizePlatformTimeTime()
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.timeSync,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(TimeSyncReply),
                RequestDataObj = ServiceHelper.GetTimeSyncRequest()
            };
            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                string errorInfo;
                MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                NotificationMessage<string> notification = new NotificationMessage<string>("Exception", errorInfo);
                Messenger.Default.Send<NotificationMessage<string>>(notification, "Login");
                return;
            }
            else
                if (responseData.WebRequestRes == WebRequestRes.OK)
                {
                    try
                    {
                        var timeSyncReply = responseData.ReplyObj as TimeSyncReply;
                        if (timeSyncReply != null)
                        {
                            timeSyncReply.Time = timeSyncReply.Time;
                            DateTimeHelper.SetSystemClock(
                                (short)timeSyncReply.Time.Year,
                                (short)timeSyncReply.Time.Month,
                                (short)timeSyncReply.Time.Day,
                                (short)timeSyncReply.Time.Hour,
                                (short)timeSyncReply.Time.Minute,
                                (short)timeSyncReply.Time.Second);
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
                else
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                    NotificationMessage<string> notification = new NotificationMessage<string>("Exception", errorInfo);
                    Messenger.Default.Send<NotificationMessage<string>>(notification, "Login");
                    return;
                }
        }

        private void CancelLogin()
        {
            _worker.CancelAsync();
            IsBusy = false;
            _worker.Dispose();
            _worker = null;
            Messenger.Default.Unregister(this);
        }

        private void CloseLongin()
        {
            Environment.Exit(0);
        }

        public ICommand ConfigServerCommand
        {
            get
            {
                if (_configLoginCommand == null)
                {
                    _configLoginCommand = new RelayCommand<object>(
                        (T) =>
                    {
                        OpenServerConfig(T);
                    },
                        null);
                }
                return _configLoginCommand;
            }
        }

        private void OpenServerConfig(object sender)
        {
            var dialog = AppEnvionment.Default.Get<IModalWindow>("LoginConfigView");

            var dialogViewModel = new LoginConfigViewModel(AppEnvionment.Default.Get<IPlatformService>());
                      

            _modalDialogService.ShowDialog(dialog, dialogViewModel);
            
        }



        private bool _userNameHasChanged = false;
        private bool _userPasswordHasChanged = false;

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

        static readonly string[] ValidatedProperties = { UserNamePropertyName,
            UserPasswordPropertyName
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
                _error = new string[2];

            switch (propertyName)
            {
                case UserNamePropertyName:
                    if (_userNameHasChanged)
                    {
                        result = this.ValidateUserName(this.UserName);
                        _error[0] = result;
                    }
                    break;
                case UserPasswordPropertyName:
                    if (_userPasswordHasChanged)
                    {
                        result = this.ValidateUserPassword(this.UserPassword);
                        _error[1] = result;
                    }
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
                if (password.Length < 6)
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_PasswordError4", out errorInfo);
                    return errorInfo;
                }
            return null;
        }

        #endregion

    }
}
