using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Platform.Model;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using Nova.NovaWeb.McGo.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nova.NovaWeb.McGo.Platform.View
{
    /// <summary>
    /// PublishManageView.xaml 的交互逻辑
    /// </summary>
    public partial class PublishManageView : UserControl
    {
        public PublishManageView()
        {
            InitializeComponent();
            this.Loaded += PublishManageView_Loaded;
        }

       
        void PublishManageView_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = this.TryFindResource("dataGrid") as DataGrid;

            if (dataGrid == null)
                throw new InvalidCastException();

            var mainViewModel = App.Current.MainWindow.DataContext as MainViewModel;

            var ContextHeaderMenu = this.TryFindResource("EmergencyTerminalColumnHeaderMenu") as ContextMenu;
            if (ContextHeaderMenu == null)
                return;

            var ContextRowMenu = this.TryFindResource("EmergencyTerminalRowMenu") as ContextMenu;
            if (ContextRowMenu == null)
                return;

            mainViewModel.ColumnHeaderContextMenu = null;
            mainViewModel.RowContextMenu = null;

            mainViewModel.ColumnHeaderContextMenu = ContextHeaderMenu;
            mainViewModel.RowContextMenu = ContextRowMenu;

            mainViewModel.LogButton = false;
            mainViewModel.MonitorButton = false;
            mainViewModel.PublishButton = true;
            mainViewModel.TerminalButton = false;
            mainViewModel.PlayingScheduleColumn = true;
            mainViewModel.DownloadingScheduleColumn = true;
            mainViewModel.ScreenShotColumn = false;
            mainViewModel.TerminalAlarm = false;
            mainViewModel.TerminalNotAlarm = true;
            if (this.PublishControl.Content != null)
            {
                this.PublishControl.Content = null;
            }
           
            this.PublishControl.Content = dataGrid;
           
        }

       
        private void ColumnHeader_RightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridColumnHeader dgr = sender as DataGridColumnHeader;
            TerminalRepositoryViewModel viewModel = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            if (viewModel == null)
            {
                return;
            }
            if (dgr == null)
            {
                viewModel.GroupingName = string.Empty;//  dgr.Column.SortMemberPath
            }
            else
            {
                viewModel.GroupingName = dgr.Column.SortMemberPath;
            }
        }

        private void terminalDataGrid1_Sorting(object sender, DataGridSortingEventArgs e)
        {           
            
        }

        private void PublishControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.PublishControl.Content = null;
        }


    }
}
