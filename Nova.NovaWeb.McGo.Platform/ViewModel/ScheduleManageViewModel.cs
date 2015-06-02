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
using Nova.NovaWeb.McGo.Utilities;
using Nova.NovaWeb.Player;
using Nova.NovaWeb.Protocol;
using Nova.NovaWeb.UI;
using Nova.NovaWeb.Windows;
using Nova.NovaWeb.Windows.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class ScheduleManageViewModel : ViewModelBase, ISearchable
    {
        private string _currentGroupName;
        private const string ScheduleListPropertyName = "ScheduleList";

        private bool _isBusy;
        private IEnumerable<Schedule> _schedules;
        private IScheduleRepositoryProvider _scheduleRepositoryProvider;
        private IModalDialogService _modalDialogService;
        private IPlatformService _platformService;


        private ObservableCollection<ScheduleViewModel> _scheduleList = new ObservableCollection<ScheduleViewModel>();
        private ObservableCollection<ScheduleViewModel> _selectedScheduleList = new ObservableCollection<ScheduleViewModel>();
        private ScheduleViewModel _selectedSchedule;
        private ScheduleViewModel _currentPreviewSchedule;
        private ViewModelLocator _viewModelLocator;

        private string _seachKeyword;
        private string _tempText = "ManangementCenter_UI_Preview";
        private ListCollectionView _allPlayProgramsView;

        private RelayCommand<object> _selectionChangedCommand;
        private RelayCommand _newPlayProgramCommand;
        private RelayCommand _openPlayProgramCommand;
        private RelayCommand _deletePlayProgramCommand;
        private RelayCommand _publishScheduleCommand;
        private RelayCommand _exportByUDiskCommand;
        private RelayCommand _inquiryPublishHistoryCommand;
        private RelayCommand _refreshPlayListCommand;

        private RelayCommand _findPlayProgramCommand;

        private RelayCommand _previewScheduleCommand;
        public FrmPlayProgramEditor fEditor = null;

        public ScheduleManageViewModel(IScheduleRepositoryProvider scheduleRepositoryProvider, IModalDialogService modalDialogService)
        {
            _scheduleRepositoryProvider = scheduleRepositoryProvider;
            _modalDialogService = modalDialogService;
            _platformService = AppEnvionment.Default.Get<IPlatformService>();
            _viewModelLocator = ViewModelLocator.Instance;
            //PreviewHelper.GetInstance();
            Messenger.Default.Register<NotificationMessage<string>>(
             this,
             "RefreshSchedule",
            message =>
            {
                if (message.Notification == "RefreshSchedule")
                {
                    RefreshScheduleList();
                }
            });

            Messenger.Default.Register<NotificationMessage>(this, "AutoLogoutAccout", message =>
            {
                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                {
                    if (fEditor != null)
                    {
                        fEditor.dispose();
                    }
                }));
            });

            RefreshScheduleList();
            //InitialPlayHelper();
            ProcessingMessages();
        }

        public override void Cleanup()
        {
            // Clean up if needed
            base.Cleanup();
            Messenger.Default.Unregister(this);
        }

        private void ProcessingMessages()
        {
            Messenger.Default.Register<NotificationMessage<string>>(this, ViewModelLocator.CLEANUP_NOTIFICATION,
            message =>
            {
                this.Cleanup();
            });

            Messenger.Default.Register<NotificationMessage<string>>(this, "StopPreview", message =>
            {
                _currentPreviewSchedule = null;
                TempText = "ManangementCenter_UI_Preview";
            });

            Messenger.Default.Register<NotificationMessage<string>>(this, "StartPreview", message =>
            {
                _currentPreviewSchedule = _selectedSchedule;
                TempText = "ManangementCenter_UI_StopPreview";
            });
        }

        public ListCollectionView AllPlayProgramsView
        {
            get
            {
                return _allPlayProgramsView;
            }
            private set
            {
                _allPlayProgramsView = value;
                RaisePropertyChanged("AllPlayProgramsView");
            }
        }

        public ObservableCollection<ScheduleViewModel> ScheduleList
        {
            get
            {
                return _scheduleList;
            }
            set
            {
                if (value == _scheduleList)
                {
                    return;
                }
                _scheduleList = value;
                RaisePropertyChanged(ScheduleListPropertyName);
            }
        }

        public ObservableCollection<ScheduleViewModel> SelectedScheduleList
        {
            get
            {
                return _selectedScheduleList;
            }
            set
            {
                if (_selectedScheduleList != value)
                {
                    _selectedScheduleList = value;
                    RaisePropertyChanged("SelectedScheduleList");
                }
            }
        }

        public ScheduleViewModel SelectedSchedule
        {
            get
            {
                return _selectedSchedule;
            }
            set
            {
                if (_selectedSchedule == value)
                    return ;
                _selectedSchedule = value;
                RaisePropertyChanged("SelectedSchedule");
            }
        }

        public string TempText
        {
            get
            {
                return _tempText;
            }
            set
            {
                _tempText = value;
                RaisePropertyChanged("TempText");
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

        public RelayCommand<object> SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    _selectionChangedCommand = new RelayCommand<object>((T) => ScheduleSelectionChanged(T), null);
                }
                return _selectionChangedCommand;
            }
        }


        public RelayCommand NewPlayProgramCommand
        {
            get
            {
                if (_newPlayProgramCommand == null)
                {
                    _newPlayProgramCommand = new RelayCommand(
                        () => NewPlayProgram(),
                        null);
                }
                return _newPlayProgramCommand;
            }
        }

        public RelayCommand OpenPlayProgramCommand
        {
            get
            {
                if (_openPlayProgramCommand == null)
                {
                    _openPlayProgramCommand = new RelayCommand(
                        () => OpenPlayProgram(),
                        () => CanOpenExcute());
                }
                return _openPlayProgramCommand;
            }
        }

        public RelayCommand PublishScheduleCommand
        {
            get
            {
                if (_publishScheduleCommand == null)
                {
                    _publishScheduleCommand = new RelayCommand(
                        () => PublishSchedule(),
                        () =>
                    {
                        return HasSelectedPlayProgram() && !IsMultipleSelectedPlayProgram() ? true : false;
                    });
                }
                return _publishScheduleCommand;
            }
        }

        public RelayCommand ExportByUDiskCommand
        {
            get
            {
                if (_exportByUDiskCommand == null)
                {
                    _exportByUDiskCommand = new RelayCommand(
                        () => ExportByUDisk(),
                        () =>
                    {
                        return HasSelectedPlayProgram() && !IsMultipleSelectedPlayProgram() ? true : false;
                    });
                }
                return _exportByUDiskCommand;
            }
        }


        public RelayCommand InquiryPublishHistoryCommand
        {
            get
            {
                if (_inquiryPublishHistoryCommand == null)
                {
                    _inquiryPublishHistoryCommand = new RelayCommand(() => InquiryPublishHistory(), null);
                }
                return _inquiryPublishHistoryCommand;
            }
        }

        public RelayCommand DeletePlayProgramCommand
        {
            get
            {
                if (_deletePlayProgramCommand == null)
                {
                    _deletePlayProgramCommand = new RelayCommand(
                        () => DeletePlayProgram(),
                        () => CanDeleteExcute());
                }
                return _deletePlayProgramCommand;
            }
        }

        public RelayCommand RefreshPlayListCommand
        {
            get
            {
                if (_refreshPlayListCommand == null)
                {
                    _refreshPlayListCommand = new RelayCommand(() => RefreshScheduleList(), null);
                }
                return _refreshPlayListCommand;
            }
        }

        public RelayCommand PreviewScheduleCommand
        {
            get
            {
                if (_previewScheduleCommand == null)
                {
                    _previewScheduleCommand = new RelayCommand(
                    () => PreviewPlayProgram(),
                        () =>
                    {
                        return HasSelectedPlayProgram() && !IsMultipleSelectedPlayProgram() ? true : false;
                    });
                }
                return _previewScheduleCommand;
            }
        }

        //public ICommand InquiryPublishHistoryCommand
        //{
        //    get
        //    {
        //        if (_inquiryPublishHistoryCommand == null)
        //        {
        //            _inquiryPublishHistoryCommand = new RelayCommand(
        //                () => InquiryPublishHistroy(),
        //                () =>
        //                {
        //                    if (_viewModelLocator.TerminalCollection.SelectedTerminals.Count > 0)
        //                    {
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        return false;
        //                    }
        //                });

        //        }
        //        return _inquiryPublishHistoryCommand;
        //    }
        //}

        //private void InitialPlayHelper()
        //{
        //    _playHelper = new PlayHelper(1, string.Empty);
        //    _playHelper.Key_EscKey_EnterEvent += playHelper_Key_EscKey_EnterEvent;
        //    _playHelper.StopPreview_ContrexMenuStrip_Event += playHelper_StopPreview_ContrexMenuStrip_Event;
        //    //_playHelper.UpdateLanguage(ClientManager.LanguageResourcePath, ClientManager.CultureName);
        //    _playHelper.IsShortcutsHidden = false;
        //    _playHelper.SetScreenHide(0, false);
        //    _playHelper.SetScreenTopMost(0, false);
        //    _playHelper.IsShowContextMenuStrip = true;
        //    _playHelper.IsShowMouseInScreen = true;
        //    _playHelper.UpdateLanguage(PlatformConfig.LanguageResourcePath, _platformService.GetPlatformLanguage());
        //}

        //private void playHelper_StopPreview_ContrexMenuStrip_Event(object sender, EventArgs e)
        //{
        //    StopPreview();
        //}

        //private void StopPreview()
        //{
        //    if (_playHelper == null)
        //        return;
        //    //_playHelper.Key_EscKey_EnterEvent -= playHelper_Key_EscKey_EnterEvent;
        //    //_playHelper.StopPreview_ContrexMenuStrip_Event -= _playHelper_StopPreview_ContrexMenuStrip_Event;
        //    _playHelper.SetScreenHide(0, false);
        //    _playHelper.StopPlay(0);
        //    _currentPreviewSchedule = null;
        //    TempText = "ManangementCenter_UI_Preview";
        //}

        //private void playHelper_Key_EscKey_EnterEvent(object sender, EventArgs e)
        //{
        //    StopPreview();
        //}
        private void RefreshScheduleList()
        {
            BackgroundWorker _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _worker.DoWork += (o, ea) =>
            {
                //CodeTimer.Time("刷新播放列表", 1, new Action(() =>
                //{
                //        _scheduleRepositoryProvider.FindAll();
                //}));
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    _scheduleList.Clear();
                }));

                _schedules = _scheduleRepositoryProvider.FindAll();


                //CodeTimer.Time("刷新UI", 1, new Action(() =>
                //{
                //DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                //{
                //    foreach (var itemModel in _schedules)
                //    {
                //        _scheduleList.Add(new ScheduleViewModel(new ScheduleModel(itemModel)));
                //    }

                //    AllPlayProgramsView = CollectionViewSource.GetDefaultView(_scheduleList) as ListCollectionView;
                //    AllPlayProgramsView.Filter = IsShow;
                //    AllPlayProgramsView.SortDescriptions.Clear();
                //    AllPlayProgramsView.SortDescriptions.Add(new SortDescription("LastEditTime", ListSortDirection.Descending));
                //    AllPlayProgramsView.MoveCurrentToFirst();
                //    SelectedSchedule = AllPlayProgramsView.CurrentItem as ScheduleViewModel;
                //    SelectedScheduleList.Clear();
                //    SelectedScheduleList.Add(SelectedSchedule);
                //}));
                //}));


                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    //CodeTimer.Time("001", 1, new Action(() =>
                    //{
                    foreach (var itemModel in _schedules)
                    {
                        _scheduleList.Add(new ScheduleViewModel(new ScheduleModel(itemModel)));
                    }
                    //}));
                    
                    AllPlayProgramsView = CollectionViewSource.GetDefaultView(_scheduleList) as ListCollectionView;
                    AllPlayProgramsView.Filter = IsShow;
                    AllPlayProgramsView.SortDescriptions.Clear();
                    AllPlayProgramsView.SortDescriptions.Add(new SortDescription("LastEditTime", ListSortDirection.Descending));
                    AllPlayProgramsView.MoveCurrentToFirst();
                    SelectedSchedule = AllPlayProgramsView.CurrentItem as ScheduleViewModel;
                    //刷新时清除所有方案
                    SelectedSchedule = null;

                    SelectedScheduleList.Clear();
                    //SelectedScheduleList.Add(SelectedSchedule);
                }));

            };

            _worker.RunWorkerCompleted += (o, ea) =>
            {
                //IsBusy = false;
                _worker.Dispose();
                _worker = null;

            };

            //IsBusy = true;
            _worker.RunWorkerAsync();
        }

        private bool IsShow(object para)
        {
            ScheduleViewModel playInfo = para as ScheduleViewModel;

            if (playInfo == null)
                return false;

            if (string.IsNullOrEmpty(SearchKeyword))
                return true;

            if (playInfo.DisplayScheduleName != null && playInfo.DisplayScheduleName.IndexOf(SearchKeyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get
            {
                return _searchKeyword;
            }
            set
            {
                if (_searchKeyword == value)
                {
                    return;
                }
                _searchKeyword = value;
                RaisePropertyChanged("SearchKeyword");
            }
        }

        public void Search(string keyword)
        {
            SearchKeyword = keyword;

            if (AllPlayProgramsView != null)
            {
                AllPlayProgramsView.Refresh();
            }
        }

        private void PreviewPlayProgram()
        {
            PreviewHelper.GetInstance().Initialize(_selectedSchedule);
           
            if (TempText == "ManangementCenter_UI_Preview")
            {
                PreviewHelper.GetInstance().StartPreview(_selectedSchedule);
            }
            else
                if (TempText == "ManangementCenter_UI_StopPreview")
                {
                    PreviewHelper.GetInstance().StopPreview();
                }
        }
        /// <summary>
        /// 新建播放方案
        /// </summary>
        private void NewPlayProgram()
        {
            bool isAsyncSchedule = true;

            ScheduleConfirmMessageBox scheduleMessageView = new ScheduleConfirmMessageBox();
            scheduleMessageView.Owner = App.Current.MainWindow;
            scheduleMessageView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            scheduleMessageView.ShowInTaskbar = false;
            bool? result = scheduleMessageView.ShowDialog();

            if (scheduleMessageView.IsAsyn == null)
                return;

            if (scheduleMessageView.IsAsyn == true)
            {
                isAsyncSchedule = false;
            }
            else
            {
                isAsyncSchedule = true;
            }

            var terminals = from item in _viewModelLocator.TerminalCollection.TerminalList
                                                                                                                                                                                                                                                                                            select item.CurrentTerminal.CurrentTerminal;
            try
            {
                //Modify-lixc
               fEditor = new FrmPlayProgramEditor(
                PlatformConfig.LocalScheduleDirectoryPath,
                null,
                PlatformConfig.LanguageResourcePath,
                _platformService.GetPlatformLanguage(),
                new System.Drawing.Size(350,350),             // _platformService.GetAppData().ScheduleScreenSize
                true,
                terminals.ToList(),
                isAsyncSchedule);
                fEditor.PlayProgramSaveFinishedEvent += (s, i) =>
                {
                    _platformService.GetAppData().ScheduleScreenSize = fEditor.PlayProgramSize;
                    _platformService.Save();
                    RefreshScheduleList();
                };
                fEditor.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                fEditor.ShowInTaskbar = false;
                fEditor.ShowDialog(new Wfp32Window(App.Current.MainWindow));

               
               
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message);
                }));
            }
            //App.GetPlatformConfig().AppDataInfo.ScheduleScreenSize = fEditor.PlayProgramSize;
        }

        /// <summary>
        /// 打开播放方案
        /// </summary>
        public void OpenPlayProgram()
        {
            //Modify-lixc
            var terminals = from item in _viewModelLocator.TerminalCollection.TerminalList
                                                                                                                                                                                                                                                                            select item.CurrentTerminal.CurrentTerminal;

            try
            {
               fEditor = new FrmPlayProgramEditor(
                PlatformConfig.LocalScheduleDirectoryPath,
                SelectedSchedule.FilePath,
                PlatformConfig.LanguageResourcePath,
                _platformService.GetPlatformLanguage(),
                new System.Drawing.Size(SelectedSchedule.ScreenWidth, SelectedSchedule.ScreenHeight),
                true,
                terminals.ToList(),
                SelectedSchedule.Type == ScheduleType.PC ? false : true
                );
                fEditor.PlayProgramSaveFinishedEvent += (s, i) =>
                {
                    RefreshScheduleList();
                };
                fEditor.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                fEditor.ShowInTaskbar = false;
                fEditor.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message);
                }));
            }
        }


        /// <summary>
        /// 发布播放方案
        /// </summary>
        private void PublishSchedule()
        {

            //if (SelectedSchedule == null)
            //    return;

            //if (!SelectedSchedule.ValidationMediaList())
            //{
            //    if (Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,
            //        LocalizationHelper.GetLocalizationBussnissInfo("ManangementCenter_Business_MediaListEmpty", "播放方案中没有添加任何媒体，发布至终端将导致终端黑屏播放，是否继续发布？"),
            //        string.Empty,
            //        MessageBoxButton.YesNo) == MessageBoxResult.No)
            //    {
            //        return;
            //    }
            //}
            //List<string> _playPathList = new List<string>();
            //_playPathList.Add(SelectedPlayProgram.DownloadFilePath);
            //FrmBinding frmBinding = new FrmBinding(
            //    _playPathList,
            //    ClientManager.PlayPath,
            //     ClientManager.LanguageResourcePath,//Path.Combine(ClientManager.LanguageResourcePath, GetLanguageResourcePath("CommandControl")),
            //    ClientManager.CultureName,
            //    GetTerminalStatus());
            //frmBinding.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            //frmBinding.ShowInTaskbar = false;
            //frmBinding.ShowDialog();
        }

        private void InquiryPublishHistory()
        {
            //SelectTerminalView selectTerminalView = new SelectTerminalView();
            //selectTerminalView.DataContext = new SelectTerminalVM(_allTerminalViewModel);
            //selectTerminalView.Owner = Application.Current.MainWindow;
            //selectTerminalView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //selectTerminalView.ShowDialog();

            var dialog = AppEnvionment.Default.Get<IModalWindow>("SelectTerminalView");

            var dialogViewModel = new LoginConfigViewModel(AppEnvionment.Default.Get<IPlatformService>());

            Messenger.Default.Register<NotificationMessage<string>>(
               this,
               dialogViewModel.MessageID,
            message =>
            {
                if (message.Notification.ToUpper() == "OK")
                {
                    dialog.Close();
                }
            });

            _modalDialogService.ShowDialog(dialog, dialogViewModel);
        }

        private void ExportByUDisk()
        {
            //Modify-lixc
            List<TerminalModel> terminals = new List<TerminalModel>();
            List<TerminalGroup> terminalGroups = new List<TerminalGroup>();
            var groupService = AppEnvionment.Default.Get<IGroupRepositoryProvider>();
            if (groupService == null)
                return;
            EquipCodeTypes currentType = SelectedSchedule.Type == ScheduleType.PC ? EquipCodeTypes.Third : EquipCodeTypes.Pluto;
            List<TerminalGroup> grouList ;
            if (SelectedSchedule.Type == ScheduleType.PC)
            {
                var terminalList = _viewModelLocator.TerminalCollection.TerminalList.Where(t => t.Type == TerminalType.PC).Select(t => t.CurrentTerminal) ;
                terminals.AddRange(terminalList);
                grouList = groupService.FindAllGroup().ToList().Where(g => g.SiteList != null && g.SiteList.Count > 0 && (g.SiteList.Count(s => s.EquipCode == currentType) > 0)).Select(g => new TerminalGroup(g)).ToList();
            }
            else
            {
                var terminalList = _viewModelLocator.TerminalCollection.TerminalList.Select(t => t.CurrentTerminal);
                terminals.AddRange(terminalList);
                grouList = groupService.FindAllGroup().ToList().Where(g => g.SiteList != null && g.SiteList.Count > 0 ).Select(g => new TerminalGroup(g)).ToList();
            }

            //groupService.FindAllGroup().ToList().ForEach(g => terminalGroups.Add(new TerminalGroup(g)));
            try
            {
                FrmTerminalListSelect frmTerminalListSelect = new FrmTerminalListSelect(
                SelectedSchedule.FilePath,
                PlatformConfig.LocalScheduleDirectoryPath,
                GetTerminalStatus(),
                PlatformConfig.LanguageResourcePath,
                _platformService.GetPlatformLanguage(),
                terminals,
                grouList);
                frmTerminalListSelect.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

                frmTerminalListSelect.ShowInTaskbar = false;
                frmTerminalListSelect.ShowDialog(new Wfp32Window(App.Current.MainWindow));
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message + "\r\n" + e.TargetSite.ToString());
                }));
            }
        }

        /// <summary>
        /// 删除播放方案
        /// </summary>
        private void DeletePlayProgram()
        {
            if (SelectedScheduleList == null)
            {
                return;
            }

            if (TempText == "ManangementCenter_UI_StopPreview" && SelectedScheduleList.Any(T => T.Name == _currentPreviewSchedule.Name))//TempText == "ManangementCenter_UI_StopPreview" &&
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_Error6", out errorInfo);
                string errorInfo2;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_Information", out errorInfo2);
                Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, errorInfo + "！", errorInfo2, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string errorInfo1;
            MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_Error7", out errorInfo1);
            string errorInfo3;
            MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_Information", out errorInfo3);
            if (Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,
                string.Format(errorInfo1 + "{0}？", "\r\n" + GetPlayProgramNameList(SelectedScheduleList)),
                errorInfo3,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                List<ScheduleViewModel> playList = SelectedScheduleList.ToList();
                for (int i = 0; i < playList.Count; i++)
                {
                    if (File.Exists(playList[i].FilePath))
                        File.Delete(playList[i].FilePath);
                    if (File.Exists(playList[i].PublishPathInfo.CloudFilePath))
                        File.Delete(playList[i].PublishPathInfo.CloudFilePath);
                    if (File.Exists(playList[i].PublishPathInfo.PlutoFilePath))
                        File.Delete(playList[i].PublishPathInfo.PlutoFilePath);
                    if (File.Exists(playList[i].PublishPathInfo.PlutoConverterFilePath))
                        File.Delete(playList[i].PublishPathInfo.PlutoConverterFilePath);
                    if (File.Exists(playList[i].PublishPathInfo.PlutoMdcorFilePath))
                        File.Delete(playList[i].PublishPathInfo.PlutoMdcorFilePath);

                    ScheduleList.Remove(playList[i]);
                    DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(playList[i].FilePath));
                    FileInfo[] fileInfos = directoryInfo.GetFiles(Path.GetFileName(playList[i].FilePath) + ".*");
                    foreach (var item in fileInfos)
                    {
                        if (File.Exists(item.FullName))
                            File.Delete(item.FullName);
                    }
                }
            }
        }

        private void InquiryPublishHistroy()
        {
            //Modify-lixc
            //List<Site> terminals = new List<Site>();
            //_viewModelLocator.TerminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));
            //InquiryWindowViewModel inquiryViewModel = new InquiryWindowViewModel(
            //    terminals,
            //    new List<CmdTypes>() { CmdTypes.downLoadPlayProgam },
            //    new List<CmdTypes>() { CmdTypes.downLoadPlayProgam },
            //       PlatformConfig.LanguageResourcePath,
            //    _platformService.GetPlatformLanguage(),
            //    true
            //    );//Path.Combine(ClientManager.LanguageResourcePath, @"zh-CN\TerminalControl.zh-CN.resources")
            //InquireCommandWindow inquireCommandWindow = new InquireCommandWindow();
            //inquireCommandWindow.DataContext = inquiryViewModel;
            //inquireCommandWindow.Owner = App.Current.MainWindow;
            //inquireCommandWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //inquireCommandWindow.ShowInTaskbar = false;
            //inquireCommandWindow.ShowDialog();
        }

        /// <summary>
        /// 已选播放方案列表改变
        /// </summary>
        /// <param name="obj"></param>
        private void ScheduleSelectionChanged(object obj)
        {
            SelectionChangedEventArgs e = obj as SelectionChangedEventArgs;
            if (e == null)
            {
                return;
            }
            DataGrid dataGrid = e.Source as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 1)
            {
                dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            }
            else
            {
                dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }

            if (dataGrid.SelectedItems != null)
            {
                SelectedScheduleList.Clear();
                foreach (ScheduleViewModel item in dataGrid.SelectedItems)
                {
                    if (!SelectedScheduleList.Contains(item))
                    {
                        SelectedScheduleList.Add(item);
                    }
                }
            }
            e.Handled = true;
        }

        private bool CanOpenExcute()
        {
            return HasSelectedPlayProgram() && !IsMultipleSelectedPlayProgram();
        }

        private bool CanDeleteExcute()
        {
            return HasSelectedPlayProgram();
        }

        private bool IsMultipleSelectedPlayProgram()
        {
            return HasSelectedPlayProgram() && _selectedScheduleList.Count > 1 ? true : false;
        }

        private bool HasSelectedPlayProgram()
        {
            return _selectedSchedule != null ? true : false;
        }

        private Dictionary<string, CmdOnlineStatus> GetTerminalStatus()
        {
            if (_viewModelLocator.TerminalCollection == null && _viewModelLocator.TerminalCollection.TerminalList.Count == 0)
            {
                return new Dictionary<string, CmdOnlineStatus>();
            }

            Dictionary<string, CmdOnlineStatus> terminalStatusDic = new Dictionary<string, CmdOnlineStatus>();

            _viewModelLocator.TerminalCollection.TerminalList.ToList().ForEach(
            t =>
            {
                CmdOnlineStatus status;
                status = t.IsOnline ? CmdOnlineStatus.onLine : CmdOnlineStatus.offLine;
                terminalStatusDic.Add(t.Mac, status);
            });
            return terminalStatusDic;
        }

        private string GetPlayProgramNameList(ObservableCollection<ScheduleViewModel> playProgramList)
        {
            string nameList = string.Empty;
            foreach (var item in playProgramList)
            {
                nameList += Path.GetFileName(item.DisplayScheduleName) + "\r\n";
            }
            return nameList;
        }


        #region  --- 分组功能 ---
        private string _groupingName;
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

        private RelayCommand<object> _groupColumnCommand;
        private RelayCommand<object> _ungroupColumnCommand;
        private RelayCommand<object> _expandAllGroupsCommand;
        private RelayCommand<object> _collapseAllGroupsCommand;

        public RelayCommand<object> GroupColumnCommand
        {
            get
            {
                if (_groupColumnCommand == null)
                {
                    _groupColumnCommand = new RelayCommand<object>(
                        (T) =>
                    {
                        UnGroupColumnCommand.Execute(null);

                        AllPlayProgramsView.GroupDescriptions.Add(new PropertyGroupDescription(GroupingName));
                        _currentGroupName = GroupingName;
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
                        ExpandAllGroupsCommand.Execute(null);
                        AllPlayProgramsView.GroupDescriptions.Clear();
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
                        if (AllPlayProgramsView.Groups != null)
                        {
                            foreach (var groupItem in AllPlayProgramsView.Groups)
                            {
                                var group = groupItem as CollectionViewGroup;
                                     (group.Items[0] as ScheduleViewModel).IsExpanded = true;
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
                        if (AllPlayProgramsView.Groups != null)
                        {
                            foreach (var groupItem in AllPlayProgramsView.Groups)
                            {
                                var group = groupItem as CollectionViewGroup;
                                     (group.Items[0] as ScheduleViewModel).IsExpanded = false;
                            }
                        }
                    },
                        null);
                }
                return _collapseAllGroupsCommand;
            }
        }

        #endregion



    }
}
