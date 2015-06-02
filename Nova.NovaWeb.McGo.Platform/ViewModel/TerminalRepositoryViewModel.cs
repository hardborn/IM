using DevComponents.DotNetBar;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ObjectBuilder2;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.Model;
using Nova.NovaWeb.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Word;
using ICommand = System.Windows.Input.ICommand;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class TerminalRepositoryViewModel : ViewModelBase, ISearchable
    {
        private TerminalViewModel  _selectedTerminal;
        private const string TerminalListPropertyName = "TerminalList";

        private ITerminalRepositoryProvider _terminalRepositoryProvider;
        private IGroupRepositoryProvider _groupRepositoryProvider;
        private IPlatformService _platformService;
        //   private BackgroundWorker _worker;
        private bool _isBusy = true;
        private int _terminalOnlineCount;
        private int _terminalOfflineCount;
        private int _terminalTotalCount;

        private BackgroundWorker _worker;
        private BackgroundWorker _statusWorker;
        private Timer _autoRefreshTimer;
        private IEnumerable<SiteStatus> _terminalStatusList;
        private IEnumerable<Site> _terminalBaseInfoList;
        private IEnumerable<Group> _groupList;

        private string _seachKeyword;
        private SortDescriptionCollection _sortDescriptions = new SortDescriptionCollection();

        private ListCollectionView _allTerminalsView;

        private ObservableCollection<TerminalViewModel> _terminalList = new ObservableCollection<TerminalViewModel>();
        private ObservableCollection<TerminalViewModel> _selectedTerminals = new ObservableCollection<TerminalViewModel>();

        private ICommand _refreshTerminalListCommand;

        private ICommand _selectionChangedCommand;
        private RelayCommand<object> _terminalDataLoadCommand;
        private RelayCommand _cancelDataLoadCommand;
        private RelayCommand _cancelDataPublishLoadCommand;

        private bool? _isGroupSelected;


        private RelayCommand<object> _expandedCommand;

        private RelayCommand<object> _unExpandedCommand;
        public TerminalRepositoryViewModel(ITerminalRepositoryProvider terminalRepositoryProvider, IGroupRepositoryProvider groupRepositoryProvider, IPlatformService platformService)
        {
            if (terminalRepositoryProvider == null)
            {
                throw new ArgumentNullException();
            }
            _terminalRepositoryProvider = terminalRepositoryProvider;
            _groupRepositoryProvider = groupRepositoryProvider;
            _platformService = platformService;

            // RefreshTerminalList();


            Messenger.Default.Register<NotificationMessage<string>>(
              this,
              "RefreshTerminal",
            message =>
            {
                if (message.Notification == "RefreshTerminal")
                {
                    RefreshTerminalList();
                }
            });

            Messenger.Default.Register<NotificationMessage<string>>(
            this,
            "ViewModelLocator",
            message =>
            {
                this.RefreshTerminalList();
            });

            Messenger.Default.Register<NotificationMessage<string>>(
                this,
                "TerminalList",
            message =>
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, message.Notification);
                }));
            });
            Messenger.Default.Register<NotificationMessage<string>>(this, ViewModelLocator.CLEANUP_NOTIFICATION,
            message =>
            {
                this.Cleanup();
            });
