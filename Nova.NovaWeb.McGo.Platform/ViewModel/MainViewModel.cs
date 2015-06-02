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
using Nova.NovaWeb.McGo.Platform.Utilities;
using Nova.NovaWeb.McGo.Platform.View;
using Nova.NovaWeb.Protocol;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private string _userIdentity;
        private MessageInfoViewModel _errorInfo = new MessageInfoViewModel(MessageType.Information, string.Empty);
        private int _moduleSelectedIndex = 2;
        private string _currentLanguage;
        private bool _hasProductionPermissions = false;
        private bool _hasMonitorPermissions = false;
        private bool _hasPublishPermissions = false;
        private bool _hasModluePermissions = true;
        private RelayCommand _loadedWindowCommand;
        private readonly IModalDialogService _modalDialogService;
        private IPlatformService _platformService;
        private IPermission _permissionService;
        private Timer _heartbeatTimer;
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IModalDialogService modalDialogService, IPlatformService platformService, IPermission permissionService)
        {
            _modalDialogService = modalDialogService;
            _platformService = platformService;
            _permissionService = permissionService;

            ServerIp = _platformService.GetPlatformServerInfo().ServerAddress.Replace("https://", string.Empty);
            UserIdentity = _platformService.GetPlatformServerInfo().CustomerId;

            CurrentLanguage = _platformService.GetPlatformLanguage();
            SetPermissions();
            
            Messenger.Default.Register<NotificationMessage<string>>(this, ViewModelLocator.CLEANUP_NOTIFICATION,
            message =>
            {
                this.Cleanup();
            });

            Messenger.Default.Register<NotificationMessage<string>>(
             this,
             "RefreshServerInfo",
            message =>
            {
                if (message.Notification == "RefreshServerInfo")
                {
                    ServerIp = _platformService.GetPlatformServerInfo().ServerAddress.Replace("https://", string.Empty);
                    UserIdentity = _platformService.GetPlatformServerInfo().CustomerId;
                }
            });
            Messenger.Default.Register<NotificationMessage<string>>(
            this,
            "Heartbeat",
            message =>
            {
                if (message.Notification == "Heartbeat")
                {
                    if (_heartbeatTimer == null)
                    {
                        System.Threading.Thread.Sleep(200);
                        double spanTime = _platformService.GetAppData().RefreshFrequency;
                        _heartbeatTimer = new Timer(HeartBeat, null, 100, 30000);
                    }
                }
            });
            Messenger.Default.Register<NotificationMessage>(this, "LogoutAccout", message =>
            {
                //System.Threading.Thread.Sleep(1000);
                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                {
                    Logout(false);
                }));
            });
            AppMessages.RaiseMessageInfoMessage.Register(this, message =>
            {
                RaiseMessageInfo(message);
            });

            LoadProductInfo();
        }

        public IPermission PermissionService
        {
            get
            {
                return _permissionService;
            }
            set
            {
                if (_permissionService == value)
                {
                    return;
                }
                _permissionService = value;
                RaisePropertyChanged("PermissionService");
            }
            
        }
        public string CopyRightInfo { get; set; }
        public string VersionNumber { get; set; }
        public string Title { get; set; }
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
                    string productName = (string)(cpyDataSet.Tables["CopyRightInfo"].Rows[0].ItemArray[3]);

                    CopyRightInfo = string.Format("{0} {1}", productCompany, productCopyRight);
                    VersionNumber = formTitle;
                    Title = productName + " " +VersionNumber;

                }
            }
            catch (Exception ex)
            {

            }
        }


        private bool _unknownflag = false;

        private void RaiseMessageInfo(NotificationMessage<MessageType> message)
        {
            if (message == null)
                return;
            DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
            {
                MessageInfo.Type = message.Content;
                MessageInfo.MessageText = message.Notification;
            }));
        }
        private void SetPermissions()
        {
            DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
            {
                ModuleSelectedIndex = 2;

            }));
            if (_permissionService == null)
            {
                HasPublishPermissions = false;
                HasProductionPermissions = false;
                HasMonitorPermissions = false;
                return;
            }

            DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
            {
                if (_permissionService.PrivilegeList.Count > 0)
                {
                    HasModluePermissions = true;
                }
                else
                {
                    HasModluePermissions = false;
                }

            }));

            

            foreach (var item in _permissionService.PrivilegeList)
            {
                switch (item)
                {
                    case PrivilegeTypes.Publish:
                        HasPublishPermissions = true;
                        HasProductionPermissions = true;
                        DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                        {
                            ModuleSelectedIndex = 0;
                        }));
                        break;
                    case PrivilegeTypes.Monitor:
                        HasMonitorPermissions = true;
                        break;
                    case PrivilegeTypes.Production:
                        HasProductionPermissions = true;
                        DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                        {
                            ModuleSelectedIndex = 1;
                        }));
                        break;
                }
            }
        }
        private string _serverIp;

        public string CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }
            set
            {
                if (value == _currentLanguage)
                    return;
                _currentLanguage = value;
                Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshTerminal"), "RefreshTerminal");
                Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshSchedule"), "RefreshSchedule");
                RaisePropertyChanged("CurrentLanguage");
            }
        }

        public int ModuleSelectedIndex
        {
            get
            {
                return _moduleSelectedIndex;
            }
            set
            {
                _moduleSelectedIndex = value;
                RaisePropertyChanged("ModuleSelectedIndex");
            }
        }

        public string ServerIp
        {
            get
            {
                return _serverIp;
            }
            set
            {
                _serverIp = value;
                RaisePropertyChanged("ServerIp");
            }
        }

        public string UserIdentity
        {
            get
            {
                return _userIdentity;
            }
            set
            {
                _userIdentity = value;
                RaisePropertyChanged("UserIdentity");
            }
        }


