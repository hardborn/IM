using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using Nova.NovaWeb.McGo.Windows;
using Nova.NovaWeb.Protocol;
using Nova.Security;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Platform.Model;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Controls;
using System;
using System.Windows.Media;
using System.Data;
using GalaSoft.MvvmLight.Threading;

namespace Nova.NovaWeb.McGo.Platform
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NebulaWindow
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            //Closing += (s, e) => ViewModelLocator.Cleanup();
        }


        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var mainViewModel = this.DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                if (mainViewModel.HasPublishPermissions)
                {
                    var publishManagementTab = FindModlueTab("publishManagementTab");
                    if (publishManagementTab != null)
                    {
                        this.modluesContext.Items.Add(publishManagementTab);
                    }
                    var scheduleManagementTab = FindModlueTab("scheduleManagementTab");
                    if (scheduleManagementTab != null)
                    {
                        this.modluesContext.Items.Add(scheduleManagementTab);
                    }
                }
                if (mainViewModel.HasProductionPermissions)
                {
                    var scheduleManagementTab = FindModlueTab("scheduleManagementTab");
                    if (scheduleManagementTab != null)
                    {
                        if (!this.modluesContext.Items.Contains(scheduleManagementTab))
                        {
                            this.modluesContext.Items.Add(scheduleManagementTab);
                        }
                    }
                }
                if (mainViewModel.HasMonitorPermissions)
                {
                    var logTab = FindModlueTab("logTab");
                    if (logTab != null)
                    {
                        this.modluesContext.Items.Add(logTab);
                    }
                    var terminalControlTab = FindModlueTab("terminalControlTab");
                    if (terminalControlTab != null)
                    {
                        this.modluesContext.Items.Add(terminalControlTab);
                    }
                    var monitoringTab = FindModlueTab("monitoringTab");
                    if (monitoringTab != null)
                    {
                        this.modluesContext.Items.Add(monitoringTab);
                    }
                }

            }
        }

        private object FindModlueTab(string resourceKey)
        {
            var modlueTab = this.TryFindResource(resourceKey);
            return modlueTab;
        }

        private void ColumnHeader_RightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridColumnHeader dgr = sender as DataGridColumnHeader;
            TerminalRepositoryViewModel viewModel = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            if (viewModel == null)
            {
                return;
            }

            if (dgr.Column.SortMemberPath == "CheckBoxChecked")
            {
                dgr.ContextMenu = null;
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

        private void sourceDatagrid_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {

        
            DataGrid dataGrid = this.TryFindResource("dataGrid") as DataGrid;
            if (ViewModelLocator.Instance.TerminalCollection.TerminalListView.Groups != null)
            {
                dataGrid.SelectedItems.Clear();
                ViewModelLocator.Instance.TerminalCollection.TerminalListView.Refresh();
                dataGrid.SelectedItems.Clear();
            }
            
            ViewModelLocator.Instance.TerminalCollection.TerminalListView.CurrentChanged += TerminalListView_CurrentChanged;
        }

        void TerminalListView_CurrentChanged(object sender, EventArgs e)
        {
            ViewModel.ViewModelLocator.Instance.TerminalCollection.RecoverySelectedItems();
        }

        private void sourceDatagrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            
        }

        private void exp_Expanded(object sender, RoutedEventArgs e)
        {

        }

        private void sourceDatagrid_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void sourceDatagrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            Debug.WriteLine("PreviewMouseDown");
            if (ViewModel.ViewModelLocator.Instance.PublishManage.IsBatch == true)
            {
                return;
            }
         //对批量模式下的回读部分进行了特殊处理，判断是否为回读图片
            var clickobject = e.OriginalSource;
            Image clickImage = clickobject as Image;
            if (clickImage != null)
            {
                if (clickImage.Source.ToString() == "pack://application:,,,/MC-go;component/Resources/Images/edit_basic.png")
                {
                    var rowobj = GetVisualParentByType((FrameworkElement)e.OriginalSource, typeof(DataGridRow)) as DataGridRow;
                    if (rowobj.IsSelected == true)
                    {
                        return;
                    }
                }
            }
            //批量模式下的选择与取消
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var row = GetVisualParentByType((FrameworkElement)e.OriginalSource, typeof(DataGridRow)) as DataGridRow;
                if (row != null)
                {
                    row.IsSelected = !row.IsSelected;

                    System.Windows.Controls.DataGrid terminalDataGrid = e.Source as System.Windows.Controls.DataGrid;
                        if (terminalDataGrid == null)
                        {
                            return;
                        }

                        TerminalViewModel viewModel = row.DataContext as TerminalViewModel;
                    if (viewModel == null)
                        return;
                    if (terminalDataGrid.SelectedItems == null)
                        return;
                    TerminalRepositoryViewModel repositoryViewModel = terminalDataGrid.DataContext as TerminalRepositoryViewModel;
                    if (repositoryViewModel == null)
                        return;

                    if (!repositoryViewModel.SelectedTerminals.Contains(viewModel))
                    {
                        repositoryViewModel.SelectedTerminals.Add(viewModel);
                        viewModel.CheckBoxChecked = true;
                    }
                    else
                    {
                        repositoryViewModel.SelectedTerminals.Remove(viewModel);
                        viewModel.CheckBoxChecked = false;
                    }

                    ViewModelLocator.Instance.TerminalCollection.ControlListChange();
                    e.Handled = true;
                }
            }
            else
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var row = GetVisualParentByType((FrameworkElement)e.OriginalSource, typeof(DataGridRow)) as DataGridRow;
                if (row != null && row.IsSelected == false)
                {
                    row.IsSelected = !row.IsSelected;

                    System.Windows.Controls.DataGrid terminalDataGrid = e.Source as System.Windows.Controls.DataGrid;
                    if (terminalDataGrid == null)
                    {
                        return;
                    }

                    TerminalViewModel viewModel = row.DataContext as TerminalViewModel;
                    if (viewModel == null)
                        return;
                    if (terminalDataGrid.SelectedItems == null)
                        return;
                    TerminalRepositoryViewModel repositoryViewModel = terminalDataGrid.DataContext as TerminalRepositoryViewModel;
                    if (repositoryViewModel == null)
                        return;

                    if (!repositoryViewModel.SelectedTerminals.Contains(viewModel))
                    {
                        repositoryViewModel.SelectedTerminals.Add(viewModel);
                        viewModel.CheckBoxChecked = true;
                    }
                    else
                    {
                        repositoryViewModel.SelectedTerminals.Remove(viewModel);
                        viewModel.CheckBoxChecked = false;
                    }

                    ViewModelLocator.Instance.TerminalCollection.ControlListChange();
                    e.Handled = true;
                }
            }
        }
        public static DependencyObject GetVisualParentByType(DependencyObject startObject, Type type)
        {
            DependencyObject parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

    }
}