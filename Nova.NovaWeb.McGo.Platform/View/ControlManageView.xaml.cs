using Nova.NovaWeb.McGo.BLL;
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
    /// ControlManageView.xaml 的交互逻辑
    /// </summary>
    public partial class ControlManageView : UserControl
    {
        public ControlManageView()
        {
            InitializeComponent();
            this.Loaded += ControlManageView_Loaded;
        }

    

        void ControlManageView_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = this.TryFindResource("dataGrid") as DataGrid;

            if(dataGrid == null)
                throw new InvalidCastException();

            var mainViewModel = App.Current.MainWindow.DataContext as MainViewModel;

            var ContextHeaderMenu = this.TryFindResource("ControlTerminalColumnHeaderMenu") as ContextMenu;
            if (ContextHeaderMenu == null)
                return;

            var ContextRowMenu = this.TryFindResource("ControlTerminalRowMenu") as ContextMenu;
            if (ContextRowMenu == null)
                return;

            mainViewModel.ColumnHeaderContextMenu = null;
            mainViewModel.RowContextMenu = null;

            mainViewModel.ColumnHeaderContextMenu = ContextHeaderMenu;
            mainViewModel.RowContextMenu = ContextRowMenu;

            mainViewModel.LogButton = false;
            mainViewModel.MonitorButton = false;
            mainViewModel.PublishButton = false;
            mainViewModel.TerminalButton = true;
            mainViewModel.PlayingScheduleColumn = false;
            mainViewModel.DownloadingScheduleColumn = false;
            mainViewModel.ScreenShotColumn = false;
            mainViewModel.TerminalAlarm = false;
            mainViewModel.TerminalNotAlarm = true;
            if (this.TerminalControl.Content != null)
            {
                this.TerminalControl.Content = null;
            }
            try
            {
                this.TerminalControl.Content = dataGrid;
            }
            catch (Exception ex)
            {
               
            }
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

        private void TerminalControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.TerminalControl.Content = null;
        }
    }
}
