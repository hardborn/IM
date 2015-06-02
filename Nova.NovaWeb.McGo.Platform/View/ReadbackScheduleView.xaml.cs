using GalaSoft.MvvmLight.Messaging;
using Nova.NovaWeb.McGo.Platform.Model;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using Nova.NovaWeb.McGo.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// ReadbackScheudleView.xaml 的交互逻辑
    /// </summary>
    public partial class ReadbackScheduleView : NebulaWindow,IModalWindow
    {
        public ReadbackScheduleView()
        {
            InitializeComponent();
            //this.Loaded += ReadbackScheudleView_Loaded;
            //this.Closed += ReadbackScheudleView_Closed;
            //Messenger.Default.Register<NotificationMessage<string>>(this, "PublishSuccess", message =>
            //{
            //    this.Close();
            //});
            
        }

        //void ReadbackScheudleView_Closed(object sender, EventArgs e)
        //{
        //    TerminalViewModel terminalViewModel = this.DataContext as TerminalViewModel;
        //    if (terminalViewModel == null)
        //    {
        //        return;
        //    }
        //    terminalViewModel.CancelScheduleDownload();
        //    Messenger.Default.Unregister(this);
        //}

        //bool isSucess = false;


        //void ReadbackScheudleView_Loaded(object sender, RoutedEventArgs e)
        //{
        //    TerminalViewModel terminalViewModel = this.DataContext as TerminalViewModel;
        //    if (terminalViewModel == null)
        //    {
        //        return;
        //    }

        //    terminalViewModel.ScheduleDownloadCompleteEvent += terminalViewModel_ScheduleDownloadCompleteEvent;

        //    BackgroundWorker worker = new BackgroundWorker();
        //    worker.DoWork += (o, ea) =>
        //    {
        //      isSucess = terminalViewModel.GetPlayingScheduleInfo();
               
        //    };

        //    //worker.RunWorkerCompleted += (o, ea) =>
        //    //{
        //    //    if (!isSucess)
        //    //    {
        //    //        this.Close();
        //    //    }
        //    //};

        //    worker.RunWorkerAsync();
        //}

        //void terminalViewModel_ScheduleDownloadCompleteEvent(Nova.NovaWeb.FileSetTransmiteCompleteEventArgs e)
        //{
        //    if (e.TransmiteRes != FileSetTransmiteRes.OK)
        //    {
        //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        {
        //            TerminalViewModel terminalViewModel = this.DataContext as TerminalViewModel;
        //            if (terminalViewModel == null)
        //            {
        //                return;
        //            }

        //            terminalViewModel.ScheduleDownloadCompleteEvent -= terminalViewModel_ScheduleDownloadCompleteEvent;
        //            this.Close();
        //        }));
                
        //    }
        //}

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void head_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