/***********************************************************/
        /// <summary>
        /// 快照列是否显示标志位
        /// </summary>
        private bool _screenShotColumn = false;
         public bool ScreenShotColumn  
        {
            get { return _screenShotColumn; }
            set
            {
                if (value == _screenShotColumn)
                {
                    return;
                }
                _screenShotColumn = value;
                RaisePropertyChanged("ScreenShotColumn");
            }
        }

         private bool _terminalAlarm = false;
         public bool TerminalAlarm
         {
             get { return _terminalAlarm; }
             set
             {
                 if (value == _terminalAlarm)
                 {
                     return;
                 }
                 _terminalAlarm = value;
                 RaisePropertyChanged("TerminalAlarm");
             }
         }

         private bool _terminalNotAlarm = false;
         public bool TerminalNotAlarm
         {
             get { return _terminalNotAlarm; }
             set
             {
                 if (value == _terminalNotAlarm)
                 {
                     return;
                 }
                 _terminalNotAlarm = value;
                 RaisePropertyChanged("TerminalNotAlarm");
             }
         } 

        /// <summary>
         /// 正在下载列是否显示标志位
        /// </summary>
          private bool _downloadingScheduleColumn = false;
         public bool DownloadingScheduleColumn  
        {
            get { return _downloadingScheduleColumn; }
            set
            {
                if (value == _downloadingScheduleColumn)
                {
                    return;
                }
                _downloadingScheduleColumn = value;
                RaisePropertyChanged("DownloadingScheduleColumn");
            }
        } 
        /// <summary>
        /// 正在播放列是否显示标志位
        /// </summary>
         private bool _playingScheduleColumn = false;
         public bool PlayingScheduleColumn  
        {
            get { return _playingScheduleColumn; }
            set
            {
                if (value == _playingScheduleColumn)
                {
                    return;
                }
                _playingScheduleColumn = value;
                RaisePropertyChanged("PlayingScheduleColumn");
            }
        } 
        
         private bool _publishButton = false;
         public bool PublishButton  
        {
            get { return _publishButton; }
            set
            {
                if (value == _publishButton)
                {
                    return;
                }
                _publishButton = value;
                RaisePropertyChanged("PublishButton");
            }
        } 

         private bool _terminalButton = false;
         public bool TerminalButton  
        {
            get { return _terminalButton; }
            set
            {
                if (value == _terminalButton)
                {
                    return;
                }
                _terminalButton = value;
                RaisePropertyChanged("TerminalButton");
            }
        } 

         private bool _logButton =false;
         public bool LogButton  
        {
            get { return _logButton; }
            set
            {
                if (value == _logButton)
                {
                    return;
                }
                _logButton = value;
                RaisePropertyChanged("LogButton");
            }
        } 



         private bool _monitorButton = false;
         public bool MonitorButton  
        {
            get { return _monitorButton; }
            set
            {
                if (value == _monitorButton)
                {
                    return;
                }
                _monitorButton = value;
                RaisePropertyChanged("MonitorButton");
            }
        } 
