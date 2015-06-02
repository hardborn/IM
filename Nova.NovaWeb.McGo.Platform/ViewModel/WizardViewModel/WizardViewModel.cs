

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Nova.NovaWeb.McGo.Platform.Utilities;
using Nova.Globalization;
using Nova.NovaWeb.McGo.Common;
using GalaSoft.MvvmLight.Threading;
using Nova.NovaWeb.McGo.DAL;
using System.Windows.Threading;
using Nova.NovaWeb.McGo.Utilities;
using System.Threading.Tasks;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class WizardViewModel : ViewModelBase
    {
        private Nova.NovaWeb.McGo.Common.PublishStatus _currentPublishStatus;
        private ReadOnlyCollection<PublishWizardPageViewModelBase> _pages;
        private PublishWizardPageViewModelBase _currentPage;
        private PublishDataViewModel _publishDataInfo;

        private ICommand _moveNextCommand;
        private ICommand _movePreviousCommand;
        private ICommand _cancelCommand;
        private RelayCommand<object> _closingWizardCommand;

        private ObservableCollection<TerminalViewModel> _currentTerminals;
        private ScheduleViewModel _currentSchedule;
        private WizardMode _wizardMode;
        public event EventHandler RequestClose;
        //public WizardViewModel()
        //{
        //    this.CurrentPage = this.Pages[0];
        //    Messenger.Default.Register<PropertyChangedMessage<bool>>(this, msg =>
        //    {
        //        if (msg.PropertyName == "IsAllPublishFinish")
        //            this.IsPublishing = !msg.NewValue;
        //        if (msg.PropertyName == "IsAllSuccess")
        //            this.IsSuccessPublished = msg.NewValue;

        //    });
        //}

        public WizardViewModel(ScheduleViewModel schedule, ObservableCollection<TerminalViewModel> terminals)
        {
            _currentTerminals = terminals;
            _currentSchedule = schedule;
            if (schedule == null && terminals != null)
            {
                _wizardMode = WizardMode.TerminalMode;
            }
            else
                if (schedule != null && terminals == null)
                {
                    _wizardMode = WizardMode.ScheduleMode;
                }

            InitializePublishDataInfo();

            this.CurrentPage = this.Pages[0];

            AppMessages.PublishStatusMessage.Register(this, OnPublishStatusChanged);

            Messenger.Default.Register<PropertyChangedMessage<bool>>(this, msg =>
            {
                if (msg.PropertyName == "IsAllPublishFinish")
                    this.IsPublishing = !msg.NewValue;
                if (msg.PropertyName == "IsAllSuccess")
                {
                    this.IsSuccessPublished = msg.NewValue;
                    if (this.IsSuccessPublished.Value)
                    {
                        this.IsCompletedCancel = true;
                    }
                }

            });
            Messenger.Default.Register<bool>(this, "CompletedCancel", message =>
            {
                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                {
                    this.IsCompletedCancel = message;
                }));
            });
            Messenger.Default.Register<NotificationMessage<string>>(this, ViewModelLocator.CLEANUP_NOTIFICATION,
            message =>
            {
                this.Cleanup();
            });

            Messenger.Default.Register<NotificationMessage>(this, "LogOutMessage", message =>
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    this.Cleanup();
                }));
            });
            Pages[0].IsCurrentPage = true;
            Pages[1].IsCurrentPage = false;
        }

        private void OnPublishStatusChanged(Common.PublishStatus obj)
        {
            CurrentPublishStatus = obj;
        }



        private void InitializePublishDataInfo()
        {
            PublishDataViewModel.Instance.ResetData();
            if (_wizardMode == WizardMode.ScheduleMode)
            {
                PublishDataViewModel.Instance.Schedule = _currentSchedule;
            }
            else
                if (_wizardMode == WizardMode.TerminalMode)
                {
                    //var publishTerminalList = 
                    foreach (var item in _currentTerminals)
                    {
                        var publishTerminalInfo = new PublishTerminalInfo();
                        publishTerminalInfo.TerminalInfo = item;
                        PublishDataViewModel.Instance.PublishTerminalInfoList.Add(publishTerminalInfo);
                    }
                }
        }

        public PublishWizardPageViewModelBase CurrentPage
        {
            get
            {
                return _currentPage;
            }
            private set
            {
                if (value == _currentPage)
                    return;

                if (_currentPage != null)
                {
                    //_currentPage.IsCurrentPage = false;
                }


                _currentPage = value;

                RaisePropertyChanged("CurrentPage");
                RaisePropertyChanged("IsOnLastPage");
            }
        }

        /*****************************************************/
        //正在停止和正在开启的实验 
        private bool _isSuspend = false;
        public bool IsSuspend
        {
            get
            {
                return _isSuspend;
            }
            set
            {
                if(value == _isSuspend)
                {
                    return;
                }
                _isSuspend = value;
                RaisePropertyChanged("IsSuspend");
            }
        }

        private bool _isBegining = false;
        public bool IsBegining
        {
            get
            {
                return _isBegining;
            }
            set
            {
                if (value == _isBegining)
                {
                    return;
                }
                _isBegining = value;
                RaisePropertyChanged("IsBegining");
            }
        }

        private bool _isGetData = false;
        public bool IsGetData
        {
            get
            {
                return _isGetData;
            }
            set
            {
                _isGetData = value;
                RaisePropertyChanged("IsGetData");
            }
        }
        /****************************************************/


        public bool IsOnLastPage
        {
            get
            {
                return Pages.IndexOf(this.CurrentPage) == this.Pages.Count - 1;
            }
        }

        private bool _isAutomaticRetry = false;
        public bool IsAutomaticRetry
        {
            get
            {
                return _isAutomaticRetry;
            }
            set
            {
                if (value == _isAutomaticRetry)
                {
                    return;
                }
                bool oldValue = _isAutomaticRetry;
                _isAutomaticRetry = value;
                RaisePropertyChanged("IsAutomaticRetry", oldValue, _isAutomaticRetry, true);
            }
        }

        private bool _isPublishing = false;
        public bool IsPublishing
        {
            get
            {
                return _isPublishing;
            }
            set
            {
                if (value == _isPublishing)
                    return;
                bool oldValue = _isPublishing;
                _isPublishing = value;
                RaisePropertyChanged("IsPublishing", oldValue, _isPublishing, true);
            }
        }

        private bool? _isSuccessPublished = null;
        public bool? IsSuccessPublished
        {
            get
            {
                return _isSuccessPublished;
            }
            set
            {
                if (value == _isSuccessPublished)
                    return;
                _isSuccessPublished = value;
                if (_isSuccessPublished == false || _isSuccessPublished == null)
                {
                    IsNoComplete = true;
                }
                else
                {
                    IsNoComplete = false;
                }
                RaisePropertyChanged("IsSuccessPublished");
            }
        }

        private bool _isNoComplete = true;
        public bool IsNoComplete
        {
            get
            {
                return _isNoComplete;
            }
            set
            {
                _isNoComplete = value;
                RaisePropertyChanged("IsNoComplete");
            }
        }

        private bool _isCompletedCancel = true;
        public bool IsCompletedCancel
        {
            get
            {
                return _isCompletedCancel;
            }
            set
            {
                if (value == _isCompletedCancel)
                    return;
                _isCompletedCancel = value;
                RaisePropertyChanged("IsCompletedCancel");
            }
        }

        public Nova.NovaWeb.McGo.Common.PublishStatus CurrentPublishStatus
        {
            get
            {
                return _currentPublishStatus;
            }
            set
            {
                if (_currentPublishStatus == value)
                    return;
                _currentPublishStatus = value;
                RaisePropertyChanged("CurrentPublishStatus");
            }
        }


        public PublishDataViewModel PublishDataInfo
        {
            get
            {
                return _publishDataInfo;
            }
            set
            {
                if (value == _publishDataInfo)
                {
                    return;
                }
                _publishDataInfo = value;
                RaisePropertyChanged("PublishDataInfo");
            }
        }


        public ReadOnlyCollection<PublishWizardPageViewModelBase> Pages
        {
            get
            {
                if (_pages == null)
                    this.CreatePages();

                return _pages;
            }
        }


        private void CreatePages()
        {
            var pages = new List<PublishWizardPageViewModelBase>();
            TerminalWizardPageViewModel terminalVM;
            ScheduleWizardPageViewModel scheduleVM;
            PublishWizardPageViewModel publishVM;

            switch (_wizardMode)
            {
                case WizardMode.ScheduleMode:
                    terminalVM = new TerminalWizardPageViewModel();
                    pages.Add(terminalVM);
                    break;
                case WizardMode.TerminalMode:
                    scheduleVM = new ScheduleWizardPageViewModel();
                    pages.Add(scheduleVM);
                    break;
                default:
                    break;
            }


            publishVM = new PublishWizardPageViewModel();
            pages.Add(publishVM);
             
            _pages = new ReadOnlyCollection<PublishWizardPageViewModelBase>(pages);
        }


        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand(this.CancelOrder, null);

                return _cancelCommand;
            }
        }

        void CancelOrder()
        {
            var publishWizardPageViewModel = CurrentPage as PublishWizardPageViewModel;
            if (publishWizardPageViewModel != null)
                publishWizardPageViewModel.CancelPublish();
            // this.OnRequestClose();
        }

        public ICommand MovePreviousCommand
        {
            get
            {
                if (_movePreviousCommand == null)
                    _movePreviousCommand = new RelayCommand(
                        this.MoveToPreviousPage,
                        () => this.CanMoveToPreviousPage);

                return _movePreviousCommand;
            }
        }

        private bool CanMoveToPreviousPage
        {
            get
            {
                return 0 < this.Pages.IndexOf(this.CurrentPage);
            }
        }

        private void MoveToPreviousPage()
        {
            _currentPage.IsCurrentPage = true;
            Pages[0].IsCurrentPage = true;
            Pages[1].IsCurrentPage = false;
            if (this.CanMoveToPreviousPage)
            {
                var publishPage = this.CurrentPage;
                this.CurrentPage = this.Pages[this.Pages.IndexOf(this.CurrentPage) - 1];
                if (publishPage != null && publishPage.IsPublishPage)
                {
                    (publishPage as PublishWizardPageViewModel).InitialPublishStatus();
                    (publishPage as PublishWizardPageViewModel).CancelPublish();
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        this.IsPublishing = false;
                        this.IsSuccessPublished = null;
                    }));

                    PublishDataViewModel.Instance.PublishTerminalInfoList.ToList().ForEach(p =>
                    {
                        p.PubSettingInfo = new PublishSettingInfo();
                        p.PubResult = new PublishResult();
                        p.ProgressPercentage = 0;
                        p.DataItemCount = 0;
                        p.CurrentDataItemIndex = 0;
                    });
                }
                if (this.CurrentPage.GetType().Equals(typeof(ScheduleWizardPageViewModel)))
                {
                    var scheduleWizardPage = this.CurrentPage as ScheduleWizardPageViewModel;
                    if (scheduleWizardPage == null)
                        return;
                    scheduleWizardPage.InitialViewSource();
                }
                if (this.CurrentPage.GetType().Equals(typeof(TerminalWizardPageViewModel)))
                {
                    var terminalWizardPage = this.CurrentPage as TerminalWizardPageViewModel;
                    if (terminalWizardPage == null)
                        return;
                    PublishDataViewModel.Instance.PublishTerminalInfoList.Clear();
                }
            }
        }

        public ICommand MoveNextCommand
        {
            get
            {
                if (_moveNextCommand == null)
                    _moveNextCommand = new RelayCommand(
                        this.MoveToNextPage,
                        () => this.CanMoveToNextPage);

                return _moveNextCommand;
            }
        }

        private bool CanMoveToNextPage
        {
            get
            {
                return this.CurrentPage != null && this.CurrentPage.IsValid();
            }
        }

        DispatcherTimer timer = new DispatcherTimer();
        private void MoveToNextPage()
        {
            Pages[0].IsCurrentPage = false;
            Pages[1].IsCurrentPage = true;
            

                if (this.CanMoveToNextPage)
                {
                    var schedulePage = this.CurrentPage as ScheduleWizardPageViewModel;
                    if (schedulePage != null)//this.CurrentPage.GetType().Equals(typeof(ScheduleWizardPageViewModel))
                    {
                        /*下一步 置回标志位*/
                        //for (int i = 0; i < PublishDataViewModel.Instance.PublishTerminalInfoList.Count; i++)
                        //{
                        //    PublishDataViewModel.Instance.PublishTerminalInfoList[i].FinishedAndSendOK = false;
                        //}
                        PublishDataViewModel.Instance.PublishTerminalInfoList.ToList().ForEach(p =>
                        {
                            p.PubResult = new PublishResult();
                        });
                        /********************************************************/

                        PreviewHelper.GetInstance().StopPreview();
                        if (!schedulePage.SelectedSchedule.ValidationMediaList())
                        {
                            if (Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,
                                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_MediaListEmpty", "播放方案中没有添加任何媒体，发布至终端将导致终端黑屏播放，是否继续发布？"),
                                string.Empty,
                                MessageBoxButton.YesNo) == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                    }


                    var terminalPage = this.CurrentPage as TerminalWizardPageViewModel;
                    if (terminalPage != null)
                    {
                        terminalPage.SelectedTerminalList.Clear();
                    }

                    if (this.Pages.IndexOf(this.CurrentPage) < this.Pages.Count - 1)
                    {
                        this.CurrentPage = this.Pages[this.Pages.IndexOf(this.CurrentPage) + 1];
                        var publishWizarPage = this.CurrentPage as PublishWizardPageViewModel;
                        if (publishWizarPage == null)
                            return;

                        publishWizarPage.SelectedPublishTerminalInfoList.Clear();
                        publishWizarPage.FirstTerminal = PublishDataViewModel.Instance.PublishTerminalInfoList[0];
                        publishWizarPage.SelectedPublishTerminalInfoList.Add(publishWizarPage.FirstTerminal);
                        publishWizarPage.TimingTime = DateTime.Now;
                    }
                    else
                    {
                      Action action = new Action(() =>
                      {  
                        if (this.CurrentPage.IsPublishPage)
                        {
                            var publishWizarPage = this.CurrentPage as PublishWizardPageViewModel;
                            if (publishWizarPage == null)
                                return;

                            if (IsPublishing == false && IsSuccessPublished != true)
                            {
                                

                                //DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                                //{
                                //    IsGetData = true;
                                //}));

                                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                                {
                                    this.IsPublishing = true;
                                    this.IsSuccessPublished = false;
                                    IsBegining = true;
                                }));


                                if (this.CurrentPage.IsPublishPage)
                                {
                                    var publishWizar = this.CurrentPage as PublishWizardPageViewModel;
                                    
                                        publishWizar.account = publishWizar.PlatformService.GetPlatformAccount();
                                        publishWizar.uploadInfo = publishWizar.TransmissionInfoService.GetPlatformDataTransInfo();
                                    if((publishWizar.account == null) || (publishWizar.uploadInfo == null))
                                    {
                                        MessageBox.Show(MultiLanguageUtils.GetLanguageString("Result_UploadSchedule_CanNotGetData", "未能获取数据"));
                                        DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                                        {
                                            IsBegining = false;
                                            IsPublishing = false;
                                        }));
                                        return;
                                    }
                                    
                                }


                                timer.Interval = TimeSpan.FromSeconds(1);
                                timer.Tick += new EventHandler(SartPublishTimerGo);
                                timer.Start();

                                /*尝试使用后台线程处理发布，防止UI阻塞*/
                                Task publish = new Task(new Action(publishWizarPage.PublishSchedule));
                                /**/
                                publish.Start();

                                return;
                            }

                            if (IsPublishing == true)
                            {
                                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                                {
                                    this.IsCompletedCancel = false;
                                    this.IsPublishing = false;
                                    this.IsSuccessPublished = false;
                                    IsSuspend = true;
                                }));


                                System.Diagnostics.Debug.WriteLine("---------MoveToNextPage-------进入publishWizarPage.CancelPublish()");
                                publishWizarPage.CancelPublish();

                                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                                {
                                    this.IsCompletedCancel = true;

                                }));
                                Messenger.Default.Send<bool>(true, "CompletedCancel");

                                timer.Interval = TimeSpan.FromSeconds(1);
                                timer.Tick += new EventHandler(StopPublishTimerGo);
                                timer.Start();

                                return;
                            }

                            if (IsPublishing == false && IsSuccessPublished == true)
                            {

                          
                                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                                {
                                    OnRequestClose();
                                }));
                            }
                        }
                      });
                      action.BeginInvoke(null, null);
                    }
                }         
          
        }

        void StopPublishTimerGo(object sender, EventArgs e)
        {
            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            {
                IsSuspend = false;
            }));
            timer.Stop();
        }

        void SartPublishTimerGo(object sender, EventArgs e)
        {

            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            {
                IsBegining = false;
            }));
            timer.Stop();
            //DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
            //{
            //    IsGetData = false;
            //}));
        }
        public RelayCommand<object> ClosingWizardCommand
        {
            get
            {
                if (_closingWizardCommand == null)
                {
                    _closingWizardCommand = new RelayCommand<object>((t) =>
                    {
                        ClosingWizard(t);
                    }, null);
                }
                return _closingWizardCommand;
            }
        }
        private RelayCommand _cancelPublishWizardCommand;
        public RelayCommand CancelPublishWizardCommand
        {
            get
            {
                if (_cancelPublishWizardCommand == null)
                {
                    _cancelPublishWizardCommand = new RelayCommand(() =>
                    {
                        this.RequestClose(null, null);
                    }, null);
                }
                return _cancelPublishWizardCommand;
            }
        }

        private void ClosingWizard(object t)
        {
            CancelEventArgs args = t as System.ComponentModel.CancelEventArgs;

            if (!CurrentPage.IsPublishPage)
            {
                Messenger.Default.Send<string>(string.Empty, "CloseReadbackView");
                Cleanup();
                if (args == null)
                    this.RequestClose(null, null);
                return;
            }
            else
                if (IsSuccessPublished != null && IsSuccessPublished.Value)
                {
                    Messenger.Default.Send<string>(string.Empty, "CloseReadbackView");
                    Cleanup();
                    if (args == null)
                        this.RequestClose(null, null);
                    return;
                }

            //if (args == null)
            //{
            //    Cleanup();
            //    return;
            //}

            string errorInfo;
            MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_ExitPublishWizardAlarm", out errorInfo);
            string errorInfo1;
            MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_Information", out errorInfo1);
            if (Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, errorInfo, errorInfo1, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Messenger.Default.Send<string>(string.Empty, "CloseReadbackView");
                Cleanup();
                if (args == null)
                    this.RequestClose(null, null);
                return;
            }
            else
            {
                if (args != null)
                    args.Cancel = true;
            }
        }

        private void OnRequestClose()
        {
            FileHelper.DeleteFiles(PlatformConfig.RemotePlutoPublicationDirectoryPath);
            EventHandler handler = this.RequestClose;
            
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            var publishWizardPageViewModel = CurrentPage as PublishWizardPageViewModel;
            if (publishWizardPageViewModel != null)
                publishWizardPageViewModel.CancelPublish();
            // Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>("PublishSuccess", "PublishSuccess"), "PublishSuccess");
            Messenger.Default.Unregister(this);
            PreviewHelper.GetInstance().StopPreview();
            PublishDataViewModel.Instance.ResetData();
        }
    }

    public enum WizardMode
    {
        ScheduleMode,
        TerminalMode
        //FullMode
    }
}
