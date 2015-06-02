using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
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
    /// QuitView.xaml 的交互逻辑
    /// </summary>
    public partial class QuitView : NebulaWindow, IModalWindow
    {
        public QuitView()
        {
            InitializeComponent();
            //Messenger.Default.Register<NotificationMessage>(this, "LogoutAccout", message =>
            //{
            //    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            //    {
            //        var viewModel = this.DataContext as QuitViewModel;
            //        viewModel.Model = QuitEnum.Logout;
            //        this.DialogResult = true;
            //    }));
            //});
         
        }
    }
}