/**********************************************************/

         private ContextMenu _rowContextMenu = null;
         public ContextMenu RowContextMenu
         {
             get { return _rowContextMenu; }
             set
             {
                 if (value == _rowContextMenu)
                 {
                     return;
                 }
                 _rowContextMenu = value;
                 RaisePropertyChanged("RowContextMenu");
             }
         }

         private ContextMenu _columnHeaderContextMenu = null;
         public ContextMenu ColumnHeaderContextMenu
         {
             get { return _columnHeaderContextMenu; }
             set
             {
                 if (value == _columnHeaderContextMenu)
                 {
                     return;
                 }
                 _columnHeaderContextMenu = value;
                 RaisePropertyChanged("ColumnHeaderContextMenu");
             }
         } 
/****************************************************************/

        public bool HasModluePermissions
        {
            get { return _hasModluePermissions; }
            set
            {
                if (value == _hasModluePermissions)
                {
                    return;
                }
                _hasModluePermissions = value;
                RaisePropertyChanged("HasModluePermissions");
            }
        }

        public bool HasPublishPermissions
        {
            get
            {
                return _hasPublishPermissions;
            }
            set
            {
                if (value == _hasPublishPermissions)
                    return;
                _hasPublishPermissions = value;
                RaisePropertyChanged("HasPublishPermissions");
            }
        }

        public bool HasMonitorPermissions
        {
            get
            {
                return _hasMonitorPermissions;
            }
            set
            {
                if (value == _hasMonitorPermissions)
                    return;
                _hasMonitorPermissions = value;
                RaisePropertyChanged("HasMonitorPermissions");
            }
        }

        public bool HasProductionPermissions
        {
            get
            {
                return _hasProductionPermissions;
            }
            set
            {
                if (value == _hasProductionPermissions)
                    return;
                _hasProductionPermissions = value;
                RaisePropertyChanged("HasProductionPermissions");
            }
        }

        /// <summary>
        /// 状态栏通知显示信息，分为告警信息、错误信息和提示信息。
        /// 主要用于将系统级消息显示在状态栏。
        /// </summary>
        public MessageInfoViewModel MessageInfo
        {
            get
            {
                return _errorInfo;
            }
            set
            {
                _errorInfo = value;
                RaisePropertyChanged("MessageInfo");
            }
        }

        /// <summary>
        /// 窗口初始化处理。
        /// 主要处理：
        /// 1.终端列表刷新
        /// 2.激活心跳
        /// </summary>
        public RelayCommand LoadedWindowCommand
        {
            get
            {
                if (_loadedWindowCommand == null)
                {
                    _loadedWindowCommand = new RelayCommand(() =>
                    {
                        SetPermissions();
                        // PreviewHelper.GetInstance();
                        Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshTerminal"), "RefreshTerminal");

                        Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "Heartbeat"), "Heartbeat");

                    },
                    null);
                }
                return _loadedWindowCommand;
            }
        }


        private RelayCommand<object> _closingWindowCommand;
        /// <summary>
        /// 窗口关闭前处理业务
        /// </summary>
        public RelayCommand<object> ClosingWindowCommand
        {
            get
            {
                if (_closingWindowCommand == null)
                {
                    _closingWindowCommand = new RelayCommand<object>((o) =>
                    {
                        ClosingWindow(o);
                    },
                    null);
                }
                return _closingWindowCommand;
            }
        }

        private RelayCommand _manageServerCommand;

        /// <summary>
        /// 调用服务端管理命令
        /// </summary>
        public RelayCommand ManageServerCommand
        {
            get
            {
                if (_manageServerCommand == null)
                {
                    _manageServerCommand = new RelayCommand(() =>
                    {
                        ManageServer();
                    },
                    null);
                }
                return _manageServerCommand;
            }
            set
            {
                _manageServerCommand = value;
            }
        }

        private RelayCommand _openAboutCommand;
        /// <summary>
        /// 打开“关于”窗口
        /// </summary>
        public RelayCommand OpenAboutCommand
        {
            get
            {
                if (_openAboutCommand == null)
                {
                    _openAboutCommand = new RelayCommand(() =>
                    {
                        OpenAbout();
                    },
                    null);
                }
                return _openAboutCommand;
            }
            set
            {
                _openAboutCommand = value;
            }
        }

        private RelayCommand _configerWorkplaceCommand;
        /// <summary>
        /// 配置用户自定义设置。
        /// 1.工作区设置
        /// 2.终端状态自动刷新设置
        /// </summary>
        public RelayCommand ConfigerWorkplaceCommand
        {
            get
            {
                if (_configerWorkplaceCommand == null)
                {
                    _configerWorkplaceCommand = new RelayCommand(() =>
                    {
                        OpenConfigWorkspace();
                    },
                    null);
                }
                return _configerWorkplaceCommand;
            }
        }

        private RelayCommand _modifyUserPasswordCommand;
        /// <summary>
        /// 修改用户密码
        /// </summary>
        public RelayCommand ModifyUserPasswordCommand
        {
            get
            {
                if (_modifyUserPasswordCommand == null)
                {
                    _modifyUserPasswordCommand = new RelayCommand(() =>
                    {
                        ModifyUserPassword();
                    },
                    null);
                }
                return _modifyUserPasswordCommand;
            }
        }

        private RelayCommand<object> _changeLanguageCommand;
        /// <summary>
        /// 更改语言
        /// </summary>
        public RelayCommand<object> ChangeLanguageCommand
        {
            get
            {
                if (_changeLanguageCommand == null)
                {
                    _changeLanguageCommand = new RelayCommand<object>((T) =>
                    {
                        ChangeLanguage(T);
                    },
                    null);
                }
                return _changeLanguageCommand;
            }
        }


        private RelayCommand<object> _exitAndLogoutCommand;
        /// <summary>
        /// 关闭&注销
        /// </summary>
        public RelayCommand<object> ExitAndLogoutCommand
        {
            get
            {
                if (_exitAndLogoutCommand == null)
                {
                    _exitAndLogoutCommand = new RelayCommand<object>((o) =>
                    {
                        var window = o as MainWindow;
                        if(window != null)
                            window.Close();                        
                    },
                    null);
                }
                return _exitAndLogoutCommand;
            }
        }


        private void ClosingWindow(object o)
        {
            if (_unknownflag)
            {
                ViewModelLocator.Cleanup();
                App.StartupComponent();
                return;
            }

            CancelEventArgs  args = o as System.ComponentModel.CancelEventArgs;

            var dialog = AppEnvionment.Default.Get<IModalWindow>("QuitView");

            var dialogViewModel = new QuitViewModel();

            _modalDialogService.ShowDialog(dialog, dialogViewModel);

            if (dialogViewModel.Model == QuitEnum.Logout)
            {
                if (_heartbeatTimer != null)
                {
                    App._isLogOff = true;
                    _heartbeatTimer.Dispose();
                }
                Debug.WriteLine("*******timer已停止******");
                App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                {
                    Messenger.Default.Send<NotificationMessage>(new NotificationMessage("LogoutAccout"), "LogoutAccout");
                }));
            }
            else
                if (dialogViewModel.Model == QuitEnum.Exit)
                {
                    /************/
                    Debug.WriteLine("进程正常退出- _ -");
                    /************/

                    LogoutAccount();
                    PreviewHelper.GetInstance().Dispose();
                    System.Threading.Thread.Sleep(1000);
                    System.Environment.Exit(0);
                }
                else
                {
                    args.Cancel = true;
                }
        }
        
        private void LogoutAccount()
        {
            string timeSyncServiceUrl = ServiceHelper.GetCurrentPhpServiceURL(ProtocolID.userLogoff);

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.userLogoff,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(SystemError),
                RequestDataObj = ServiceHelper.GetLogoutRequest()
            };

            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                string errorInfo;
                MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, errorInfo);
                }));
                return;
            }
            else
                if (responseData.WebRequestRes == WebRequestRes.OK)
                {
                    Debug.WriteLine("LogoutAccount: --- OK ---");
                    ViewModelLocator.Cleanup();
                }
                else
                {
                    ViewModelLocator.Cleanup();
                }
        }

        /// <summary>
        /// 发送及处理系统心跳
        /// </summary>
        /// <param name="o"></param>
        private void HeartBeat(object o)
        {
            /**********/
            Debug.WriteLine("我已经发送了一次心跳");
            /*********/

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.userHeartbeat,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(SystemError),
                RequestDataObj = new object()
            };

            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                if (App._isLogOff)
                {
                    Debug.WriteLine("******服务器说账号过期了*****_isLogOff=" + App._isLogOff.ToString());
                    return;
                }

                if (responseData.SysCode == SysErrorCode.Error_VerifyAcc_UserOverdueOrNotExist)
                {
                    if (_heartbeatTimer != null)
                        _heartbeatTimer.Dispose();

                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        string errorInfo;
                        MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                        MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, errorInfo);
                        if (result == MessageBoxResult.OK)
                        {
                            doOverdue(responseData);
                        }

                    }));
                    
                }
                else
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                    AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error, errorInfo));
                }

                if (responseData.SysCode == SysErrorCode.Error_VerifyAcc_UserOverdueOrNotExist)
                {
                    
                }

              
            }
            else
                if (responseData.WebRequestRes == WebRequestRes.OK)
                {
                    string info;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_SuccessHitBeat", out info);//"Successful communication with the server"
                    AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Information, info));
                }
                else
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                    AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error, errorInfo));
                }

        }

        public void doOverdue(RequestInfo responseData)
        {
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage("LogOutMessage"), "LogOutMessage");
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage("AutoLogoutAccout"), "AutoLogoutAccout");
            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            {
                Debug.WriteLine("*****没有别的窗口弹出来******");
                _unknownflag = true;
                Messenger.Default.Send<NotificationMessage>(new NotificationMessage("LogoutAccout"), "LogoutAccout");
            }));
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        public override void Cleanup()
        {
            // Clean up if needed
            base.Cleanup();
            Messenger.Default.Unregister(this);
            if (_heartbeatTimer != null)
            {
                _heartbeatTimer.Dispose();
                _heartbeatTimer = null;
            }
        }

        /// <summary>
        /// 服务端配置
        /// </summary>
        private void ManageServer()
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();


            if (serverInfo == null)
                return;
            AppData appData = _platformService.GetAppData();//http://localhost:81/Deploy/index.php/Public/doLogin?
            string url = String.Format("{0}/{1}/index.php/Public/doLogin?token={2}&l={3}", serverInfo.ServerAddress, serverInfo.CustomerId, appData.Token, _platformService.GetPlatformLanguage());

            System.Diagnostics.Process.Start("iexplore.exe", url);
        }


        private void OpenAbout()
        {
            var dialog = AppEnvionment.Default.Get<IModalWindow>("AboutView");

            var dialogViewModel = new AboutViewModel();


            _modalDialogService.ShowDialog(dialog, dialogViewModel);
        }

        private void OpenConfigWorkspace()
        {
            var dialog = AppEnvionment.Default.Get<IModalWindow>("ConfigWorkspaceView");

            var dialogViewModel = new ConfigWorkspaceViewModel(AppEnvionment.Default.Get<IPlatformService>());


            _modalDialogService.ShowDialog(dialog, dialogViewModel);
        }

        private void ModifyUserPassword()
        {
            var dialog = AppEnvionment.Default.Get<IModalWindow>("ModifyPasswordView");

            var dialogViewModel = new ModifyPasswordViewModel(AppEnvionment.Default.Get<IPlatformService>());
            _modalDialogService.ShowDialog(dialog, dialogViewModel);
        }

        private void ToolTipChange()
        {
            if (ViewModelLocator.Instance.PublishManage.IsBatch == false)
            {

                ViewModelLocator.Instance.PublishManage.SelectModelHint = MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_Batch", "批量模式");
            }
            else
            {

                ViewModelLocator.Instance.PublishManage.SelectModelHint = MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_NoBatch", "非批量模式");
            }
        }

        private void ChangeLanguage(object obj)
        {
            string markStr = obj as string;
            if (markStr == "中文")
            {
                App.GetPlatformConfig().PlatformLanguage = "zh-cn";
                App.GetPlatformConfig().Save();
                CurrentLanguage = "zh-cn";
                AppMessages.ChangeLanguageMessage.Send("zh-cn");
                ToolTipChange();
            }
            else
                if (markStr == "English")
                {
                    App.GetPlatformConfig().PlatformLanguage = "en";
                    App.GetPlatformConfig().Save();
                    CurrentLanguage = "en";
                    AppMessages.ChangeLanguageMessage.Send("en");
                    ToolTipChange();
                }
                else
                    if (markStr == "español")
                    {
                        App.GetPlatformConfig().PlatformLanguage = "es";
                        App.GetPlatformConfig().Save();
                        CurrentLanguage = "es";
                        AppMessages.ChangeLanguageMessage.Send("es");
                        ToolTipChange();
                    }
        }

        private void Logout(bool isShowMessage = true)
        {

            if (isShowMessage)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_Error4", out errorInfo);
                string errorInfo1;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_Information", out errorInfo1);
                if (Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, errorInfo + "？", errorInfo1, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    PreviewHelper.GetInstance().Dispose();
                    LogoutAccount();
                    //ViewModelLocator.Cleanup();
                    App.StartupComponent();
                }
            }
            else
            {
                PreviewHelper.GetInstance().Dispose();
                LogoutAccount();
                if (_unknownflag == true)
                {
                    App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    App.Current.MainWindow.Close();
                    return;
                }
                //ViewModelLocator.Cleanup();
                App.StartupComponent();
            }
        }
    }
}