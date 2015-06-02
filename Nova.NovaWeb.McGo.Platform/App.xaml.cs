using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.Diagnostics;
using System;
using Nova.NovaWeb.McGo.Platform.View;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.DAL;
using Nova.Globalization;
using System.IO;
using System.Collections.Generic;
using Nova.Xml.Serialization;
using Nova.NovaWeb.McGo.Common;
using System.Windows.Controls;

namespace Nova.NovaWeb.McGo.Platform
{
    public partial class App : Application
    {
        private static ILog _log;
        public static bool _isLogOff = false;

        static App()
        {
            DispatcherHelper.Initialize();
        }


        private Visibility _isDebugModel = Visibility.Collapsed;

        public Visibility IsDebugModel
        {
            get
            {
                return _isDebugModel;
            }
            set
            {
                _isDebugModel = value;
            }

        }


        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeComponent();
           
            base.OnStartup(e);

            ///捕获未处理异常
            App.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            SerializableFont.IsSerializaByChinaCulture = true;

            ///启用Winform的可视样式
            System.Windows.Forms.Application.EnableVisualStyles();

            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            //var timeZoneList = TimeZoneInfo.GetSystemTimeZones();

            //注册更新语言环境消息
            AppMessages.ChangeLanguageMessage.Register(this, OnChangeLanguageMessage);
            Messenger.Default.Register<NotificationMessage<string>>(App.Current, "ChangeLanguage", (message) =>
            {
                UpdateLanguageEnvionment(message);
            });

            Messenger.Default.Register<NotificationMessage<string>>(App.Current, "Logout", message =>
            {
                StartupComponent();
            });

           if(ViewModelLocator.Instance.IsInDesignMode)
           { }
           else
           { }

            _log = AppEnvionment.Default.Get<ILog>();

            ///启动组件
            StartupComponent();
        }


        private void OnChangeLanguageMessage(string languageName)
        {
            SystemLangParser.GetInstant().UpdateLanguage(PlatformConfig.LanguageResourcePath, languageName, App.Current.Resources);
            MultiLanguageUtils.LoadLanguage(Path.Combine(PlatformConfig.LanguageResourcePath, languageName, String.Format("ManagementCenter.{0}.xaml", languageName)), App.Current.Resources);
        }
        private void UpdateLanguageEnvionment(NotificationMessage<string> message)
        {
            SystemLangParser.GetInstant().UpdateLanguage(PlatformConfig.LanguageResourcePath, message.Notification, App.Current.Resources);
            MultiLanguageUtils.LoadLanguage(Path.Combine(PlatformConfig.LanguageResourcePath, message.Notification, String.Format("ManagementCenter.{0}.xaml", message.Notification)), App.Current.Resources);
        }


       public static void Log(string info)
        {
            try
            {
                if (EntryPoint.IsDebugModel)
                {
                    if (_log == null)
                        return;
                    _log.Log(info);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
            }
          
        }

       public static void Log(Exception exception)
       {
           try
           {
               if (EntryPoint.IsDebugModel)
               {
                   if (_log == null)
                       return;
                   _log.Log(exception);
               }
           }
           catch (Exception e)
           {
               MessageBox.Show(e.StackTrace);
           }
         
       }

        private static void InitializeConfig()
        {
            App.Log("配置文件初始化...");
            var platformConfig = new PlatformConfig();
            platformConfig.InitializeConfig();
            AppEnvionment.Default["PlatformConfig"] = platformConfig;
            App.Log("配置文件初始化完成...");
        }


        static LoginView login;

        public static void StartupComponent()
        {
            
            try
            {
                //AppDomain.CurrentDomain.GetData("IsDebugModel")
               
                App.Log("开始初始化组件...");

                InitializeConfig();


                // 首先显示登录控件
                //LoginViewModel loginViewModel = new LoginViewModel();
                //login.DataContext = loginViewModel;
                // 将Login设置为主窗体
                if (App.Current.MainWindow == null)
                {
                    login = new LoginView();
                    App.Current.MainWindow = login;
                }
                else
                {
                    Window window = App.Current.MainWindow;
                    login = new LoginView();
                    App.Current.MainWindow = login;
                    App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    //window.Close();
                }
                App.Log("进入登录窗口");
                App.Current.MainWindow.Show();



                Messenger.Default.Register<bool?>(
                                 App.Current,
                m =>
                {
                    // 登录成功后，显示主页面
                    if (m.HasValue && m.Value)
                    {
                        if (login == null)
                            return;
                        _isLogOff = false;
                        App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                        login.Close();
                        login = null;
                        // 更改主窗体
                        App.Current.MainWindow = new MainWindow();
                        App.Log("进入主界面");
                        App.Current.MainWindow.Show();
                        App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    }
                    else
                        if (!m.HasValue)
                        {
                            App.Current.MainWindow.Close();
                        }
                    //Messenger.Default.Unregister<bool?>(App.Current);
                });
                App.Log("初始化组件完成...");
            }
            catch (Exception e)
            {
                App.Log(e);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            App.Log("进行资源回收，退出程序中...");

            var platform = AppEnvionment.Default.Get<IPlatformService>();
            platform.Save();
            Messenger.Default.Unregister(this);
            base.OnExit(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            if (exception != null)
            {
                App.Log(exception);

                //string errorInfo1;
                //MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_Error2", out errorInfo1);
                //string errorInfo2;
                //MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_Error3", out errorInfo2);
                //MessageBox.Show(errorInfo1 + "：\n" + exception.StackTrace + "\n" + errorInfo2);
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //string errorInfo1;
            //MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_Error2", out errorInfo1);
            //string errorInfo2;
            //MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_Error3", out errorInfo2);
            //MessageBox.Show(errorInfo1 + "：\n" + e.Exception.Message + "\n" + errorInfo2);
            App.Log(e.Exception);
            e.Handled = true;
        }

        public static PlatformConfig GetPlatformConfig()
        {
            var config = AppEnvionment.Default["PlatformConfig"] as  PlatformConfig;
            return config;
        }
    }

    //<summary> 
    //Entry point class to handle single instance of the application 
    //</summary> 
    public static class EntryPoint
    {
        private static System.Threading.Mutex mutex;
        public static bool IsDebugModel = false;
        [STAThread]
        public static void Main(string[] args)
        {
            bool can_execute;
            mutex = new System.Threading.Mutex(true, "MC-GO", out can_execute);
            if (can_execute)
            {
                if (args != null && args.Length > 0)
                {
                    foreach (var arg in args)
                    {
                        if (arg.Equals("-debug", StringComparison.CurrentCultureIgnoreCase))
                        {
                            
                            AppDomain.CurrentDomain.SetData("IsDebugModel", true);
                            IsDebugModel = true;
                            break;
                        }
                    }
                }
                //IsDebugModel = true;
                App app = new App();
                if(IsDebugModel == true)
                {
                    app.IsDebugModel = Visibility.Visible;

                }
                else
                {
                    app.IsDebugModel = Visibility.Collapsed;
                }

                app.Run();
            }
            else
            {
                //App.Log("MC-GO已启动！");
                System.Threading.Thread.Sleep(1000);
                System.Environment.Exit(0);
            }
        }
    }
}