//new NotificationMessage("KeyWordChanged"), "KeyWordChanged"
            Messenger.Default.Register<NotificationMessage>(this, "KeyWordChanged", message =>
            {
               // System.Threading.Thread.Sleep(1000);
                Search(message.Notification);
            });

            Messenger.Default.Register<NotificationMessage<AppData>>(
                this,
                "RefreshFrequency",
            message =>
            {
                if (!message.Content.IsAutoStatusRefresh)
                {
                    if (_autoRefreshTimer != null)
                    {
                        _autoRefreshTimer.Dispose();
                    }
                }
                else
                {
                    if (_autoRefreshTimer != null)
                    {
                        long spanTime = (long)message.Content.RefreshFrequency * 1000;
                        _autoRefreshTimer.Change((long)100.0, spanTime);
                    }
                }
            });
        }

        public ListCollectionView TerminalListView
        {
            get
            {
                return _allTerminalsView;
            }
            private set
            {
                _allTerminalsView = value;
                RaisePropertyChanged("TerminalListView");
            }
        }

        public string SearchKeyword
        {
            get
            {
                return _seachKeyword;
            }
            set
            {
                if (_seachKeyword == value)
                {
                    return;
                }
                _seachKeyword = value;
                RaisePropertyChanged("SearchKeyword");
            }
        }


        private void RefreshTerminalList()
        {
            DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
            {
                _terminalList.Clear();
            }));

            _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            // _worker.DoWork -= _worker_DoWork;
            _worker.DoWork += _worker_DoWork;

            _worker.RunWorkerCompleted += (o, ea) =>
            {
                if (_worker != null)
                {
                    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                    {
                        IsBusy = false;
                    }));
                    _worker.Dispose();
                    _worker = null;

                    if (_autoRefreshTimer == null)
                    {
                        if (_platformService.GetAppData().IsAutoStatusRefresh)
                        {
                            double spanTime = _platformService.GetAppData().RefreshFrequency;
                            _autoRefreshTimer = new Timer(RefreshTerminalStatus, null, 100, ((spanTime < 10.0) || (spanTime > 3600)) ? 30000 : (int)(spanTime * 1000));
                        }
                    }
                }

            };
            DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
            {
                IsBusy = true;
            }));

            _worker.RunWorkerAsync();
        }

        private Thread currentThread = null;
        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            currentThread = Thread.CurrentThread;
            GetTerminalData();
        }

        private void GetTerminalData()
        {
            _terminalBaseInfoList = _terminalRepositoryProvider.FindAllTerminalBaseInfo();
            _terminalStatusList = _terminalRepositoryProvider.FindAllTerminalStatusInfo();
            _groupList = _groupRepositoryProvider.FindAllGroup();

            if (_terminalBaseInfoList == null || _terminalStatusList == null || _groupList == null)
                return;

            List<TerminalModel> reslut = new List<TerminalModel>();
            foreach (var groupItem in _groupList)
            {
                foreach (var terminalItem in groupItem.SiteList)
                {
                    var terminalBaseInfo = _terminalBaseInfoList.FirstOrDefault(t => t.Mac == terminalItem.Mac);
                    var terminalStatus  = _terminalStatusList.FirstOrDefault(t => t.Mac == terminalItem.Mac);
                    if (terminalBaseInfo == null || terminalStatus == null)
                        continue;

                    TerminalModel terminalModel = new TerminalModel(groupItem, terminalBaseInfo, terminalStatus);
                    if (!reslut.Contains(terminalModel))
                        reslut.Add(terminalModel);
                }
            }


            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            {
                foreach (var itemModel in reslut)
                {
                    if (!_terminalList.Any(t => t.Mac == itemModel.Mac))
                        _terminalList.Add(new TerminalViewModel(itemModel));
                }
                TerminalOfflineCount = _terminalList.Count(t => t.IsOnline == false);
                TerminalOnlineCount = _terminalList.Count(t => t.IsOnline == true);
                TerminalTotalCount = TerminalOfflineCount + TerminalOnlineCount;
                if (TerminalListView != null)
                {
                    _sortDescriptions.Clear();
                    foreach (var item in TerminalListView.SortDescriptions)
                    {
                        _sortDescriptions.Add(item);
                    }
                }
                TerminalListView = GetAllTerminalsView(_terminalList);
                TerminalListView.GroupDescriptions.Clear();
                TerminalListView.GroupDescriptions.Add(new PropertyGroupDescription(_platformService.GetAppData().TerminalSortField));
                if (_sortDescriptions != null)
                {
                    foreach (var item in _sortDescriptions)
                    {
                        TerminalListView.SortDescriptions.Add(item);
                    }
                }
                TerminalListView.Filter = IsShow;

                SelectedTerminals.Clear();
                TerminalListView.MoveCurrentToFirst();

                SelectedTerminal = TerminalListView.CurrentItem as TerminalViewModel;
                SelectedTerminal = null;
                if (SelectedTerminal != null)
                {
                    SelectedTerminals.Add(SelectedTerminal);
                    SelectedTerminal.CheckBoxChecked = true;
                    GroupCheckChange();
                }
            }));
        }

        private Visibility _showRefreshImage = Visibility.Hidden;
        public Visibility ShowRefreshImage
        {
            get
            {
                return _showRefreshImage;
            }
            set
            {
                if (_showRefreshImage == value)
                {
                    return;
                }
                _showRefreshImage = value;
                RaisePropertyChanged("ShowRefreshImage");
            }
        }

        private Visibility _showImage = Visibility.Hidden;
        public Visibility ShowImage
        {
            get
            {
                return _showImage;
            }
            set
            {
                if (_showImage == value)
                {
                    return;
                }
                _showImage = value;
                RaisePropertyChanged("ShowImage");
            }
        }

        private void RefreshTerminalStatus(object obj)
        {
           
            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            {
                App.Current.MainWindow.Focus();
            }));

            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            {
                ShowRefreshImage = Visibility.Visible;
                ShowImage = Visibility.Collapsed;
            }));
            _statusWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _statusWorker.DoWork += (o, ea) =>
            {

               
                _terminalStatusList = _terminalRepositoryProvider.FindAllTerminalStatusInfo();

                if (_terminalBaseInfoList == null || _terminalStatusList == null || _groupList == null)
                    return;

                List<TerminalModel> reslut = new List<TerminalModel>();
                foreach (var groupItem in _groupList)
                {
                    foreach (var terminalItem in groupItem.SiteList)
                    {
                        var terminalBaseInfo = _terminalBaseInfoList.FirstOrDefault(t => t.Mac == terminalItem.Mac);
                        var terminalStatus = _terminalStatusList.FirstOrDefault(t => t.Mac == terminalItem.Mac);
                        if (terminalBaseInfo == null || terminalStatus == null)
                            continue;

                        TerminalModel terminalModel = new TerminalModel(groupItem, terminalBaseInfo, terminalStatus);
                        if (!reslut.Contains(terminalModel))
                            reslut.Add(terminalModel);
                    }
                }

                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                {
                    foreach (var itemModel in reslut)
                    {
                        var terminalItem = TerminalList.FirstOrDefault(t => t.Mac == itemModel.Mac);
                        if (terminalItem == null)
                            continue;
                        terminalItem.UpdateTerminalStatus(itemModel);
                    }
                    TerminalOfflineCount = _terminalList.Count(t => t.IsOnline == false);
                    TerminalOnlineCount = _terminalList.Count(t => t.IsOnline == true);
                    TerminalTotalCount = TerminalOfflineCount + TerminalOnlineCount;
                }));
                System.Threading.Thread.Sleep(100);
                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                {
                    ShowRefreshImage = Visibility.Collapsed;
                    ShowImage = Visibility.Visible;
                })); 
            };

           
            _statusWorker.RunWorkerCompleted += (o, ea) =>
            {
                //IsBusy = false;
                if (_statusWorker != null)
                {
                    _statusWorker.Dispose();
                    _statusWorker = null;
                }
            };
            
            //IsBusy = true;

            _statusWorker.RunWorkerAsync();

        }


        public ListCollectionView GetAllTerminalsView(ObservableCollection<TerminalViewModel> terminalList)
        {
            return new ListCollectionView(terminalList);
        }

        private bool IsShow(object para)
        {
            TerminalViewModel terminalInfo = para as TerminalViewModel;

            if (terminalInfo == null)
                return false;

            if (string.IsNullOrEmpty(SearchKeyword))
                return true;

            if (terminalInfo.Name != null && terminalInfo.Name.IndexOf(SearchKeyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }


        public ObservableCollection<TerminalViewModel> TerminalList
        {
            get
            {
                return _terminalList;
            }
            set
            {
                if (value == _terminalList)
                {
                    return;
                }
                _terminalList = value;
                RaisePropertyChanged(TerminalListPropertyName);
            }
        }

        public ObservableCollection<TerminalViewModel> SelectedTerminals
        {
            get
            {
                return _selectedTerminals;
            }
            set
            {
                if (_selectedTerminals != value)
                {
                    _selectedTerminals = value;
                    RaisePropertyChanged("SelectedTerminals");
                }
            }
        }

        public TerminalViewModel  SelectedTerminal
        {
            get
            {
                return _selectedTerminal;
            }
            set
            {
                if (_selectedTerminal != value)
                {
                    _selectedTerminal = value;
                    RaisePropertyChanged("SelectedTerminal");
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
                RaisePropertyChanged("IsBusy");
            }
        }


        public int TerminalOnlineCount
        {
            get
            {
                return _terminalOnlineCount;
            }
            set
            {
                if (value == _terminalOnlineCount)
                {
                    return;
                }
                _terminalOnlineCount = value;
                RaisePropertyChanged("TerminalOnlineCount");
            }
        }

        public int TerminalOfflineCount
        {
            get
            {
                return _terminalOfflineCount;
            }
            set
            {
                if (value == _terminalOfflineCount)
                {
                    return;
                }
                _terminalOfflineCount = value;
                RaisePropertyChanged("TerminalOfflineCount");
            }
        }

        public int TerminalTotalCount
        {
            get
            {
                return _terminalTotalCount;
            }
            set
            {
                if (value == _terminalTotalCount)
                {
                    return;
                }
                _terminalTotalCount = value;
                RaisePropertyChanged("TerminalTotalCount");
            }
        }

        public ICommand RefreshTerminalListCommand
        {
            get
            {
                if (_refreshTerminalListCommand == null)
                {
                    _refreshTerminalListCommand = new RelayCommand(() => RefreshTerminalList());
                }
                return _refreshTerminalListCommand;
            }
        }

        private RelayCommand<object> _refreshTerminalStatusCommand;
        public RelayCommand<object> RefreshTerminalStatusCommand
        {
            get
            {
                
                if (_refreshTerminalStatusCommand == null)
                {
                    
                    _refreshTerminalStatusCommand = new RelayCommand<object>(
                        (T) => this.RefreshTerminalStatus(T)
                        );
                }
                

                return _refreshTerminalStatusCommand;
            }
        }

        public ICommand SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    _selectionChangedCommand = new RelayCommand<object>(T => TerminalSelectionChanged(T), null);
                }
                return _selectionChangedCommand;
            }
        }

        public RelayCommand<object> TerminalDataLoadCommand
        {
            get
            {
                if (_terminalDataLoadCommand == null)
                {
                    _terminalDataLoadCommand = new RelayCommand<object>(T => TerminalDataLoad(T), null);
                }
                return _terminalDataLoadCommand;
            }
        }

        public RelayCommand CancelDataLoadCommand
        {
            get
            {
                if (_cancelDataLoadCommand == null)
                {
                    _cancelDataLoadCommand = new RelayCommand(() =>
                    {
                        if (_worker != null)
                        {
                            _worker.CancelAsync();
                            _worker.DoWork -= _worker_DoWork;
                            IsBusy = false;
                            _worker.Dispose();
                            _worker = null;
                        }
                        if (currentThread != null)
                        {
                            currentThread.Abort();
                            currentThread = null;
                        }
                    },
                    null);
                }
                return _cancelDataLoadCommand;
            }
        }

        public RelayCommand CancelDataPublishLoadCommand
        {
            get
            {
                if (_cancelDataPublishLoadCommand == null)
                {
                    _cancelDataPublishLoadCommand = new RelayCommand(() =>
                    {
                        MessageBox.Show("123");
                    },
                    null);
                }
                return _cancelDataPublishLoadCommand;
            }
        }

        //判断是点击复选框的还是点击行的标志位
        private bool _bclickCheckBox = false;

        private RelayCommand<object> _checkBoxPreviewMouseDownCommand;
        public RelayCommand<object> CheckBoxPreviewMouseDownCommand
        {
            
            get
            {
                if (_checkBoxPreviewMouseDownCommand == null)
                {
                    _checkBoxPreviewMouseDownCommand = new RelayCommand<object>(
                         (T) =>
                         {                           
                             _bclickCheckBox = true;

                         }, null);
                }
                return _checkBoxPreviewMouseDownCommand;
            }
        }


        private RelayCommand<object> _batchSelect;
        public RelayCommand<object> BatchSelect
        {

            get
            {
                if (_batchSelect == null)
                {
                    _batchSelect = new RelayCommand<object>(
                         (T) =>
                         {

                             System.Windows.Controls.DataGrid terminalDataGrid = App.Current.MainWindow.Resources["dataGrid"] as System.Windows.Controls.DataGrid;
                             if (terminalDataGrid == null)
                                 return;

                             TerminalViewModel viewModel = terminalDataGrid.SelectedItem as TerminalViewModel;
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
                             }

                             foreach (var item in repositoryViewModel.SelectedTerminals)
                             {
                                 if (!terminalDataGrid.SelectedItems.Contains(item))
                                 {
                                     terminalDataGrid.SelectedItems.Add(item);
                                 }
                             }
                             GroupCheckChange();
                            
                         }, null);
                }
                return _batchSelect;
            }
        }

        public void GroupCheckChange()
        {
            if(IsGroupSelected == true)
            {
                return;
            }

                if (TerminalListView.Groups != null)
                {
                    foreach (var item in TerminalListView.Groups)
                    {
                        var groupItem = item as CollectionViewGroup;
                        if (groupItem != null && groupItem.Items != null)
                        {
                            int count = 0;
                            bool? result = null;
                            for (int i = 0; i < groupItem.Items.Count; ++i)
                            {
                                var terminalItem = groupItem.Items[i] as TerminalViewModel;
                                if (terminalItem.CheckBoxChecked == true)
                                    count++;

                            }

                            if (groupItem.Items.Count == count)
                            {
                                result = true;
                            }
                            else if (count > 0 && groupItem.Items.Count > count)
                            {
                                result = null;
                            }
                            else
                                result = false;

                            (groupItem.Items[0] as TerminalViewModel).IsChecked = result;
                        }
                    }
                }
            
        }

        private RelayCommand<object> _checkBox_CheckedCommand;
        public RelayCommand<object> CheckBox_CheckedCommand
        {
            get
            {
                if (_checkBox_CheckedCommand == null)
                {
                    _checkBox_CheckedCommand = new RelayCommand<object>(
                         (T) =>
                         {
                             System.Windows.Controls.CheckBox checkBox = T as System.Windows.Controls.CheckBox;

                             System.Windows.Controls.DataGrid terminalDataGrid = App.Current.MainWindow.Resources["dataGrid"] as System.Windows.Controls.DataGrid;
                             if (terminalDataGrid == null)
                                 return;

                             if (checkBox == null)
                                 return;
                             TerminalViewModel viewModel = checkBox.DataContext as TerminalViewModel;
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
                             }
                             
                             foreach (var item in repositoryViewModel.SelectedTerminals)
                             {
                                 if (!terminalDataGrid.SelectedItems.Contains(item))
                                 {
                                     terminalDataGrid.SelectedItems.Add(item);
                                 }
                             }
                             GroupCheckChange();
                             
                         }, null);
                }
                return _checkBox_CheckedCommand;
            }
        }


        private RelayCommand<object> _checkBox_UncheckedCommand;
        public RelayCommand<object> CheckBox_UncheckedCommand
        {
            get
            {
                if (_checkBox_UncheckedCommand == null)
                {
                    _checkBox_UncheckedCommand = new RelayCommand<object>(
                         (T) =>
                         {
                             System.Windows.Controls.CheckBox checkBox = T as System.Windows.Controls.CheckBox;

                             System.Windows.Controls.DataGrid terminalDataGrid = App.Current.MainWindow.Resources["dataGrid"] as System.Windows.Controls.DataGrid;
                             if (terminalDataGrid == null)
                                 return;

                             if (checkBox == null)
                                 return;
                             TerminalViewModel viewModel = checkBox.DataContext as TerminalViewModel;
                             if (viewModel == null)
                                 return;

                             if (terminalDataGrid.SelectedItems == null)
                                 return;
                             TerminalRepositoryViewModel repositoryViewModel = terminalDataGrid.DataContext as TerminalRepositoryViewModel;
                             if (repositoryViewModel == null)
                                 return;
      
                             if (repositoryViewModel.SelectedTerminals.Contains(viewModel))
                             {
                                 repositoryViewModel.SelectedTerminals.Remove(viewModel);
                             }
                        
                             if (terminalDataGrid.SelectedItems.Contains(viewModel))
                             {
                                 terminalDataGrid.SelectedItems.Remove(viewModel);
                             }
                             GroupCheckChange();

                         }, null);
                }
                return _checkBox_UncheckedCommand;
            }
        }

        public void ControlListChange()
        {
            Nova.NovaWeb.McGo.Platform.ViewModel.ControlManageViewModel.terminalListType terminallistType = ViewModelLocator.Instance.ControlManage.JugeType();

            if (terminallistType == Nova.NovaWeb.McGo.Platform.ViewModel.ControlManageViewModel.terminalListType.allSyn)
            {

                ViewModelLocator.Instance.ControlManage.ShowPlutoPowerPlanSet = Visibility.Collapsed;
                ViewModelLocator.Instance.ControlManage.ShowTerminalLock = Visibility.Visible;

            }
            else
                if (terminallistType == Nova.NovaWeb.McGo.Platform.ViewModel.ControlManageViewModel.terminalListType.allAsyn)
                {
                    ViewModelLocator.Instance.ControlManage.ShowTerminalLock = Visibility.Collapsed;
                    ViewModelLocator.Instance.ControlManage.ShowPlutoPowerPlanSet = Visibility.Visible;
                }
                else
                {
                    ViewModelLocator.Instance.ControlManage.ShowPlutoPowerPlanSet = Visibility.Visible;
                    ViewModelLocator.Instance.ControlManage.ShowTerminalLock = Visibility.Visible;
                }
        }

        private void TerminalSelectionChanged(object sender)
        {
            Debug.WriteLine("selection changed");
            System.Windows.Controls.SelectionChangedEventArgs e = sender as System.Windows.Controls.SelectionChangedEventArgs;
            if (e == null)
            {
                return;
            }
            System.Windows.Controls.DataGrid dataGrid = e.Source as System.Windows.Controls.DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            if (ViewModel.ViewModelLocator.Instance.PublishManage.IsBatch == false)
            {
                if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count >= 1)
                {
                    dataGrid.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.Collapsed;
                }
                else
                {
                    dataGrid.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                }
            }
            else
            {
            if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 1)
            {
                dataGrid.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.Collapsed;
            }
            else
            {
                dataGrid.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }
            }

            if (ViewModel.ViewModelLocator.Instance.PublishManage.IsBatch == false)
            {
                return;
            }
           

            if(IsGroupSelected == true)
            {
                return;
            }

            if(_bclickCheckBox == true)
            {
             
                _bclickCheckBox = false;
                return;
            }
            if (dataGrid.SelectedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    TerminalViewModel terminalViewModel = item as TerminalViewModel;
                    if (terminalViewModel == null)
                        continue;
                    if (!SelectedTerminals.Contains(terminalViewModel))
                    {
                        terminalViewModel.IsSelected = true;
                        SelectedTerminals.Add(terminalViewModel);
                        terminalViewModel.CheckBoxChecked = true;
                    }
                }
                foreach (var item in e.RemovedItems)
                {
                    TerminalViewModel terminalViewModel = item as TerminalViewModel;
                    if (terminalViewModel == null)
                        continue;
                    terminalViewModel.IsSelected = false;
                    terminalViewModel.CheckBoxChecked = false;
                    SelectedTerminals.Remove(terminalViewModel);
                }
            }
           

           GroupCheckChange();
           ControlListChange();

           e.Handled = true;
        }

        private void TerminalDataLoad(object sender)
        {
            //RoutedEventArgs e = sender as RoutedEventArgs;
            //if (e == null)
            //    return;
            //System.Windows.Controls.DataGrid dataGrid = e.Source as System.Windows.Controls.DataGrid;

            //if (dataGrid == null)
            //    return;
            //Messenger.Default.Register<NotificationMessage<ReadOnlyObservableCollection<object>>>(
            //    this,
            //T =>
            //{
            //    if (T.Notification == "Items")
            //    {
            //        foreach (var item in T.Content)
            //        {
            //            var terminalViewModel = item as TerminalViewModel;
            //            if (terminalViewModel == null)
            //                continue;
            //            if (terminalViewModel.CheckBoxChecked)
            //            {
            //                if (!this.SelectedTerminals.Contains(terminalViewModel))
            //                    this.SelectedTerminals.Add(terminalViewModel);
            //                if (!dataGrid.SelectedItems.Contains(terminalViewModel))
            //                    dataGrid.SelectedItems.Add(terminalViewModel);
            //            }
            //            else
            //            {
            //                if (this.SelectedTerminals.Contains(terminalViewModel))
            //                    this.SelectedTerminals.Remove(terminalViewModel);
            //                if (dataGrid.SelectedItems.Contains(terminalViewModel))
            //                    dataGrid.SelectedItems.Remove(terminalViewModel);
            //            }
            //        }
            //    }
            //});
            //// dataGrid.SelectedItems.Clear();

            //if (SelectedTerminals == null || SelectedTerminals.Count == 0)
            //    return;
            //IEnumerable<TerminalViewModel> terminals = SelectedTerminals.ToList();
            //dataGrid.SelectedItems.Clear();
            //foreach (var item in terminals)
            //{
            //    dataGrid.SelectedItems.Add(item);
            //}
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="keyword"></param>
        public void Search(string keyword)
        {
            SearchKeyword = keyword;

            if (TerminalListView != null)
            {
                if (ViewModel.ViewModelLocator.Instance.PublishManage.IsBatch == false)
                {
                    System.Windows.Controls.DataGrid terminalDataGrid = App.Current.MainWindow.Resources["dataGrid"] as System.Windows.Controls.DataGrid;
                    if (terminalDataGrid == null)
                        return;
                    TerminalRepositoryViewModel repositoryViewModel = terminalDataGrid.DataContext as TerminalRepositoryViewModel;
                    if (repositoryViewModel == null)
                        return;
                    repositoryViewModel.SelectedTerminals.Clear();

                    foreach (var terminalViewModel in TerminalList)
                    {
                        terminalViewModel.CheckBoxChecked = false;
                    }

                    TerminalListView.Refresh();

                        TerminalListView.MoveCurrentToFirst();
                        SelectedTerminal = TerminalListView.CurrentItem as TerminalViewModel;
                        if (SelectedTerminal != null)
                        {
                            SelectedTerminals.Add(SelectedTerminal);
                            SelectedTerminal.CheckBoxChecked = true;
                            GroupCheckChange();
                        }
                  
                }
                else
                    TerminalListView.Refresh();
                ControlListChange();
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            if (_autoRefreshTimer != null)
                _autoRefreshTimer.Dispose();
            Messenger.Default.Unregister(this);
            if (_terminalList != null)
                _terminalList.Clear();
            if (_worker != null)
            {
                _worker.Dispose();
            }
            if (_statusWorker != null)
                _statusWorker.Dispose();
        }


        #region 分组功能

        private string _groupingName;

        private RelayCommand<object> _groupColumnCommand;
        private RelayCommand<object> _ungroupColumnCommand;
        private RelayCommand<object> _expandAllGroupsCommand;
        private RelayCommand<object> _collapseAllGroupsCommand;
        private RelayCommand<object> _selecteGroupItemsCommand;
        private bool? _isAllSelected;

        public string GroupingName
        {
            get
            {
                return _groupingName;
            }
            set
            {
                _groupingName = value;
                RaisePropertyChanged("GroupingName");
            }
        }

        public void RecoverySelectedItems()
        {
            System.Windows.Controls.DataGrid terminalDataGrid = App.Current.MainWindow.Resources["dataGrid"] as System.Windows.Controls.DataGrid;
            if (terminalDataGrid == null)
                return;

            TerminalRepositoryViewModel repositoryViewModel = terminalDataGrid.DataContext as TerminalRepositoryViewModel;
            if (repositoryViewModel == null)
                return;

            foreach (var item in repositoryViewModel.SelectedTerminals)
            {
                if (!terminalDataGrid.SelectedItems.Contains(item))
                {
                    terminalDataGrid.SelectedItems.Add(item);
                }
            }
            GroupCheckChange();
        }
        public RelayCommand<object> GroupColumnCommand
        {
            get
            {
                if (_groupColumnCommand == null)
                {
                    _groupColumnCommand = new RelayCommand<object>(
                        (T) =>
                    {
                        if (GroupingName == "IsSelected")
                            return;

                        UnGroupColumnCommand.Execute(null);

                        TerminalListView.GroupDescriptions.Add(new PropertyGroupDescription(GroupingName));

                        App.GetPlatformConfig().AppDataInfo.TerminalSortField = GroupingName;
                            RecoverySelectedItems();
                            //App.GetPlatformConfig().Save();
                    },
                        null);
                }
                return _groupColumnCommand;
            }
        }

        public RelayCommand<object> UnGroupColumnCommand
        {
            get
            {
                if (_ungroupColumnCommand == null)
                {
                    _ungroupColumnCommand = new RelayCommand<object>(
                        (T) =>
                    {
                        //if (GroupingName == "IsSelected")
                        //    return;
                        ExpandAllGroupsCommand.Execute(null);
                        TerminalListView.GroupDescriptions.Clear();
                            RecoverySelectedItems();
                    },
                        null);
                }
                return _ungroupColumnCommand;
            }
        }

        public RelayCommand<object> ExpandAllGroupsCommand
        {
            get
            {
                if (_expandAllGroupsCommand == null)
                {
                    _expandAllGroupsCommand = new RelayCommand<object>(
                         (T) =>
                    {
                        if (TerminalListView.Groups != null)
                        {
                            foreach (var groupItem in TerminalListView.Groups)
                            {
                                var group = groupItem as CollectionViewGroup;
                                     (group.Items[0] as TerminalViewModel).IsExpanded = true;
                            }
                        }
                    },
                        null);
                }
                return _expandAllGroupsCommand;
            }
        }

        public RelayCommand<object> CollapseAllGroupsCommand
        {
            get
            {
                if (_collapseAllGroupsCommand == null)
                {
                    _collapseAllGroupsCommand = new RelayCommand<object>(
                         (T) =>
                    {
                        if (TerminalListView.Groups != null)
                        {
                            foreach (var groupItem in TerminalListView.Groups)
                            {
                                var group = groupItem as CollectionViewGroup;
                                     (group.Items[0] as TerminalViewModel).IsExpanded = false;
                            }
                        }
                    },
                        null);
                }
                return _collapseAllGroupsCommand;
            }
        }

        #endregion

        private void ColumnHeader_RightButtonDown(object sender, EventArgs e)
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


        public bool? IsGroupSelected
        {
            get
            {
                return _isGroupSelected;
            }
            set
            {
                _isGroupSelected = value;
                RaisePropertyChanged("IsGroupSelected");
            }
        }

        public bool? IsAllSelected
        {
            get
            {
                return _isAllSelected;
            }
            set
            {
                if (value.Equals(_isAllSelected))
                    return;
                _isAllSelected = value;
                RaisePropertyChanged("IsAllSelected");
            }
        }

        public RelayCommand<object> SelecteGroupItemsCommand
        {
            get
            {
                if (_selecteGroupItemsCommand == null)
                {
                    _selecteGroupItemsCommand = new RelayCommand<object>(
                        (o) =>
                    {
                        SelecteGroupItems(o);
                    },
                        null);
                }
                return _selecteGroupItemsCommand;
            }
        }


        private RelayCommand<object> _checkedCommand;

        public RelayCommand<object> CheckedCommand
        {
            get
            {
                if (_checkedCommand == null)
                {
                    _checkedCommand = new RelayCommand<object>(
                    (o) =>
                    {
                        CheckedObject(o);
                    } , null);
                }
                return _checkedCommand;
            }
        }

        public RelayCommand<object> ExpandedCommand
        {
            get
            {
                if (_expandedCommand == null)
                {
                    _expandedCommand = new RelayCommand<object>((o) =>
                    {
                        CollectionViewGroup group = o as CollectionViewGroup;
                        if (group == null)
                            return;
                        foreach (var item in group.Items)
                        {
                            (item as TerminalViewModel).IsExpanded = true;
                        }
                    }, null
                    );
                }
                return _expandedCommand;
            }
        }

        public RelayCommand<object> UnExpandedCommand
        {
            get
            {
                if (_unExpandedCommand == null)
                {
                    _unExpandedCommand = new RelayCommand<object>((o) =>
                    {
                        CollectionViewGroup group = o as CollectionViewGroup;
                        if (group == null)
                            return;
                        foreach (var item in group.Items)
                        {
                            (item as TerminalViewModel).IsExpanded = false;
                        }
                    }, null
                    );
                }
                return _unExpandedCommand;
            }
        }

        private void CheckedObject(object o)
        {
            //throw new NotImplementedException();
        }

        public void SelecteGroupItems(object o)
        {
            IsGroupSelected = true;
            var collectionViewGroup = o as CollectionViewGroup;
            if (collectionViewGroup == null || collectionViewGroup.Items == null)
                return;

            for (int index = 0; index < collectionViewGroup.Items.Count; index++)
            {
                var fixItem = collectionViewGroup.Items[0] as TerminalViewModel;
                var terminalViewModel = collectionViewGroup.Items[index] as TerminalViewModel;
                if (fixItem == null || terminalViewModel == null)
                    return;
               if(fixItem.IsChecked == false)
               {
                   terminalViewModel.CheckBoxChecked = false;
               }
               else
               {
                   terminalViewModel.CheckBoxChecked = true;
               }
            }
            IsGroupSelected = false;
            GroupCheckChange();
            ControlListChange();
        }
    }
}
