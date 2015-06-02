using GalaSoft.MvvmLight.Messaging;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.Model;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using Nova.NovaWeb.McGo.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nova.NovaWeb.McGo.Platform.View
{
    /// <summary>
    /// LoginConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginConfigView : NebulaWindow, IModalWindow
    {
        public LoginConfigView()
        {
            InitializeComponent();
            this.Closing += LoginConfigView_Closing;
            Messenger.Default.Register<NotificationMessage<string>>(
              this,
              "LoginConfig",
           message =>
           {
               if (message.Notification.ToUpper() == "OK")
               {
                   this.Close();
               }

           });
        }

        void LoginConfigView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
             Messenger.Default.Unregister(this);
        }

        private void OKButtonClik(object sender, RoutedEventArgs e)
        {
            LoginConfigViewModel viewModel = this.DataContext as LoginConfigViewModel;
            var platform = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            platform.ServerInfo = new ServerInfo() { ServerAddress = string.Format("{0}{1}", "https://", viewModel.ServerIp), CustomerId =viewModel.CustomerId };
            platform.Save();

            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshServerInfo"), "RefreshServerInfo");
            this.Close();
        }
    }
}
