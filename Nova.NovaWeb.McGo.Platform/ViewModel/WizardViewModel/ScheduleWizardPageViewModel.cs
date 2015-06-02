

using GalaSoft.MvvmLight.Command;
using Nova.NovaWeb;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.View;
using Nova.Xml.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Nova.NovaWeb.Player;
using System.Windows.Data;
using System.ComponentModel;
using Nova.NovaWeb.Common;
using Nova.Globalization;
using Nova.NovaWeb.McGo.Platform.Utilities;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class ScheduleWizardPageViewModel : PublishWizardPageViewModelBase
    {
        private ListCollectionView _allScheduleView;
        private ObservableCollection<ScheduleViewModel> _allScheduleList = new ObservableCollection<ScheduleViewModel>();
        private ICommand _newCommand;
        private ICommand _editCommand;
        #region preview
        private ScheduleViewModel _selectedSchedule = null;
        private string _tempText = "ManangementCenter_UI_Preview";
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


        private bool HasSelectedPlayProgram()
        {
            return PublishDataViewModel.Instance.Schedule != null ? true : false;
        }
        private RelayCommand _previewScheduleCommand;
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
                        return HasSelectedPlayProgram();// && !IsMultipleSelectedPlayProgram() ? true : false;
                    });
                }
                return _previewScheduleCommand;
            }
        }

        private void PreviewPlayProgram()
        {
            PreviewHelper.GetInstance().Initialize(PublishDataViewModel.Instance.Schedule);

            if (TempText == "ManangementCenter_UI_Preview")
            {
                PreviewHelper.GetInstance().StartPreview(PublishDataViewModel.Instance.Schedule);
            }
            else
                if (TempText == "ManangementCenter_UI_StopPreview")
                {
                    PreviewHelper.GetInstance().StopPreview();
                }
        }
        //private void StopPreview()
        //{
        //    if (_playHelper == null)
        //        return;
        //    //_playHelper.Key_EscKey_EnterEvent -= playHelper_Key_EscKey_EnterEvent;
        //    //_playHelper.StopPreview_ContrexMenuStrip_Event -= _playHelper_StopPreview_ContrexMenuStrip_Event;
        //    _playHelper.SetScreenHide(0, false);
        //    _playHelper.StopPlay(0);
        //    PublishDataInfo.Schedule = null;
        //    TempText = "ManangementCenter_UI_Preview";
        //}
        //private void playHelper_Key_EscKey_EnterEvent(object sender, EventArgs e)
        //{
        //    StopPreview();
        //}
        //private void playHelper_StopPreview_ContrexMenuStrip_Event(object sender, EventArgs e)
        //{
        //    StopPreview();
        //}
        #endregion
        #region create playprogram
        private ICommand _newScheduleCommand;

        public FrmPlayProgramEditor fEditor = null;

        public ICommand NewScheduleCommand
        {
            get
            {
                if (_newScheduleCommand == null)
                {
                    _newScheduleCommand = new RelayCommand(
                        () => CreateSchedule(),
                        () =>
                    {
                        return true;
                    });
                }
                return _newScheduleCommand;
            }
        }
        private void CreateSchedule()
        {
            var terminalRepository = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            var terminals = from item in terminalRepository.TerminalList
                                                            select item.CurrentTerminal.CurrentTerminal;
            var platForm = AppEnvionment.Default.Get<IPlatformService>();

            try
            {
                //Modify-lixc
               fEditor = new FrmPlayProgramEditor(
                    PlatformConfig.LocalScheduleDirectoryPath,
                    string.Empty,
                    PlatformConfig.LanguageResourcePath,
                    _platformService.GetPlatformLanguage(),
                    _platformService.GetAppData().ScheduleScreenSize,
                    true,
                    terminals.ToList(),
                    PublishDataViewModel.Instance.IsIncludingTerminals(TerminalType.Embedded));
                fEditor.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                fEditor.ShowInTaskbar = false;
                fEditor.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                _platformService.GetAppData().ScheduleScreenSize = fEditor.PlayProgramSize;
                _platformService.Save();
                GetScheduleList();

                //Messenger.Default.Register<NotificationMessage>(this, "AutoLogoutAccout", message =>
                //{
                //    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                //    {
                //        if (fEditor != null)
                //        {
                //            fEditor.dispose();
                //        }
                //    }));
                //});

            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message);
                }));
            }
        }
        private ICommand _editorCommand;
        public ICommand EditorScheduleCommand
        {
            get
            {
                if (_editorCommand == null)
                {
                    _editorCommand = new RelayCommand(
                        () => EditorSchedule(),
                        () =>
                    {
                        return true;
                    });
                }
                return _editorCommand;
            }
        }
        private void EditorSchedule()
        {
            var terminalRepository = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            var terminals = from item in terminalRepository.TerminalList
                                                            select item.CurrentTerminal.CurrentTerminal;

            try
            {
                //Modify-lixc
               fEditor = new FrmPlayProgramEditor(
                     PlatformConfig.LocalScheduleDirectoryPath,
                     PublishDataViewModel.Instance.Schedule.FilePath,
                     PlatformConfig.LanguageResourcePath,
                     _platformService.GetPlatformLanguage(),
                    new System.Drawing.Size(PublishDataViewModel.Instance.Schedule.ScreenWidth, PublishDataViewModel.Instance.Schedule.ScreenHeight),
                    true,
                    terminals.ToList(),
                    PublishDataViewModel.Instance.Schedule.Type == ScheduleType.Common ? true : false);
                fEditor.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                fEditor.ShowInTaskbar = false;
                fEditor.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                fEditor.PlayProgramSaveFinishedEvent += (s, i) =>
                {
                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshSchedule"), "RefreshSchedule");
                    GetScheduleList();
                };
                
                //Messenger.Default.Register<NotificationMessage>(this, "AutoLogoutAccout", message =>
                //{
                //    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                //    {
                //        if (fEditor != null)
                //        {
                //            fEditor.dispose();
                //        }
                //    }));
                //});

            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message);
                }));
            }
        }
        #endregion
        private IPlatformService _platformService;

        //public ScheduleWizardPageViewModel()//(PublishDataViewModel publishInfo)
        //{
        //    var info = this.GetType().GetProperties();
        //    _platformService = AppEnvionment.Default.Get<IPlatformService>();
        //    //PublishInfo = publishInfo;
        //    //InitialPublishData();
        //    //InitialPlayHelper();
        //    RefreshScheduleList();
        //    ProcessingMessages();
        //}

        public ScheduleWizardPageViewModel()
        {
            _platformService = AppEnvionment.Default.Get<IPlatformService>();

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

            ProcessingMessages();
            GetScheduleList();
            InitialViewSource();
        }

        public override void Cleanup()
        {
            // Clean up if needed
            base.Cleanup();
            Messenger.Default.Unregister(this);
        }

        public void InitialViewSource()
        {
            AllScheduleView = CollectionViewSource.GetDefaultView(AllScheduleList) as ListCollectionView;
            AllScheduleView.SortDescriptions.Clear();
            AllScheduleView.SortDescriptions.Add(new SortDescription("LastEditTime", ListSortDirection.Descending));
            SelectedSchedule = PublishDataViewModel.Instance.Schedule;
        }
        private void ProcessingMessages()
        {
            Messenger.Default.Register<NotificationMessage<string>>(this, "StopPreview", message =>
            {
                //_currentPreviewSchedule = null;
                TempText = "ManangementCenter_UI_Preview";
            });

            Messenger.Default.Register<NotificationMessage<string>>(this, "StartPreview", message =>
            {
                //_currentPreviewSchedule = _selectedSchedule;
                TempText = "ManangementCenter_UI_StopPreview";
            });
        }

        private bool HasEmbeddedInTargetTerminals()
        {
            bool result = false;

            foreach (var item in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (item.TerminalInfo.Type == Nova.NovaWeb.McGo.Common.TerminalType.Embedded)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public ObservableCollection<ScheduleViewModel> AllScheduleList
        {
            get
            {
                return _allScheduleList;
            }
            set
            {
                if (_allScheduleList == value)
                {
                    return;
                }
                _allScheduleList = value;
                RaisePropertyChanged("AllScheduleList");
            }
        }

        public override string DisplayName
        {
            get
            {
                string info;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_PlayProgram", out info);
                return info;
            }
        }

        internal override bool IsValid()
        {
            if(SelectedSchedule == null)
                return false;
            else
                return true;
        }

        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(
                        () => NewSchedule(),
                        () =>
                    {
                        return true;
                    });
                }
                return _newCommand;
            }
        }


        private void NewSchedule()
        {
            TerminalRepositoryViewModel terminalRepositoryViewModel = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();

            bool isAsyncSchedule = terminalRepositoryViewModel.SelectedTerminals.Any(t => t.Type == TerminalType.Embedded);

            var terminals = from item in terminalRepositoryViewModel.TerminalList
                                                            select item.CurrentTerminal.CurrentTerminal;

            try
            {
                //Modify-lixc
                fEditor = new FrmPlayProgramEditor(
                     PlatformConfig.LocalScheduleDirectoryPath,
                     null,
                     PlatformConfig.LanguageResourcePath,
                     _platformService.GetPlatformLanguage(),
                     new System.Drawing.Size(350, 350),  //_platformService.GetAppData().ScheduleScreenSize
                     true,
                     terminals.ToList(),
                     isAsyncSchedule);
                fEditor.PlayProgramSaveFinishedEvent += (s, i) =>
                {
                    _platformService.GetAppData().ScheduleScreenSize = fEditor.PlayProgramSize;
                    _platformService.Save();
                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshSchedule"), "RefreshSchedule");
                    GetScheduleList();
                };
                fEditor.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                fEditor.ShowInTaskbar = false;
                fEditor.ShowDialog(new Wfp32Window(App.Current.MainWindow));

                //Messenger.Default.Register<NotificationMessage>(this, "AutoLogoutAccout", message =>
                //{
                //    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                //    {
                //        if (fEditor != null)
                //        {
                //            fEditor.dispose();
                //        }
                //    }));
                //});

            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message);
                }));
            }
           
        }

        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(
                        () => EditSchedule(),
                        () =>
                    {
                        return PublishDataViewModel.Instance.Schedule != null ? true : false;
                    });
                }
                return _editCommand;
            }
        }

        public ListCollectionView AllScheduleView
        {
            get
            {
                return _allScheduleView;
            }
            private set
            {
                _allScheduleView = value;
                RaisePropertyChanged("AllScheduleView");
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
                    return;
                _selectedSchedule = value;
                PublishDataViewModel.Instance.Schedule = value;
                RaisePropertyChanged("SelectedSchedule");
            }
        }

        private void GetScheduleList()
        {
            var scheduleRepository = AppEnvionment.Default.Get<IScheduleRepositoryProvider>();
            var scheduleList = scheduleRepository.FindAll();
            if (scheduleList == null)
            {
                return;
            }
            AllScheduleList.Clear();
            if (HasEmbeddedInTargetTerminals())
            {
                foreach (var item in scheduleList)
                {
                    if (item.Type == ScheduleType.Common)
                    {
                        AllScheduleList.Add(new ScheduleViewModel(new ScheduleModel(item)));
                    }
                }
            }
            else
            {
                foreach (var item in scheduleList)
                {
                    AllScheduleList.Add(new ScheduleViewModel(new ScheduleModel(item)));
                }
            }

           
        }



        private void EditSchedule()
        {
            TerminalRepositoryViewModel terminalRepositoryViewModel = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();

            var terminals = from item in terminalRepositoryViewModel.TerminalList
                                                            select item.CurrentTerminal.CurrentTerminal;

            try
            {
                //Modify-lixc
               fEditor = new FrmPlayProgramEditor(
                   PlatformConfig.LocalScheduleDirectoryPath,
                    PublishDataViewModel.Instance.Schedule.FilePath,
                   PlatformConfig.LanguageResourcePath,
                   _platformService.GetPlatformLanguage(),
                    new System.Drawing.Size(PublishDataViewModel.Instance.Schedule.ScreenWidth, PublishDataViewModel.Instance.Schedule.ScreenHeight),
                    true,
                    terminals.ToList(),
                    PublishDataViewModel.Instance.Schedule.Type == ScheduleType.PC ? false : true);

                fEditor.PlayProgramSaveFinishedEvent += (s, i) =>
                {
                    Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshSchedule"), "RefreshSchedule");
                    GetScheduleList();
                };
                fEditor.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                fEditor.ShowInTaskbar = false;
                fEditor.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                
                //Messenger.Default.Register<NotificationMessage>(this, "AutoLogoutAccout", message =>
                //{
                //    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                //    {
                //        if (fEditor != null)
                //        {
                //            fEditor.dispose();
                //        }
                //    }));
                //});

            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message);
                }));
            }
        }


        private List<FileInfo> GetFiles(string path, string extName)
        {
            return GetDir(path, extName);
        }


        private List<FileInfo> GetDir(string path, string extName)
        {
            List<FileInfo> lst = new List<FileInfo>();
            try
            {
                string[] dir = Directory.GetDirectories(path);
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                //FileInfo[] file = Directory.GetFiles(path); //文件列表   
                if (file.Length != 0 || dir.Length != 0)
                {
                    foreach (FileInfo f in file)
                    {
                        if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0)
                        {
                            lst.Add(f);
                        }
                    }
                    foreach (string d in dir)
                    {
                        GetDir(d, extName);
                    }
                }
            }
            catch
            {
            }
            ;
            return lst;
        }
    }
}
