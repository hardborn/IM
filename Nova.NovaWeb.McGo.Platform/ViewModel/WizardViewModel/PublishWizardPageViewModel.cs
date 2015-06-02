using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Nova.Globalization;
using Nova.MD5;
using Nova.Net.Http;
using Nova.NovaWeb;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.MD5;
using Nova.NovaWeb.OperationCommon;
using Nova.NovaWeb.Protocol;
using Nova.Pluto.PlutoManager.UI;
using Nova.Studio.Codec;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;
using Nova.NovaWeb.Windows.ViewModel;
using Nova.NovaWeb.Windows;
using System.Diagnostics;
using GalaSoft.MvvmLight.Threading;
using TimeZoneInfo = Nova.NovaWeb.Common.TimeZoneInfo;
using Nova.NovaWeb.McGo.Utilities;
using Nova.Systems;
using TimeZoneInformation = Nova.NovaWeb.McGo.Common.TimeZoneInformation;
using Nova.Windows.Forms;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class PublishWizardPageViewModel : PublishWizardPageViewModelBase
    {
        private TimeZoneInformation _selectedTimeZoneInfo = null;
        private List<TimeZoneInformation> _timeZoneTable = new List<TimeZoneInformation>();
        private Nova.NovaWeb.McGo.Common.TimeZoneTable _allTimeZone;
        private IPlatformService _platformService;
        private ITransmissionInfoService _transmissionInfoService;

        private BackgroundWorker _pcPublishWorker;
        private BackgroundWorker _embeddedPublishWorker;

        private RelayCommand<object> _rePublishScheduleCommand;
        private RelayCommand<object> _reSendCommandCommand;
        private RelayCommand<object> _dateTimeValueChangedCommand;

        private ListUploader2013 uploader;
        private ListUploader2013 embeddedUploader;

        private ObservableCollection<PublishTerminalInfo> _selectedPublishTerminalInfoList = new ObservableCollection<PublishTerminalInfo>();
        public Account account;

        public TransmissionInfo uploadInfo;

        public IPlatformService PlatformService
        {
            get
            {
                return _platformService;
            }
            set
            {
                _platformService = value;
                RaisePropertyChanged("PlatformService");
            }
        }

        public ITransmissionInfoService TransmissionInfoService
        {
            get
            {
                return _transmissionInfoService;
            }
            set
            {
                _transmissionInfoService = value;
                RaisePropertyChanged("TransmissionInfoService");
            }
        }

        public PublishWizardPageViewModel()
        {
            _platformService = AppEnvionment.Default.Get<IPlatformService>();
            _transmissionInfoService = AppEnvionment.Default.Get<ITransmissionInfoService>();



            IsPublishPage = true;

            _allTimeZone = AppEnvionment.Default.Get<IPlatformService>().GetTimeZoneTable();

            _timeZoneTable = _allTimeZone.TimeZoneInfoList;

            Messenger.Default.Register<PropertyChangedMessage<bool>>(this, msg =>
            {
                if (msg.PropertyName == "IsAutomaticRetry")
                    this.IsAutoRetry = msg.NewValue;
            });
        }

        private PublishTerminalInfo _firstTerminal = null;
        public PublishTerminalInfo FirstTerminal
        {
            get
            {
                return _firstTerminal;
            }
            set
            {
                //if (_firstTerminal == value)
                //    return;
                _firstTerminal = value;
                RaisePropertyChanged("FirstTerminal");
            }
        }


        private bool _isPublishing = false;


        private bool _isAutoRetry = false;
        public bool IsAutoRetry
        {
            get
            {
                return _isAutoRetry;
            }
            set
            {
                if (_isAutoRetry == value)
                    return;
                _isAutoRetry = value;
                RaisePropertyChanged("IsAutoRetry");
            }
        }

        private bool _isCancelSuccessed = true;

        public bool IsCancelSuccessed
        {
            get
            {
                return _isCancelSuccessed;
            }
            set
            {
                if (_isCancelSuccessed == value)
                    return;
                _isCancelSuccessed = value;
                RaisePropertyChanged("IsCancelSuccessed");
            }
        }


        #region publish schedule
        public void PublishSchedule()
        {
            //PublishDataViewModel.Instance.PublishTerminalInfoList.ToList().ForEach(p =>
            //{
            //    //p.PubSettingInfo = new PublishSettingInfo();
            //    p.PubResult = new PublishResult();
            //    p.ProgressPercentage = 0;
            //    p.DataItemCount = 0;
            //    p.CurrentDataItemIndex = 0;
            //});

            //PublishDataViewModel.Instance.PublishTerminalInfoList.ToList().ForEach(p =>
            //{
            //    p.PubResult = new PublishResult();
            //});

            AppMessages.PublishStatusMessage.Send(Common.PublishStatus.Publishing);

            if (PublishDataViewModel.Instance.PublishTerminalInfoList == null || PublishDataViewModel.Instance.PublishTerminalInfoList == null)
            {
                throw new NullReferenceException("PublishSchedule");
            }

            if (PublishDataViewModel.Instance.IsIncludingTerminals(TerminalType.Embedded))
            {
                _isEmbeddedPublishFinish = false;

                if (IsNeedConvert())
                {
                    bool isConvertSuccess = ConvertSchedule();
                    if (!isConvertSuccess)
                    {
                        IsAllPublishFinish = true;
                        IsAllSuccess = false;
                        AppMessages.PublishStatusMessage.Send(Common.PublishStatus.PublishFailed);
                        return;
                    }
                }

                UploadEmbeddedSchedule();
            }
            else
            {
                _isEmbeddedPublishFinish = true;
            }


            if (!PublishDataViewModel.Instance.IsIncludingTerminals(TerminalType.PC))
            {
                _isPcPublishFinish = true;
                IsAllPublishFinish = _isPcPublishFinish && _isEmbeddedPublishFinish;
                return;
            }

            UploadPCSchedule();
        }
        #endregion
        #region embedded upload
        private List<string> _embeddedUploadMediaList = null;
        public void UploadEmbeddedSchedule()
        {
            _embeddedUploadMediaList = new List<string>();

            //var account = _platformService.GetPlatformAccount();
            //var uploadInfo = _transmissionInfoService.GetPlatformDataTransInfo();

            _embeddedPublishWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _embeddedPublishWorker.DoWork += (o, e) =>
            {
                embeddedUploader = new PlayProgramUploader2013(
                      account.Name,
                    account.Password,
                    new Uri(uploadInfo.DestinationAddress),
                    uploadInfo.Account,
                    uploadInfo.Password,
                    new Uri(uploadInfo.DestinationAddress),
                    uploadInfo.Account,
                    uploadInfo.Password,
                    Encoding.UTF8);

                var res = PlutoPlayProgramMD5Builder2013.MatchAllFileMD5OfPlayProgram(
                    PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath,
                    new List<string>() {
                            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath,
                            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath
                        },
                    false,
                    string.Empty,
                    false);
                if (res != PlayProgramCheckRes.ok)
                {
                    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                        () =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, string.Format(MultiLanguageUtils.GetLanguageString("Result_UploadSchedule_CreatePlutoMD5Fail", "生成通用播放方案的MD5失败")));
                    }
                        ));
                    IsAllPublishFinish = true;
                    IsAllSuccess = false;
                    return;
                }

                var serverInfo = _platformService.GetPlatformServerInfo();

                embeddedUploader.ListFileInquireUri = ServiceHelper.GetCurrentPhpServiceURL();
                embeddedUploader.ListFileUploadUri = ServiceHelper.GetCurrentPhpServiceURL();
                embeddedUploader.MediaFileInquireUri = ServiceHelper.GetCurrentPhpServiceURL();
                embeddedUploader.MediaFileUploadUri = ServiceHelper.GetCurrentPhpServiceURL();

                embeddedUploader.IsEnableSsl = _platformService.GetAppData().IsEnableFTPS;

                embeddedUploader.SingleFileUploadCompleteEvent -= embeddedUploader_SingleFileUploadCompleteEvent;
                embeddedUploader.SingleFileUpLoader_ProgressChangedEvent -= embeddedUploader_SingleFileUpLoader_ProgressChangedEvent;
                embeddedUploader.ListUploadInfoEvent -= embeddedUploader_ListUploadInfoEvent;
                embeddedUploader.ListUpLoadCompleteEvent -= embeddedUploader_ListUpLoadCompleteEvent;

                embeddedUploader.SingleFileUploadCompleteEvent += embeddedUploader_SingleFileUploadCompleteEvent;
                embeddedUploader.SingleFileUpLoader_ProgressChangedEvent += embeddedUploader_SingleFileUpLoader_ProgressChangedEvent;
                embeddedUploader.ListUploadInfoEvent += embeddedUploader_ListUploadInfoEvent;
                embeddedUploader.ListUpLoadCompleteEvent += embeddedUploader_ListUpLoadCompleteEvent;

                embeddedUploader.UploadList(
                    PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath,
                    new List<string>() {
                             PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath,
                            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath
                        }, 
                    false,
                    false,
                    string.Empty,
                    false,
                    "Embedded");
            };

            _embeddedPublishWorker.RunWorkerCompleted += (o, ea) =>
            {

            };

            _embeddedPublishWorker.RunWorkerAsync();
        }

        void embeddedUploader_ListUpLoadCompleteEvent(object sender, ListTransmiteCompleteEventArgs e)
        {
            if (e.TransmiteRes != ListTransmiteRes.OK)
            {
                //embeddedUploader.SingleFileUploadCompleteEvent -= embeddedUploader_SingleFileUploadCompleteEvent;
                //embeddedUploader.SingleFileUpLoader_ProgressChangedEvent -= embeddedUploader_SingleFileUpLoader_ProgressChangedEvent;
                //embeddedUploader.ListUploadInfoEvent -= embeddedUploader_ListUploadInfoEvent;
                //embeddedUploader.ListUpLoadCompleteEvent -= embeddedUploader_ListUpLoadCompleteEvent;
                //embeddedUploader.SingleFileUploadCompleteEvent -= embeddedUploader_SingleFileUploadCompleteEvent;
                //embeddedUploader.SingleFileUpLoader_ProgressChangedEvent -= embeddedUploader_SingleFileUpLoader_ProgressChangedEvent;
                //embeddedUploader.ListUploadInfoEvent -= embeddedUploader_ListUploadInfoEvent;
                //embeddedUploader.ListUpLoadCompleteEvent -= embeddedUploader_ListUpLoadCompleteEvent;
                //Messenger.Default.Send<bool>(true, "CompletedCancel");
            }
            //int terminalCount = PublishDataViewModel.Instance.PublishTerminalInfoList.Count;
            //int i = 0;
            foreach (var pubInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (pubInfo.TerminalInfo.Type == TerminalType.PC)
                {
                    //i++;
                    continue;
                }

                //if (pubInfo.FinishedAndSendOK == true)
                //{
                //    i++;
                //    continue;
                //}

                if (pubInfo.PubResult.CommandSendResult == CommandSendResult.SendOk)
                {
                    //i++;
                    continue;
                }
                if (e.TransmiteRes == ListTransmiteRes.OK)
                {
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        pubInfo.ProgressPercentage = 100;
                    }));
                    pubInfo.PubResult.TransmiteResult = ListTransmiteRes.OK;

                    //PublishDataViewModel.Instance.PublishTerminalInfoList[i].FinishedAndSendOK = true;

                    pubInfo.PubResult.CommandSendResult = SendPlayProgramDownloadCmd(
                        pubInfo.TerminalInfo.GroupId,
                        pubInfo.TerminalInfo.GroupName,
                        pubInfo.TerminalInfo.Mac,
                        pubInfo.TerminalInfo.Type);

                    Debug.WriteLine("*****************");
                    Debug.WriteLine("异步下载命令已发送");
                    Debug.WriteLine("*****************");
                }
                else
                {
                    pubInfo.PubResult.TransmiteResult = e.TransmiteRes;
                }
            }


            if (!IsAllTerminalsSuccess(TerminalType.Embedded) && IsAutoRetry)
                UploadEmbeddedSchedule();
            else
            {
                _isEmbeddedPublishFinish = true;
                IsAllPublishFinish = _isPcPublishFinish && _isEmbeddedPublishFinish;
            }
        }

        void embeddedUploader_ListUploadInfoEvent(object sender, ListTransmitInfoArgs e)
        {
            var fileList = e.TransmitFileList;
            for (var i = 0; i < e.TransmitMediaFileNameList.Count; i++)
            {
                fileList.Add(e.TransmitMediaFileNameList[i]);
            }
            bool isSuccess = PublishDataViewModel.Instance.Schedule.PublishPathInfo.SetPlutoUploadMediaListWeights(fileList);
            if (!isSuccess)
            {
                this.CancelPublish();
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                       () =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_UploadSchedule_MediaNotExist", "播放方案中的部分媒体在服务端和本地都不存在"));
                }
                       ));
                return;
            }
            foreach (var pubInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (pubInfo.TerminalInfo.Type == TerminalType.PC)
                    continue;
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                  () =>
                  {
                      pubInfo.DataItemCount = e.TransmitFileList.Count;

                  }));
               
            }
            //PublishDataViewModel.Instance.Schedule.PublishPathInfo.;
            //EmbeddedPublishInfo.PublishItemCount = e.TransmitMediaFileNameList.Count + e.TransmitFileList.Count;
            //EmbeddedPublishInfo.PublishedItemCount = 0;
            //if (e.TransmitMediaFileNameList.Count != 0)
            //{
            //    EmbeddedPublishInfo.ProgressMessage = "正在上传媒体文件";
            //}
            //else if (e.TransmitFileList.Count != 0)
            //{
            //    EmbeddedPublishInfo.ProgressMessage = "正在上传播放方案";
            //}
        }

        void embeddedUploader_SingleFileUpLoader_ProgressChangedEvent(object sender, FileSetTransmitProgressChangedEventArgs e)
        {
            var fileName = e.FileInfo.TransfersInfo.FileNameInfo;
            //string fileName = Path.GetDirectoryName(e.FileInfo.TransfersInfo.FileNameInfo);
            //fileName = Path.Combine(fileName, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(e.FileInfo.TransfersInfo.FileNameInfo)) + Path.GetExtension(e.FileInfo.TransfersInfo.FileNameInfo));
            //EmbeddedPublishInfo.ProgressPercentage = EmbeddedPublishInfo.PublishedItemCount * e.FileInfo.TransmitePercentage / EmbeddedPublishInfo.PublishItemCount;
            //EmbeddedPublishInfo.ProgressMessage = "正在上传：" + Path.GetFileName(fileName);
            var uploadProcess = 0;
            if (_embeddedUploadMediaList.Count != 0)
            {
                foreach (var mediaName in _embeddedUploadMediaList)
                {
                    uploadProcess += PublishDataViewModel.Instance.Schedule.PublishPathInfo.DicPlutoMediaFileWeights[mediaName];
                }
            } 
            uploadProcess += e.FileInfo.TransmitePercentage * PublishDataViewModel.Instance.Schedule.PublishPathInfo.DicPlutoMediaFileWeights[fileName] / 100;
            foreach (var pubInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (pubInfo.TerminalInfo.Type == TerminalType.PC)
                    continue;
                //pubInfo.CurrentDataItemIndex++;
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                () =>
                {
                    pubInfo.ProgressPercentage = uploadProcess; 

                }));
                //pubInfo.DataItemCount * 100 / pubInfo.DataItemCount;
            }
        }

        void embeddedUploader_SingleFileUploadCompleteEvent(object sender, TransmiteSingleFileEventArgs e)
        {
            var fileName = e.Token.ToString();
            //string fileName = Path.GetDirectoryName(e.Token.ToString());
            //fileName = Path.Combine(fileName, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(e.Token.ToString())) + Path.GetExtension(e.Token.ToString()));
            var uploadProcess = 0;
            if (_embeddedUploadMediaList.Count != 0)
            {
                foreach (var mediaName in _embeddedUploadMediaList)
                {
                    uploadProcess += PublishDataViewModel.Instance.Schedule.PublishPathInfo.DicPlutoMediaFileWeights[mediaName];
                }
            }
            uploadProcess += PublishDataViewModel.Instance.Schedule.PublishPathInfo.DicPlutoMediaFileWeights[fileName];
            foreach (var pubInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (pubInfo.TerminalInfo.Type == TerminalType.PC)
                    continue;
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
               () =>
               {
                   pubInfo.CurrentDataItemIndex++;
                   pubInfo.ProgressPercentage = uploadProcess;
               }));
               
            }
            _embeddedUploadMediaList.Add(fileName);
        }
        #endregion
        #region history inquiry
        private RelayCommand _inquiryPublishHistoryCommand;
        public ICommand InquiryPublishHistoryCommand
        {
            get
            {
                if (_inquiryPublishHistoryCommand == null)
                {
                    _inquiryPublishHistoryCommand = new RelayCommand(
                        InquiryPublishHistroy,
                        () =>
                    {
                        if (PublishDataViewModel.Instance.PublishTerminalInfoList.Count > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
                return _inquiryPublishHistoryCommand;
            }
        }
        private void InquiryPublishHistroy()
        {
            //Modify-lixc
            var terminals = new List<TerminalModel>();
            PublishDataViewModel.Instance.PublishTerminalInfoList.ToList().ForEach(e => terminals.Add(e.TerminalInfo.CurrentTerminal));
            var inquiryViewModel = new InquiryWindowViewModel(
                terminals,
                new List<CmdTypes>() { CmdTypes.downloadPlaySchedule },
                new List<CmdTypes>() { CmdTypes.downloadPlaySchedule },
                 PlatformConfig.LanguageResourcePath,
                _platformService.GetPlatformLanguage(),
                true
                );//Path.Combine(ClientManager.LanguageResourcePath, @"zh-CN\TerminalControl.zh-CN.resources")
            var inquireCommandWindow = new InquireCommandWindow();
            inquireCommandWindow.DataContext = inquiryViewModel;
            inquireCommandWindow.Owner = App.Current.MainWindow;
            inquireCommandWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            inquireCommandWindow.ShowInTaskbar = false;
            inquireCommandWindow.ShowDialog();
        }
        #endregion
        #region retry
        private bool _isAllSuccess = false;
        public bool IsAllSuccess
        {
            get
            {
                return _isAllSuccess;
            }
            set
            {
                //if(_isAllSuccess == value)
                //    return;
                bool oldvalue = _isAllSuccess;
                _isAllSuccess = value;
                RaisePropertyChanged("IsAllSuccess", oldvalue, _isAllSuccess, true);
            }
        }

        private bool IsAllTerminalsSuccess(TerminalType terType)
        {
            foreach (var pbInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if ((pbInfo.TerminalInfo.Type == terType && pbInfo.PubResult.TransmiteResult != ListTransmiteRes.OK) ||
                    pbInfo.PubResult.CommandSendResult != CommandSendResult.SendOk)
                {
                    IsAllSuccess = false;
                    return IsAllSuccess;
                }
            }
            IsAllSuccess = true;
            return IsAllSuccess;
        }
        bool IsRetry(bool isPC)
        {
            var tType = TerminalType.Embedded;
            if (isPC)
                tType = TerminalType.PC;
            foreach (var pbInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (pbInfo.TerminalInfo.Type != tType)
                    continue;
                if (pbInfo.PubResult.TransmiteResult != ListTransmiteRes.OK)
                    return true;
            }
            return false;
        }
        private bool _isAllPublishFinish;
        public bool IsAllPublishFinish
        {
            get
            {
                return _isAllPublishFinish;
            }
            set
            {
                //if(_isAllPublishFinish == value)
                //    return;
                bool oldValue = _isAllPublishFinish;
                _isAllPublishFinish = value;
                RaisePropertyChanged("IsAllPublishFinish", oldValue, _isAllPublishFinish, true);
            }
        }
        private bool _isEmbeddedPublishFinish = false;
        private bool _isPcPublishFinish = false;
        #endregion


        #region pc upload

        private List<string> _uploadMediaList = null;
        public void UploadPCSchedule()
        {
            System.Diagnostics.Debug.WriteLine("---------UploadPCSchedule-------进入UploadPCSchedule()");
            _uploadMediaList = new List<string>();
       
            _pcPublishWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            System.Diagnostics.Debug.WriteLine("---------UploadPCSchedule-------准备进入UploadPcSchedule()");
            _pcPublishWorker.DoWork += (o, e) => UploadPcSchedule(account, uploadInfo);

            _pcPublishWorker.RunWorkerCompleted += (o, ea) =>
            {

            };

            _pcPublishWorker.RunWorkerAsync();
        }


        private void UploadPcSchedule(Account account, TransmissionInfo uploadInfo)
        {
            System.Diagnostics.Debug.WriteLine("---------UploadPcSchedule-------已进入UploadPcSchedule()");
            if (account == null)
            {
                throw new NullReferenceException("Account");
            }
            if (uploadInfo == null)
            {
                throw new NullReferenceException("TransmissionInfo");
            }
            System.Diagnostics.Debug.WriteLine("---------UploadPcSchedule-------初始化uploader");
            uploader = new PlayProgramUploader2013(
                account.Name,
                account.Password,
                new Uri(uploadInfo.DestinationAddress), //"ftp://192.168.0.188/"),
                uploadInfo.Account,
                uploadInfo.Password,
                new Uri(uploadInfo.DestinationAddress), //"ftp://192.168.0.188/"),//
                uploadInfo.Account,
                uploadInfo.Password,
                Encoding.UTF8);
            System.Diagnostics.Debug.WriteLine("---------UploadPcSchedule-------初始化uploader完成");
            var res = PlayProgramMD5Builder2013.MatchAllFileMD5OfPlayProgram(
                PublishDataViewModel.Instance.Schedule.PublishPathInfo.CloudFilePath,
                false,
                string.Empty,
                false);
            if (res != PlayProgramCheckRes.ok)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                    () => Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, string.Format(MultiLanguageUtils.GetLanguageString("Result_UploadSchedule_CreatePCMD5Fail", "生成同步播放方案的MD5失败")))));
                IsAllPublishFinish = true;
                IsAllSuccess = false;
                return;
            }

            var serverInfo = _platformService.GetPlatformServerInfo();

            uploader.ListFileInquireUri = ServiceHelper.GetCurrentPhpServiceURL();
            uploader.ListFileUploadUri = ServiceHelper.GetCurrentPhpServiceURL();
            uploader.MediaFileInquireUri = ServiceHelper.GetCurrentPhpServiceURL();
            uploader.MediaFileUploadUri = ServiceHelper.GetCurrentPhpServiceURL();

            //uploader.ListFileUploadUri = @"https://192.168.0.234/cgi-bin/DPY1000001/playprogupdate.php";
            uploader.IsEnableSsl = _platformService.GetAppData().IsEnableFTPS;

            uploader.SingleFileUploadCompleteEvent -= uploader_SingleFileUploadCompleteEvent;
            uploader.SingleFileUpLoader_ProgressChangedEvent -= uploader_SingleFileUpLoader_ProgressChangedEvent;
            uploader.ListUploadInfoEvent -= uploader_ListUploadInfoEvent;
            uploader.ListUpLoadCompleteEvent -= uploader_ListUpLoadCompleteEvent;

            uploader.SingleFileUploadCompleteEvent += uploader_SingleFileUploadCompleteEvent;
            uploader.SingleFileUpLoader_ProgressChangedEvent += uploader_SingleFileUpLoader_ProgressChangedEvent;
            uploader.ListUploadInfoEvent += uploader_ListUploadInfoEvent;
            uploader.ListUpLoadCompleteEvent += uploader_ListUpLoadCompleteEvent;

            System.Diagnostics.Debug.WriteLine("---------UploadPcSchedule-------准备进入uploader.UploadList");
            uploader.UploadList(
                PublishDataViewModel.Instance.Schedule.PublishPathInfo.CloudFilePath,
                new List<string>(),
                false,
                false,
                string.Empty,
                true,
                "PC");
        }


        void uploader_ListUpLoadCompleteEvent(object sender, ListTransmiteCompleteEventArgs e)
        {
            if (e.TransmiteRes != ListTransmiteRes.OK)
            {
                //uploader.SingleFileUploadCompleteEvent -= uploader_SingleFileUploadCompleteEvent;
                //uploader.SingleFileUpLoader_ProgressChangedEvent -= uploader_SingleFileUpLoader_ProgressChangedEvent;
                //uploader.ListUploadInfoEvent -= uploader_ListUploadInfoEvent;
                //uploader.ListUpLoadCompleteEvent -= uploader_ListUpLoadCompleteEvent;
                // Messenger.Default.Send<bool>(true, "CompletedCancel");
            }
            //int terminalCount = PublishDataViewModel.Instance.PublishTerminalInfoList.Count;
            //int i = 0;
            foreach (var pbInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {

                if (pbInfo.TerminalInfo.Type != TerminalType.PC)
                {
                    //i++;
                    continue;
                }

                //if (pbInfo.FinishedAndSendOK == true)
                //{
                //    i++;
                //    continue;
                //}

                if (pbInfo.PubResult.CommandSendResult == CommandSendResult.SendOk)
                {
                    //i++;
                    continue;
                }

                if (e.TransmiteRes == ListTransmiteRes.OK)
                {
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        pbInfo.ProgressPercentage = 100;
                    }));
                    pbInfo.PubResult.TransmiteResult = e.TransmiteRes;

                    //PublishDataViewModel.Instance.PublishTerminalInfoList[i].FinishedAndSendOK = true;

                    pbInfo.PubResult.CommandSendResult = SendPlayProgramDownloadCmd(
                                   pbInfo.TerminalInfo.GroupId,
                                   pbInfo.TerminalInfo.GroupName,
                                   pbInfo.TerminalInfo.Mac,
                                   pbInfo.TerminalInfo.Type);

                    Debug.WriteLine("*****************");
                    Debug.WriteLine("同步下载命令已发送");
                    Debug.WriteLine("*****************");
                }
                else
                    if (e.TransmiteRes == ListTransmiteRes.Cancelled)
                    {
                    }
                    else
                    {
                        pbInfo.PubResult.TransmiteResult = e.TransmiteRes;
                    }
            }
            if (!IsAllTerminalsSuccess(TerminalType.PC) && IsAutoRetry)
                UploadPCSchedule();
            else
            {
                _isPcPublishFinish = true;
                IsAllPublishFinish = _isPcPublishFinish && _isEmbeddedPublishFinish;
            }
            //i++;
        }

        void uploader_ListUploadInfoEvent(object sender, ListTransmitInfoArgs e)
        {
            var fileList = e.TransmitFileList;
            for (var i = 0; i < e.TransmitMediaFileNameList.Count; i++)
            {
                fileList.Add(e.TransmitMediaFileNameList[i]);
            }
            var isSuccess =  PublishDataViewModel.Instance.Schedule.PublishPathInfo.SetCloudUploadMediaListWeights(fileList);
            if (!isSuccess)
            {
                this.CancelPublish();
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                       () =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_UploadSchedule_MediaNotExist", "播放方案中的部分媒体在服务端和本地都不存在"));
                }
                       ));
                return;
            }
            foreach (var pubInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (pubInfo.TerminalInfo.Type != TerminalType.PC)
                    continue;
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                   () =>
                   {
                       pubInfo.DataItemCount = e.TransmitFileList.Count;

                   }));
            }

            //PcPublishInfo.PublishItemCount = e.TransmitMediaFileNameList.Count + e.TransmitFileList.Count;
            //PcPublishInfo.PublishedItemCount = 0;
            //if (e.TransmitMediaFileNameList.Count != 0)
            //{
            //    PcPublishInfo.ProgressMessage = "正在上传媒体文件";
            //}
            //else if (e.TransmitFileList.Count != 0)
            //{
            //    PcPublishInfo.ProgressMessage = "正在上传播放方案";
            //}
        }

        void uploader_SingleFileUpLoader_ProgressChangedEvent(object sender, FileSetTransmitProgressChangedEventArgs e)
        {
            var fileName = e.FileInfo.TransfersInfo.FileNameInfo;
            var uploadProcess = 0;
            if (_uploadMediaList.Count != 0)
            {
                foreach (var mediaName in _uploadMediaList)
                {
                    uploadProcess += PublishDataViewModel.Instance.Schedule.PublishPathInfo.DicCloudMediaFileWeights[mediaName];
                }
            }
            uploadProcess += e.FileInfo.TransmitePercentage * PublishDataViewModel.Instance.Schedule.PublishPathInfo.DicCloudMediaFileWeights[fileName] / 100;
            foreach (var pubInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (pubInfo.TerminalInfo.Type != TerminalType.PC)
                    continue;
                //pubInfo.CurrentDataItemIndex++;
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                    () =>
                    {
                        pubInfo.ProgressPercentage = uploadProcess;
                    }));
                 //pubInfo.DataItemCount * 100 / pubInfo.DataItemCount;
            }


            //PcPublishInfo.ProgressPercentage = PcPublishInfo.PublishedItemCount * e.FileInfo.TransmitePercentage / PcPublishInfo.PublishItemCount;
            //PcPublishInfo.ProgressMessage = "正在上传：" + Path.GetFileName(fileName);
        }

        void uploader_SingleFileUploadCompleteEvent(object sender, TransmiteSingleFileEventArgs e)
        {
            var fileName = e.Token.ToString();
            var uploadProcess = 0;
            if (_uploadMediaList.Count != 0)
            {
                foreach (var mediaName in _uploadMediaList)
                {
                    uploadProcess += PublishDataViewModel.Instance.Schedule.PublishPathInfo.DicCloudMediaFileWeights[mediaName];
                }
            }
            uploadProcess += PublishDataViewModel.Instance.Schedule.PublishPathInfo.DicCloudMediaFileWeights[fileName];
            Debug.WriteLine("uploader_SingleFileUploadCompleteEvent:" + uploadProcess);
            foreach (var pubInfo in PublishDataViewModel.Instance.PublishTerminalInfoList)
            {
                if (pubInfo.TerminalInfo.Type != TerminalType.PC)
                    continue;
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(
                    () =>
                    {
                        pubInfo.CurrentDataItemIndex++;
                        pubInfo.ProgressPercentage = uploadProcess;
                    }));
                //pubInfo.DataItemCount * 100 / pubInfo.DataItemCount;
            }
            _uploadMediaList.Add(fileName);
            //string fileName = e.Token.ToString();
            //PcPublishInfo.PublishedItemCount++;
            //PcPublishInfo.ProgressPercentage = PcPublishInfo.PublishedItemCount * 100 / PcPublishInfo.PublishItemCount;
            //PcPublishInfo.ProgressMessage = Path.GetFileName(fileName) + "上传完成";
        }
        #endregion
        public void CancelPublish()
        {
            System.Diagnostics.Debug.WriteLine("---------CancelPublish-------进入CancelPublish()");
            IsAutoRetry = false;
            IsCancelSuccessed = false;
            if (embeddedUploader != null)
            {
                embeddedUploader.SingleFileUploadCompleteEvent -= embeddedUploader_SingleFileUploadCompleteEvent;
                embeddedUploader.SingleFileUpLoader_ProgressChangedEvent -= embeddedUploader_SingleFileUpLoader_ProgressChangedEvent;
                embeddedUploader.ListUploadInfoEvent -= embeddedUploader_ListUploadInfoEvent;
                embeddedUploader.ListUpLoadCompleteEvent -= embeddedUploader_ListUpLoadCompleteEvent;
                try
                {
                    embeddedUploader.CancelTransmite();
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

            System.Diagnostics.Debug.WriteLine("---------CancelPublish-------判断uploader!= null");
            if (uploader == null)
            {
                Debug.WriteLine("uploader==null");
            }

            if (uploader != null)
            {
                System.Diagnostics.Debug.WriteLine("---------CancelPublish-------uploader!= null判断完成，准备进入uploader.CancelTransmite()");
                uploader.SingleFileUploadCompleteEvent -= uploader_SingleFileUploadCompleteEvent;
                uploader.SingleFileUpLoader_ProgressChangedEvent -= uploader_SingleFileUpLoader_ProgressChangedEvent;
                uploader.ListUploadInfoEvent -= uploader_ListUploadInfoEvent;
                uploader.ListUpLoadCompleteEvent -= uploader_ListUpLoadCompleteEvent;
                try
                {
                    uploader.CancelTransmite();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
        }

        #region 发布播放方案下载命令

        private CommandSendResult SendPlayProgramDownloadCmd(int groupId, string groupName, string mac, TerminalType type)
        {
            CmdPubRequest request = SetCmdData(groupId, groupName, mac, type);
            if (request == null)
                return CommandSendResult.SendFailure;
            return Post(request);
        }

        private CommandSendResult Post(CmdPubRequest request)
        {
            //var serverInfo = _platformService.GetPlatformServerInfo();
            var cmdPublishUrl = ServiceHelper.GetCurrentPhpServiceURL();

            var hClient = new WebServerRequest();
            ProtocolRequestData pReqData = new ProtocolRequestData();
            pReqData.PID = ProtocolID.cmdPublish;
            pReqData.Url = cmdPublishUrl;
            pReqData.ReplyDataType = typeof(CmdPubReply);
            pReqData.RequestDataObj = request;
            RequestInfo resInfo;
            hClient.Post(pReqData, out resInfo);
            if (resInfo.WebRequestRes != WebRequestRes.OK)
                return CommandSendResult.SendFailure;
            CmdPubReply pubReply = (CmdPubReply)resInfo.ReplyObj;
            if (pubReply == null || pubReply.SitePubResList == null)
                return CommandSendResult.SendFailure;
            for (int i = 0; i <  pubReply.SitePubResList.Count       ; i++)
            {
                if (pubReply.SitePubResList[0].PubRes != SysErrorCode.Operation_OK)
                    return CommandSendResult.SendFailure;
            }
            return CommandSendResult.SendOk;
        }
        /// <summary>
        /// 设置命令发布参数
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="mac"></param>
        /// <param name="playProgramPathInServer"></param>
        /// <param name="mediaDirInServer"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private CmdPubRequest SetCmdData(int groupId, string groupName, string mac, TerminalType type)
        {
            CmdPubRequest request = new CmdPubRequest();
            request.PubCmdList = new List<PubCmd>();
            PublishTerminalInfo pubInfo = PublishDataViewModel.Instance.PublishTerminalInfoList.FirstOrDefault(a => a.TerminalInfo.Mac == mac);
            if (pubInfo == null)
                return null;

            PubCmd pCmd;

            pCmd = new PubCmd();
            pCmd.MacList = new List<string>();
            pCmd.MacList.Add(pubInfo.TerminalInfo.Mac);
            pCmd.Cmd = new Cmd();
            pCmd.Cmd.CmdType = CmdTypes.downloadPlaySchedule;
            pCmd.Cmd.ExcuDT = pubInfo.PubSettingInfo.ExecuteTime == null ? DateTime.UtcNow : pubInfo.PubSettingInfo.ExecuteTime.Value;
            pCmd.Cmd.Mode = pubInfo.PubSettingInfo.CmdMode;
            pCmd.Cmd.OverdueDT = pubInfo.PubSettingInfo.OverdueTime == null ? DateTime.UtcNow.AddMonths(1) : pubInfo.PubSettingInfo.OverdueTime.Value;
            string addFile = string.Empty;
            string filepath;
            if (type == TerminalType.Embedded)
            {
                addFile = Path.GetFileName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath) + "*" + Path.GetFileName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath);
                filepath = Path.GetFileName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath);
            }
            else
            {
                filepath = Path.GetFileName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.CloudFilePath);
            }
            pCmd.Cmd.Para = string.Format("{0}+{1}+{2}+{3}+{4}",
                filepath,
                addFile,
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                10 * 1024 * 1024,
                (int)Nova.Algorithms.CheckCodes.MD5Helper.CaculateMode.Sample);
            ;
            pCmd.Cmd.SetDT = DateTime.UtcNow;
            pCmd.Cmd.Status = CmdStatus.active;
            request.PubCmdList.Add(pCmd);
            return request;
        }
        #endregion

        static AutoResetEvent autoEvent;
        private bool IsNeedConvert()
        {
            if (!File.Exists(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath))
                return true;
            var schFileDir = Path.GetDirectoryName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath);
            string fileNameWithoutEx; //= Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.CloudFilePath);
            string[] plymInfoList;
            string[] mdcorInfoList;
            string[] plpymInfoList;
            if (PublishDataViewModel.Instance.Schedule.SourceType == ScheduleSourceType.LocalSource)
            {
                fileNameWithoutEx = Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.CloudFilePath);
                plymInfoList = Directory.GetFiles(schFileDir, fileNameWithoutEx + ".*.plym", SearchOption.TopDirectoryOnly);
                mdcorInfoList = Directory.GetFiles(schFileDir, fileNameWithoutEx + ".*.mdcor", SearchOption.TopDirectoryOnly);
                plpymInfoList = Directory.GetFiles(schFileDir, fileNameWithoutEx + ".*.plpym", SearchOption.TopDirectoryOnly);
            }
            else
            {
                string plymFileNameWithoutEx = Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath);
                string mdcorFileNameWithoutEx = Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath);
                string plpymFileNameWithoutEx = Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath);
                plymInfoList = Directory.GetFiles(schFileDir, plymFileNameWithoutEx + ".plym", SearchOption.TopDirectoryOnly);
                mdcorInfoList = Directory.GetFiles(schFileDir, mdcorFileNameWithoutEx + ".mdcor", SearchOption.TopDirectoryOnly);
                plpymInfoList = Directory.GetFiles(schFileDir, plpymFileNameWithoutEx + ".plpym", SearchOption.TopDirectoryOnly);
            }
           //var plymInfoList = Directory.GetFiles(schFileDir, fileNameWithoutEx + ".*.plym", SearchOption.TopDirectoryOnly);
           //var mdcorInfoList = Directory.GetFiles(schFileDir, fileNameWithoutEx + ".*.mdcor", SearchOption.TopDirectoryOnly);
           //var plpymInfoList = Directory.GetFiles(schFileDir, fileNameWithoutEx + ".*.plpym", SearchOption.TopDirectoryOnly);
            if (plymInfoList.Length != 1 || mdcorInfoList.Length != 1 || plpymInfoList.Length != 1)
            {
                try
                {
                    foreach (var plymInfo in plymInfoList)
                    {
                        if (File.Exists(plymInfo))
                            File.Delete(plymInfo);
                    }
                    foreach (var mdcorInfo in mdcorInfoList)
                    {
                        if (File.Exists(mdcorInfo))
                            File.Delete(mdcorInfo);
                    }
                    foreach (var plpymInfo in plymInfoList)
                    {
                        if (File.Exists(plpymInfo))
                            File.Delete(plpymInfo);
                    }
                }
                catch
                {
                }
                return true;
            }
            List<string> addInfoList = new List<string>();
            addInfoList.Add(plpymInfoList[0]);
            addInfoList.Add(mdcorInfoList[0]);
            if (PlutoPlayProgramMD5Builder2013.MatchAllFileMD5OfPlayProgram(plymInfoList[0], addInfoList, false, "", true) != PlayProgramCheckRes.ok)
            {
                try
                {
                    foreach (var plymInfo in plymInfoList)
                    {
                        if (File.Exists(plymInfo))
                            File.Delete(plymInfo);
                    }
                    foreach (var mdcorInfo in mdcorInfoList)
                    {
                        if (File.Exists(mdcorInfo))
                            File.Delete(mdcorInfo);
                    }
                    foreach (var plpymInfo in plpymInfoList)
                    {
                        if (File.Exists(plpymInfo))
                            File.Delete(plpymInfo);
                    }
                }
                catch
                {
                }
                return true;
            }
            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath = plymInfoList[0];
            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath = mdcorInfoList[0];
            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath = plpymInfoList[0];
            return false;
        }
        private bool ConvertSchedule()
        {
            //_schedule.PublishPathInfo.PlutoFilePath = Path.Combine(Path.GetDirectoryName(CurrentSchedule.PublishPathInfo.PublishPathInfo.PlutoFilePath), Path.GetFileName(CurrentSchedule.FilePath));


            try
            {
                autoEvent = new AutoResetEvent(false);

                bool isConvertSuccess = true;


                //File.Copy(PublishDataViewModel.Instance.Schedule.FilePath, PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, true);
                //File.Copy(PublishDataViewModel.Instance.Schedule.FilePath, Path.ChangeExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, "bak"), true);
                File.Copy(PublishDataViewModel.Instance.Schedule.FilePath, PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, true);


                var scheduleConverter = new Frm_ConvertProcess(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mencoder.exe"),
                new List<string> { PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath },
                Path.Combine(Path.GetDirectoryName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath), "Media"),
                Path.GetDirectoryName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath),
                PlayFileExtensionType.Plpym);
                {
                    //PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath = Path.ChangeExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, ".plpym");
                    PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath = Path.ChangeExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, ".mdcor");
                    PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath = Path.ChangeExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, ".plpym");
                }
                var platformService = AppEnvionment.Default.Get<IPlatformService>();
                CustomMessageBox.LangFileName=Path.Combine(PlatformConfig.LanguageResourcePath, platformService.GetPlatformLanguage(), "Convert." + platformService.GetPlatformLanguage() + ".resources");
                
                scheduleConverter.UpdateLanguage(Path.Combine(PlatformConfig.LanguageResourcePath, platformService.GetPlatformLanguage(), "Convert." + platformService.GetPlatformLanguage() + ".resources"));
                scheduleConverter.FFMEPGToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

                
                

                scheduleConverter.CompleteConvertEvent += (s, e) =>
                {

                    e.IsCloseFrm = true;

                    if (e.IsAllConvertSucceed)
                    {
                        //scheduleConverter.Close();

                        if (PublishDataViewModel.Instance.Schedule.SourceType == ScheduleSourceType.LocalSource)
                        {
                            string tempFilePath =  Path.ChangeExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath, ".temp");
                            string plpymFilePath =   Path.ChangeExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, ".plpym");
                            string plymFilePath = Path.ChangeExtension(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath, ".plym");

                            File.Copy(plpymFilePath, tempFilePath, true);
                            File.Copy(plymFilePath, plpymFilePath, true);
                            File.Copy(tempFilePath, plymFilePath, true);
                        }

                        if (PublishDataViewModel.Instance.Schedule.SourceType == ScheduleSourceType.RemoteSource && !ConvertAndModifySchedule())
                        {
                            Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_PlutoMediaConvert_ScheduleChangeFail", "播放方案修改失败"));
                            isConvertSuccess = false;
                        }
                        //File.Copy(CurrentSchedule.FilePath, Path.ChangeExtension(CurrentSchedule.PublishPathInfo.PlutoFilePath, "plpym"), true);


                        if (GeneratePublishScheduleFile())
                            isConvertSuccess = true;
                        else
                            isConvertSuccess = false;
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_PlutoMediaConvert_Failed", "媒体转换失败"));
                        //scheduleConverter.Close();
                        isConvertSuccess = false;
                    }
                    autoEvent.Set();
                    //scheduleConverter.Close();
                };
                scheduleConverter.IsNoticeFileNotExist = false;
                scheduleConverter.IsCaculatorMediaMD5 = true;
                //scheduleConverter.MatchedInfoSaveType = MediaMatchedInfoSaveType.CustomSave;
                scheduleConverter.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                scheduleConverter.ShowInTaskbar = false;
                //处理其他线程无法调用转换窗口的问题
                DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                {
                    scheduleConverter.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                }));
                //scheduleConverter.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                autoEvent.WaitOne();
                return isConvertSuccess;
            }
            catch (IOException ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_PlutoMediaConvert_CopyFailed", "在转换中文件复制异常"));
                autoEvent.Set();
                return false;
            }
            catch (Exception ex)
            {
                autoEvent.Set();
                return false;
            }
            finally
            {
            }
        }

        private bool ConvertAndModifySchedule()
        {
            string a, b;
            string sourceScheduleFilePath;
            string sourceConvertedScheduleFilePath;
            string convertedScheduleFilePath;
            if (PublishDataViewModel.Instance.Schedule.SourceType == ScheduleSourceType.LocalSource)
            {
                sourceScheduleFilePath = PublishDataViewModel.Instance.Schedule.FilePath;
                sourceConvertedScheduleFilePath = Path.Combine(PlatformConfig.LocalPlutoPublicationDirectoryPath, Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.FilePath) + ".plpym");
                convertedScheduleFilePath = Path.Combine(PlatformConfig.LocalPlutoPublicationDirectoryPath, Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.FilePath) + ".plpym");
            }
            else
            {
                sourceScheduleFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, PublishDataViewModel.Instance.Schedule.RemoteInfo.OriginalScheduleName);
                sourceConvertedScheduleFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, PublishDataViewModel.Instance.Schedule.RemoteInfo.OptimizedScheduleName);
                //sourceScheduleFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, PublishDataViewModel.Instance.Schedule.RemoteInfo.OptimizedScheduleName);
                //sourceConvertedScheduleFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, PublishDataViewModel.Instance.Schedule.RemoteInfo.OriginalScheduleName);
                if (PublishDataViewModel.Instance.Schedule.RemoteInfo.OptimizedScheduleName == "")
                {
                    sourceScheduleFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.RemoteInfo.OriginalScheduleName) + ".plpym");
                    File.Copy(sourceConvertedScheduleFilePath, sourceScheduleFilePath);
                }
                convertedScheduleFilePath = Path.Combine(PlatformConfig.RemotePlutoPublicationDirectoryPath, Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.FilePath) + ".plpym");
            }
            bool isScucess = false;
            try
            {
                isScucess = ConvertedPlayProgramModify.ModifyConvertedPlayProgram(
                sourceConvertedScheduleFilePath,
                sourceScheduleFilePath,
                convertedScheduleFilePath,
                out a,
                out b);
            }
            catch (Exception ex)
            {
                return false;
            }

            string plutoConverterFilePath = Path.Combine(Path.GetDirectoryName(convertedScheduleFilePath), Path.GetFileNameWithoutExtension(PublishDataViewModel.Instance.Schedule.FilePath) + ".plpym");
            try
            {
                File.Copy(PublishDataViewModel.Instance.Schedule.FilePath, plutoConverterFilePath, true);
            }
            catch (Exception ex)
            {
            }

            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath = a;
            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath = b;
            PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath = plutoConverterFilePath;
            return isScucess;
        }

        private bool GeneratePublishScheduleFile()
        {
            //异步
            try
            {
                string scheduleConvertedMd5, scheduleMcordMd5, scheduleMd5;


                Nova.Algorithms.CheckCodes.MD5Helper.CreateMD5(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath, out scheduleConvertedMd5);
                Nova.Algorithms.CheckCodes.MD5Helper.CreateMD5(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath, out scheduleMcordMd5);
                Nova.Algorithms.CheckCodes.MD5Helper.CreateMD5(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, out scheduleMd5);
                //string plutoConverterFilePath = PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath;
                //string plutoMdcorFilePath = PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath;
                //string plutoFilePath = PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath;

                //return FilterFilePathMD5(plutoConverterFilePath);
                var plutoConvertedFilePath = Path.Combine(
                      Path.GetDirectoryName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath),
                      String.Format("{0}.{1}.plpym",
                      Path.GetFileNameWithoutExtension(FilterFilePathMD5(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath)), scheduleConvertedMd5));

                var plutoMdcorFilePath = Path.Combine(
                          Path.GetDirectoryName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath),
                          String.Format("{0}.{1}.mdcor",
                          Path.GetFileNameWithoutExtension(FilterFilePathMD5(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath)), scheduleMcordMd5));

                var plutoFilePath = Path.Combine(
                        Path.GetDirectoryName(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath),
                        String.Format("{0}.{1}.plym",
                        Path.GetFileNameWithoutExtension(FilterFilePathMD5(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath)), scheduleMd5));

                //File.Copy(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath, plutoConvertedFilePath, true);
                //File.Copy(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath, plutoMdcorFilePath, true);
                //File.Copy(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, plutoFilePath, true);

                FileCopy(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath, plutoConvertedFilePath,true);
                FileCopy(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath, plutoMdcorFilePath, true);
                FileCopy(PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath, plutoFilePath, true);
                ///同步播放方案
                //File.Copy(PublishDataViewModel.Instance.Schedule.FilePath, PublishDataViewModel.Instance.Schedule.PublishPathInfo.CloudFilePath, true);


                PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoConverterFilePath = plutoConvertedFilePath;
                PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoMdcorFilePath = plutoMdcorFilePath;
                PublishDataViewModel.Instance.Schedule.PublishPathInfo.PlutoFilePath = plutoFilePath;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void FileCopy(string sourcePath, string targetPath, bool overwrite)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(targetPath))
                return;
            if (sourcePath == targetPath)
                return;

            File.Copy(sourcePath, targetPath, true);
        }

        private static string FilterFilePathMD5(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;
            string[] splitedStrs = filePath.Split(new char[] { '.' });
            if (splitedStrs.Length > 3)
            {
                return splitedStrs[0] + "." + splitedStrs[1] + "." + splitedStrs[splitedStrs.Length - 1];
            }
            else
            {
                return filePath;
            }
        }

        public override string DisplayName
        {
            get
            {
                string info;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_PublishWizardPage", out info);
                return info == null ? string.Empty : info;
            }
        }

        internal override bool IsValid()
        {
            return true;
        }


        private bool _isSelectedItem = false;

        public bool IsSelectedItem
        {
            get
            {
                return _isSelectedItem;
            }
            set
            {
                bool oldValue = _isSelectedItem;
                _isSelectedItem = value;
                RaisePropertyChanged("IsSelectedItem", oldValue, _isSelectedItem, true);
            }
        }


        public List<TimeZoneInformation> TimeZoneTable
        {
            get
            {
                return _timeZoneTable;
            }
            set
            {
                if (_timeZoneTable == value)
                    return;
                _timeZoneTable = value;
                RaisePropertyChanged("TimeZoneTable");
            }
        }

        public Nova.NovaWeb.McGo.Common.TimeZoneInformation SelectedTimeZoneInfo
        {
            get
            {
                return _selectedTimeZoneInfo;
            }
            set
            {
                //if (_selectedTimeZoneInfo == value)
                //    return;
                _selectedTimeZoneInfo = value;
                RaisePropertyChanged("SelectedTimeZoneInfo");
                UpdataSelectedItemPublishInfo();
            }
        }

        public ObservableCollection<PublishTerminalInfo> SelectedPublishTerminalInfoList
        {
            get
            {
                return _selectedPublishTerminalInfoList;
            }
            set
            {
                if (_selectedPublishTerminalInfoList == value)
                {
                    return;
                }
                _selectedPublishTerminalInfoList = value;
                RaisePropertyChanged("SelectedPublishTerminalInfoList");
            }
        }


        private ICommand _selectionChangedCommand;
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

        private void TerminalSelectionChanged(object sender)
        {
            var e = sender as System.Windows.Controls.SelectionChangedEventArgs;
            if (e == null)
            {
                return;
            }
            var dataGrid = e.Source as System.Windows.Controls.DataGrid;
            if (dataGrid == null)
            {
                return;
            }


            if (dataGrid.SelectedItems != null)
            {
                foreach (PublishTerminalInfo item in e.AddedItems)
                {
                    if (!SelectedPublishTerminalInfoList.Contains(item))
                    {
                        SelectedPublishTerminalInfoList.Add(item);
                    }
                }
                foreach (PublishTerminalInfo item in e.RemovedItems)
                {
                    SelectedPublishTerminalInfoList.Remove(item);
                }

                IsRightNow = false;
                IsDefault = false;
                IsTimer = false;
                TimingTime = DateTime.Now;
                IsEnableTimeZone = false;
                SelectedTimeZoneInfo = null;
            }

            if (SelectedPublishTerminalInfoList.Any())
            {
                IsSelectedItem = true;
            }
            else
            {
                IsSelectedItem = false;
            }
            e.Handled = true;
        }

        private bool? _isRightNow = false;
        public bool? IsRightNow
        {
            get
            {
                return _isRightNow;
            }
            set
            {
                if (_isRightNow == value)
                {
                    return;
                }

                _isRightNow = value;
                RaisePropertyChanged("IsRightNow");
                UpdataSelectedItemPublishInfo();
            }
        }

        private void UpdataSelectedItemPublishInfo()
        {
            if (!SelectedPublishTerminalInfoList.Any())
                return;

            if (_isRightNow == true)
            {
                foreach (var item in SelectedPublishTerminalInfoList)
                {
                    item.PubSettingInfo.CmdMode = CmdModeTypes.immediate;
                    item.PubSettingInfo.ExecuteTime = DateTime.UtcNow;
                    item.PubSettingInfo.OverdueTime = DateTime.UtcNow.AddMonths(1);
                }
            }
            else
                if (_isDefault == true)
                {
                    foreach (var item in SelectedPublishTerminalInfoList)
                    {
                        item.PubSettingInfo.CmdMode = CmdModeTypes.Default;
                        item.PubSettingInfo.ExecuteTime = DateTime.UtcNow;
                        item.PubSettingInfo.OverdueTime = DateTime.UtcNow.AddMonths(1);
                    }
                }
                else
                    if (_isTimer == true && _timingTime != null)
                    {
                        //var timeZoneInfo = SelectedTimeZoneInfo;
                        foreach (var item in SelectedPublishTerminalInfoList)
                        {
                            if (SelectedTimeZoneInfo == null)
                                item.PubSettingInfo.TimeZoneInfo = _allTimeZone.GetTimeZoneInfoById(item.TerminalInfo.TimeZoneId);
                            else
                                item.PubSettingInfo.TimeZoneInfo = SelectedTimeZoneInfo;
                            item.PubSettingInfo.CmdMode = CmdModeTypes.timing;
                            DateTime currentDateTime = item.PubSettingInfo.TimeZoneInfo.GetSystemTime(_timingTime.Value, true);

                            item.PubSettingInfo.ExecuteTime = new DateTime(item.PubSettingInfo.TimeZoneInfo.GetSystemTime(_timingTime.Value, true).Ticks, DateTimeKind.Utc);
                            item.PubSettingInfo.OverdueTime = item.PubSettingInfo.ExecuteTime.Value.AddMonths(1);
                        }
                    }
                    else
                    {
                        return;
                    }
        }

        private bool _isEnableTimeZone = false;
        public bool IsEnableTimeZone
        {
            get
            {
                return _isEnableTimeZone;
            }
            set
            {
                if (_isEnableTimeZone == value)
                    return;
                _isEnableTimeZone = value;
                RaisePropertyChanged("IsEnableTimeZone");
                UpdataSelectedItemPublishInfo();
            }
        }

        private bool? _isDefault = false;

        public bool? IsDefault
        {
            get
            {
                return _isDefault;
            }
            set
            {
                if (_isDefault == value)
                {
                    return;
                }
                _isDefault = value;
                RaisePropertyChanged("IsDefault");
                UpdataSelectedItemPublishInfo();
            }
        }

        private bool? _isTimer = false;

        public bool? IsTimer
        {
            get
            {
                return _isTimer;
            }
            set
            {
                if (_isTimer == value)
                {
                    return;
                }
                _isTimer = value;
                RaisePropertyChanged("IsTimer");
                UpdataSelectedItemPublishInfo();
            }
        }

        private DateTime? _timingTime;

        public DateTime? TimingTime
        {
            get
            {
                return _timingTime;
            }
            set
            {
                if (_timingTime == value)
                    return;
                _timingTime = value;
                RaisePropertyChanged("TimingTime");
                UpdataSelectedItemPublishInfo();
            }
        }

        private bool _isShowSetting = true;
        public bool IsShowSetting
        {
            get
            {
                return _isShowSetting;
            }
            set
            {
                if (value == _isShowSetting)
                {
                    return;
                }
                _isShowSetting = value;
                RaisePropertyChanged("IsShowSetting");
            }
        }


        public void InitialPublishStatus()
        {
            this._isAllPublishFinish = false;
            this._isAllSuccess = false;
            this._isEmbeddedPublishFinish = false;
            this._isPcPublishFinish = false;
            this._isPublishing = false;
            this._isSelectedItem = false;
            this._isRightNow = false;
            this._isTimer = false;
            this._isDefault = false;
            if (this._pcPublishWorker != null)
            {
                this._pcPublishWorker.Dispose();
                this._pcPublishWorker = null;
            }
            if (this._embeddedPublishWorker != null)
            {
                this._embeddedPublishWorker.Dispose();
                this._embeddedPublishWorker = null;
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Messenger.Default.Unregister(this);
        }

        public RelayCommand<object> DateTimeValueChangedCommand
        {
            get
            {
                if (_dateTimeValueChangedCommand == null)
                {
                    _dateTimeValueChangedCommand = new RelayCommand<object>(T => TimeValueChanged(T), null);
                }
                return _dateTimeValueChangedCommand;
            }
        }

        private void TimeValueChanged(object T)
        {
            RoutedPropertyChangedEventArgs<object> e = T as RoutedPropertyChangedEventArgs<object>;
            if (e == null)
                return;

            if (e.NewValue == null)
            {
                this.TimingTime = DateTime.Now;
            }

            DateTime newTime = (DateTime)e.NewValue;
            if (newTime == null)
                this.TimingTime = DateTime.Now;

            if (newTime.Equals(new DateTime(1, 1, 1)))
            {
                this.TimingTime = DateTime.Now;
            }
        }
    }
}
