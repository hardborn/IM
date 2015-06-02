/*
 In App.xaml:
 <Application.Resources>
     <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:Nova.NovaWeb.McGo.Platform.ViewModel"
                                  x:Key="Locator" />
 </Application.Resources>
  
 In the View:
 DataContext="{Binding Source={x:Static viewModel:ViewModelLocator.Instance}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
//using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.Model;
using Nova.NovaWeb.McGo.Platform.Service;
using Nova.NovaWeb.McGo.Platform.View;
using System;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator:ViewModelBase
    {

        public static string CLEANUP_NOTIFICATION = "Cleanup";


        private static ViewModelLocator uniqueInstance;
        

        // 定义一个标识确保线程同步
        private static readonly object locker = new object();

        // 定义私有构造函数，使外界不能创建该类实例
        private ViewModelLocator()
        {
           //PlatformService platformService = new PlatformService();
           // MockPlatformService platformService = new MockPlatformService();
            //AppEnvionment.Default.InjectAsSingleton<IPlatformService>(platformService);
            AppEnvionment.Default.RegisterAsSingleton<IPlatformService, PlatformService>();
            AppEnvionment.Default.RegisterAsSingleton<IPermission, TempPermission>();
            AppEnvionment.Default.RegisterAsSingleton<IScheduleRepositoryProvider, FlatfileScheduleRepositoryProvider>();

            AppEnvionment.Default.Register<ITransmissionInfoService, FTPTransmissionInfoService>();
            //AppEnvionment.Default.Register<ITransmissionInfoService, TempTransmissionInfoService>();

            AppEnvionment.Default.Register<IAccountAuthenticationService, HttpAccountService>();


            AppEnvionment.Default.RegisterAsSingleton<ILog, TxtLogger>();

            //HttpTerminalRepositoryProvider httpTerminalRepositoryProvider = new HttpTerminalRepositoryProvider();
            //AppEnvionment.Default.InjectAsSingleton<ITerminalRepositoryProvider>(httpTerminalRepositoryProvider);

            AppEnvionment.Default.RegisterAsSingleton<ITerminalRepositoryProvider, HttpTerminalRepositoryProvider>();

            AppEnvionment.Default.RegisterAsSingleton<IGroupRepositoryProvider, HttpGroupRepositoryProvider>();

            AppEnvionment.Default.Register<IModalDialogService, ModalDialogService>();

            AppEnvionment.Default.Register<IModalWindow, LoginConfigView>(typeof(LoginConfigView).Name);

            AppEnvionment.Default.Register<IModalWindow, AboutView>(typeof(AboutView).Name);

            AppEnvionment.Default.Register<IModalWindow, ConfigWorkspaceView>(typeof(ConfigWorkspaceView).Name);

            AppEnvionment.Default.Register<IModalWindow, ModifyPasswordView>(typeof(ModifyPasswordView).Name);
           
            AppEnvionment.Default.Register<IModalWindow, QuitView>(typeof(QuitView).Name);

            AppEnvionment.Default.Register<IModalWindow, ReadbackScheduleView >(typeof(ReadbackScheduleView).Name);



            AppEnvionment.Default.Register<LoginViewModel>();
           // TerminalRepositoryViewModel terminalRepositoryViewModel = new TerminalRepositoryViewModel(httpTerminalRepositoryProvider, AppEnvionment.Default.Get<IGroupRepositoryProvider>(), platformService);
            //AppEnvionment.Default.InjectAsSingleton<TerminalRepositoryViewModel>(terminalRepositoryViewModel);
            AppEnvionment.Default.Register<TerminalRepositoryViewModel>();
            AppEnvionment.Default.Register<MainViewModel>();
            AppEnvionment.Default.Register<ScheduleManageViewModel>();
            AppEnvionment.Default.Register<PublishManageViewModel>();
            AppEnvionment.Default.Register<ControlManageViewModel>();
            AppEnvionment.Default.Register<MonitorManageViewModel>();
            AppEnvionment.Default.Register<LogManageViewModel>();
        }

       public static ViewModelLocator Instance
        {
            get
            {
                if (uniqueInstance == null)
                {
                    lock (locker)
                    {
                        // 如果类的实例不存在则创建，否则直接返回
                        if (uniqueInstance == null)
                        {
                            uniqueInstance = new ViewModelLocator();
                        }
                    }
                }
                return uniqueInstance;
            }
        }

       
        /// <summary>
        /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
        /// </summary>
        /// <returns></returns>
        //public static ViewModelLocator GetInstance()
        //{
        //    // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
        //    // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
        //    // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
        //    // 双重锁定只需要一句判断就可以了
        //    if (uniqueInstance == null)
        //    {
        //        lock (locker)
        //        {
        //            // 如果类的实例不存在则创建，否则直接返回
        //            if (uniqueInstance == null)
        //            {
        //                uniqueInstance = new ViewModelLocator();
        //            }
        //        }
        //    }
        //    return uniqueInstance;
        //}

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                //return ServiceLocator.Current.GetInstance<MainViewModel>();
                return AppEnvionment.Default.Get<MainViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public LoginViewModel Login
        {
            get
            {
                // return ServiceLocator.Current.GetInstance<LoginViewModel>();
                return AppEnvionment.Default.Get<LoginViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ScheduleManageViewModel ScheduleManage
        {
            get
            {
                return AppEnvionment.Default.Get<ScheduleManageViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public PublishManageViewModel PublishManage
        {
            get
            {
                return AppEnvionment.Default.Get<PublishManageViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public ControlManageViewModel ControlManage
        {
            get
            {
                return AppEnvionment.Default.Get<ControlManageViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public MonitorManageViewModel MonitorManage
        {
            get
            {
                return AppEnvionment.Default.Get<MonitorManageViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public LogManageViewModel LogManage
        {
            get
            {
                return AppEnvionment.Default.Get<LogManageViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
        "CA1822:MarkMembersAsStatic",
        Justification = "This non-static member is needed for data binding purposes.")]
        public TerminalRepositoryViewModel TerminalCollection
        {
            get
            {
                var terminalRepository = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
               // RaisePropertyChanged("TerminalCollection");
                return terminalRepository;
            }
        }

      


        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(CLEANUP_NOTIFICATION, CLEANUP_NOTIFICATION), CLEANUP_NOTIFICATION);
            if (uniqueInstance == null)
                return;
            uniqueInstance = null;        

        }

        public static void Dispose()
        {
            if(uniqueInstance == null)
                return ;
            uniqueInstance = null;
        }
    }
}