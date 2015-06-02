using GalaSoft.MvvmLight;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nova.NovaWeb.Protocol;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Nova.NovaWeb.McGo.Platform.View;
using Nova.Xml.Files;
using System.IO;
using Nova.Globalization;
using System.Threading;
using GalaSoft.MvvmLight.Threading;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.Xml;
using Nova.NovaWeb.McGo.Platform.Model;
using Nova.NovaWeb.McGo.Platform.Service;
using System.Runtime.Serialization.Json;
using Nova.NovaWeb.McGo.Platform.Utilities;
using GalaSoft.MvvmLight.Messaging;
using System.Diagnostics;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class TerminalViewModel : ViewModelBase
    {
        private bool _isScreenShot;
        private bool? _enableIPCam;
        private string _placeAddress;
        private int _lEDWidth;
        private int _lEDHeight;
        private string _timeZoneId;
        private TerminalType _type;
        private int _groupId;
        private string _groupName;
        private string _displayName;
        private string _name;
        private string _mac;
        private bool _isEnableAlarm;
        private bool _isAlarm;
        private bool _isOnline;

        private bool _isGroupsExpanded = true;
        private bool _isSelected = false;
        private bool? _isChecked = false;

        private List<StatusData> _terminalStatusList;
        private StatusData _playingStatus;
        private StatusData _downloadingStatus;


        private TerminalModel _terminalModel;

        private RemoteScheduleInfo _playingScheduleInfo;
        private RemoteScheduleInfo _downloadingScheduleInfo;

        private ScheduleViewModel _editingSchedule;

        private RelayCommand _cancelReadbackCommand;


        public TerminalViewModel(TerminalModel terminalModel)
        {
            if (terminalModel == null)
            {
                throw new ArgumentNullException();
            }
            _terminalModel = terminalModel;
            ModelToViewModel(terminalModel);
        }

        public TerminalModel CurrentTerminal
        {
            get
            {
                return _terminalModel;
            }
            set
            {
                if (value == _terminalModel)
                    return;
                _terminalModel = value;
                RaisePropertyChanged("CurrentTerminal");
            }
        }

        /// <summary>
        /// 终端Mac
        /// </summary>
        public string Mac
        {
            get
            {
                return _mac;
            }
            set
            {
                if (value == _mac)
                    return;
                _mac = value;
                RaisePropertyChanged("Mac");
            }
        }

        /// <summary>
        /// 终端名称
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == _name)
                    return;
                _name = value;
                RaisePropertyChanged("Name");

            }
        }

        /// <summary>
        /// 终端显示名称
        /// </summary>
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                if (value == _displayName)
                    return;
                _displayName = value;
                RaisePropertyChanged("DisplayName");
            }
        }

        /// <summary>
        /// 终端所在分组名
        /// </summary>
        public string GroupName
        {
            get
            {
                return _groupName;
            }
            set
            {
                if (value == _groupName)
                    return;
                _groupName = value;
                RaisePropertyChanged("GroupName");
            }
        }

        /// <summary>
        /// 终端所在分组Id
        /// </summary>
        public int GroupId
        {
            get
            {
                return _groupId;
            }
            set
            {
                if (value == _groupId)
                    return;
                _groupId = value;
                RaisePropertyChanged("GroupId");
            }
        }

        /// <summary>
        /// 终端在线状态
        /// </summary>
        /// 

        public bool IsOnline
        {
            get { return _isOnline; }
            set
            {
                if (value == _isOnline)
                {
                    return;
                }
                _isOnline = value;
                RaisePropertyChanged("IsOnline");
            }
        }

        public bool IsEnableAlarm
        {
            get
            {
                return _isEnableAlarm;
            }
            set
            {
                if (value == _isEnableAlarm)
                    return;
                _isEnableAlarm = value;
                RaisePropertyChanged("IsEnableAlarm");
            }
        }

        public bool IsAlarm
        {
            get
            {
                return _isAlarm;
            }
            set
            {
                if (_isAlarm == value)
                {
                    return;
                }
                _isAlarm = value;
                RaisePropertyChanged("IsAlarm");
            }
        }

        /// <summary>
        /// 终端类型
        /// </summary>
        public TerminalType Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (value == _type)
                    return;
                _type = value;
                RaisePropertyChanged("Type");
            }
        }

        public string TimeZoneId
        {
            get
            {
                return _timeZoneId;
            }
            set
            {
                if (value == _timeZoneId)
                    return;
                _timeZoneId = value;
                RaisePropertyChanged("TimeZoneId");
            }
        }

        public int LEDHeight
        {
            get
            {
                return _lEDHeight;
            }
            set
            {
                if (value == _lEDHeight)
                    return;
                _lEDHeight = value;
                RaisePropertyChanged("LEDHeight");
                RaisePropertyChanged("LEDResolution");
            }
        }

        public int LEDWidth
        {
            get
            {
                return _lEDWidth;
            }
            set
            {
                if (value == _lEDWidth)
                    return;
                _lEDWidth = value;
                RaisePropertyChanged("LEDWidth");
                RaisePropertyChanged("LEDResolution");
            }
        }

        public string LEDResolution
        {
            get { return string.Format("{0}*{1}", _lEDWidth, _lEDHeight); }
        }

        public string PlaceAddress
        {
            get
            {
                return _placeAddress;
            }
            set
            {
                if (value == _placeAddress)
                    return;
                _placeAddress = value;
                RaisePropertyChanged("PlaceAddress");
            }
        }


        public RemoteScheduleInfo DownloadingScheduleInfo
        {
            get { return _downloadingScheduleInfo; }
            set
            {
                if (value == _downloadingScheduleInfo)
                {
                    return;
                }
                _downloadingScheduleInfo = value;
                RaisePropertyChanged("DownloadingScheduleInfo");
            }
        }

        
        public RemoteScheduleInfo PlayingScheduleInfo
        {
            get
            {
                return _playingScheduleInfo;
            }
            set
            {
                if (_playingScheduleInfo == value)
                    return;
                _playingScheduleInfo = value;
                RaisePropertyChanged("PlayingScheduleInfo");
            }
        }



        public ScheduleViewModel EditingSchedule
        {
            get
            {
                return _editingSchedule;
            }
            set
            {
                if (_editingSchedule == value)
                    return;
                _editingSchedule = value;
                RaisePropertyChanged("EditingSchedule");
            }
        }

        public bool? EnableIPCam
        {
            get
            {
                return _enableIPCam;
            }
            set
            {
                if (value == _enableIPCam)
                    return;
                _enableIPCam = value;
                RaisePropertyChanged("EnableIPCam");
            }
        }

        public bool IsScreenShot
        {
            get
            {
                return _isScreenShot;
            }
            set
            {
                if (value == _isScreenShot)
                    return;
                _isScreenShot = value;
                RaisePropertyChanged("IsScreenShot");
            }
        }


        public bool IsExpanded
        {
            get { return _isGroupsExpanded; }
            set
            {
                if (value == _isGroupsExpanded)
                    return;
                _isGroupsExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
                if (_isSelected)
                RaisePropertyChanged("IsSelected");
            }
        }
        private bool _checkBoxChecked = false;
        public bool CheckBoxChecked
        {
            get { return _checkBoxChecked; }
            set
            {
                if (value.Equals(_checkBoxChecked)) return;
                _checkBoxChecked = value;
                RaisePropertyChanged("CheckBoxChecked");
            }
        }
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value.Equals(_isChecked)) return;
                _isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        private void ModelToViewModel(TerminalModel terminalModel)
        {
            this.DisplayName = terminalModel.Name;
            this.TimeZoneId = terminalModel.TimeZoneId;
    
            this.GroupId = terminalModel.GroupId;
            this.GroupName = terminalModel.GroupName;
            this.IsAlarm = terminalModel.IsAlarm;
            this.IsEnableAlarm = terminalModel.IsEnableAlarm;
            this.LEDHeight = terminalModel.LEDHeight;
            this.LEDWidth = terminalModel.LEDWidth;
            this.Mac = terminalModel.Mac;
            this.Name = terminalModel.Name;
            this.PlaceAddress = terminalModel.PlaceAddress;

            this._terminalStatusList = terminalModel.StatusList;

            this.Type = terminalModel.GetTerminalType();
            this.EnableIPCam = terminalModel.GetIPCamEnable();
            this.IsOnline = terminalModel.GetTerminalOnlineState();
            this.IsScreenShot = terminalModel.GetScreenShotStatus();

            this.PlayingScheduleInfo = GetScheduleInfo(InquireStatusTypes.lastPubSucProg);
            this.DownloadingScheduleInfo = GetScheduleInfo(InquireStatusTypes.downloadProg);
            //this._playingSchedule = new ScheduleViewModel(terminalModel.PlayingSchedule, true);
            //this._downloadingSchedule = new ScheduleViewModel(terminalModel.DownloadingSchedule, true);
        }





        #region Readback Function


        private bool _isScheduleDownloading;
        public bool IsScheduleDownloading
        {
            get { return _isScheduleDownloading; }
            set
            {
                if (_isScheduleDownloading == value)
                {
                    return;
                }
                _isScheduleDownloading = value;
                RaisePropertyChanged("IsScheduleDownloading");
            }
        }


        private RelayCommand _viewPlayProgramCommand;
        public ICommand ViewPlayProgramCommand
        {
            get
            {
                if (_viewPlayProgramCommand == null)
                {
                    _viewPlayProgramCommand = new RelayCommand(
                        () =>
                        {
                            ViewPlayingScheduleInfo();
                            Debug.WriteLine("mouse up");
                        },
                        () =>
                        {
                            return true;
                        });
                }
                return _viewPlayProgramCommand;
            }
        }

        public void UpdateTerminalStatus(TerminalModel terminalModel)
        {
            _terminalModel = terminalModel;
            ModelToViewModel(_terminalModel);
        }

        private RemoteScheduleInfo GetScheduleInfo(InquireStatusTypes statusType)
        {
            StatusData statusData = _terminalStatusList.FirstOrDefault(T => T.StatusType == statusType);
            if (statusData == null)
            {
                App.Log(new Exception("获取播放方案信息失败。(GetScheduleInfo)"));
                return null;
            }


           return new RemoteScheduleInfo(statusData);
        }

        private void ViewPlayingScheduleInfo()
        {
            if (_terminalStatusList == null)
            {
                App.Log(new Exception("获取终端状态列表失败。(ViewPlayingScheduleInfo)"));
                return;
            }

            GetScheduleInfo(InquireStatusTypes.lastPubSucProg);

            if (_playingScheduleInfo.SupportReadback)
            {
                var modalDialogService = AppEnvionment.Default.Get<IModalDialogService>();
                if (modalDialogService == null)
                    throw new NotImplementedException();

                var dialog = AppEnvionment.Default.Get<IModalWindow>(typeof(ReadbackScheduleView).Name);

                var dialogViewModel = new ReadbackScheduleViewModel(this);

                Messenger.Default.Register<string>(this, "CloseReadbackView", message =>
                {
                    //DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    //{
                    //    dialog.Close();
                    //}));
                });

                modalDialogService.ShowDialog(dialog, dialogViewModel, viewModel =>
                {
                    //to do something 
                });
                
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_NoSupportSchedule", "不支持此播放方案"));
            }
        }

        public RelayCommand CancelReadbackCommand
        {
            get
            {
                if (_cancelReadbackCommand == null)
                {
                    _cancelReadbackCommand = new RelayCommand(() =>
                    {
                        CancelReadback();
                    },
                    null);
                }
                return _cancelReadbackCommand;
            }
        }

        private void CancelReadback()
        {
            IsScheduleDownloading = false;
            CancelScheduleDownload();
        }


        private string RemoveMD5InFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            string[] nameSplits = fileName.Split(new Char[] { '.' });
            if (nameSplits.Length == 2)
            {
                return fileName;
            }
            else if (nameSplits.Length == 3)
            {
                return nameSplits[0] + "." + nameSplits[2];
            }
            else
            {
                return nameSplits[0] + "." + nameSplits[nameSplits.Length - 1];
            }
        }

        private string GetDownloadServerPath(string fileServerPath)
        {
            var transimissionService = AppEnvionment.Default.Get<ITransmissionInfoService>();
            var transimissionInfo = transimissionService.GetPlatformDataTransInfo();
            return Path.Combine(transimissionInfo.DestinationAddress, "PlayProgram", fileServerPath);
        }

        private FileSetDownLoader fileDownLoader;
        //private void DownloadScheduleFileFromFtpServer(ScheduleViewModel schedule, string downloadAddress, string ftpAddress, string ftpUserName, string ftpPassword)
        //{
        //    string str = Path.GetDirectoryName(downloadAddress).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(":", ":/");
        //    fileDownLoader = new FileSetDownLoader(new Uri(str), ftpUserName, ftpPassword, Encoding.UTF8);

        //    var appData = AppEnvionment.Default.Get<IPlatformService>();
        //    if (appData == null)
        //        return;

        //    fileDownLoader.IsEnableSsl = appData.GetAppData().IsEnableFTPS;

        //    fileDownLoader.FileSetDownLoadCompleteEvent += fileDownLoader_FileSetDownLoadCompleteEvent;

        //    List<string> downloadFilePathList = new List<string>();
        //    if (this.Type == TerminalType.PC)
        //    {
        //        downloadFilePathList.Add(Path.GetFileName(schedule.FilePath));
        //    }
        //    else
        //    {
        //        downloadFilePathList.Add(Path.GetFileName(schedule.FilePath));
        //        downloadFilePathList.Add(Path.GetFileName(schedule.ConvertedFilePath));
        //    }

        //    fileDownLoader.DownLoadFileSet(downloadFilePathList, ".temp", true, true, PlatformConfig.ServerPlayDirectoryPath, null);

        //}

        public delegate void ScheduleDownloadComplete(FileSetTransmiteCompleteEventArgs e);

        public event ScheduleDownloadComplete ScheduleDownloadCompleteEvent;

        public void CancelScheduleDownload()
        {
            if (fileDownLoader != null)
            {
                fileDownLoader.CancelTransmite();

            }
        }

        //void fileDownLoader_FileSetDownLoadCompleteEvent(object sender, FileSetTransmiteCompleteEventArgs e)
        //{
        //    switch (e.TransmiteRes)
        //    {
        //        case FileSetTransmiteRes.Cancelled:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_Cancelled", "下载被取消"));

        //            }));
        //            break;
        //        case FileSetTransmiteRes.ConnectToServerFailed:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_ConnectToServerFailed", "连接服务器失败"));

        //            }));
        //            break;
        //        case FileSetTransmiteRes.FileSizeConflict:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_FileSizeConflict", "文件大小冲突"));

        //            }));
        //            break;
        //        case FileSetTransmiteRes.NotAllFileExistInServer:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_NotAllFileExistInServer", "不是所有的文件都存在于服务器"));

        //            }));
        //            break;
        //        case FileSetTransmiteRes.OK:
        //            CopyScheduleFileToCloudAndPluto();
        //            _editingSchedule.ParaseSchedule();
        //            //GetMediaFiles(_playingSchedule);
        //            break;
        //        case FileSetTransmiteRes.PathNotExistInServer:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_PathNotExistInServer", "下载路径不存在于服务器"));

        //            }));
        //            break;
        //        case FileSetTransmiteRes.WrongUserNameOrPwd:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_WrongUserNameOrPwd", "用户名或密码错误"));

        //            }));
        //            break;
        //        case FileSetTransmiteRes.UnknowErr:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_UnknowErr", "下载过程中出现未知错误"));

        //            }));
        //            break;
        //        case FileSetTransmiteRes.InsufficientHarddiskSpaceLocal:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_InsufficientHarddiskSpaceLocal", "本地磁盘空间不足"));

        //            }));
        //            break;
        //        case FileSetTransmiteRes.InsufficientHarddiskSpaceServer:
        //            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
        //            {
        //                Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_InsufficientHarddiskSpaceServer", "服务器磁盘空间不足"));

        //            }));
        //            fileDownLoader.CancelTransmite();
        //            break;
        //    }

        //    if (ScheduleDownloadCompleteEvent != null)
        //    {
        //        ScheduleDownloadCompleteEvent(e);
        //    }


        //    IsScheduleDownloading = false;
        //}

        //private void CopyScheduleFileToCloudAndPluto()
        //{
        //    string str1 = _editingSchedule.FilePath;


        //    _editingSchedule.FilePath = Path.Combine(
        //        Path.GetDirectoryName(_editingSchedule.FilePath),
        //        Path.GetFileName(_editingSchedule.FilePath));


        //    File.Move(str1, _editingSchedule.FilePath);



        //    if (File.Exists(_editingSchedule.ConvertedFilePath))
        //    {
        //        string str2 = _editingSchedule.ConvertedFilePath;

        //        _editingSchedule.ConvertedFilePath = Path.Combine(
        //      Path.GetDirectoryName(_editingSchedule.ConvertedFilePath),
        //      Path.GetFileName(_editingSchedule.ConvertedFilePath));

        //        File.Move(str2, _editingSchedule.ConvertedFilePath);

        //        string tempFilePath = Path.ChangeExtension(_editingSchedule.FilePath, "plymBak");
        //        string cFilePath = Path.ChangeExtension(_editingSchedule.FilePath, "plpym");
        //        string dFilePath = Path.ChangeExtension(_editingSchedule.ConvertedFilePath, "plym");
        //        File.Move(_editingSchedule.FilePath, tempFilePath);
        //        File.Move(_editingSchedule.ConvertedFilePath, dFilePath);
        //        File.Move(tempFilePath, cFilePath);
        //        _editingSchedule.FilePath = dFilePath;
        //        _editingSchedule.ConvertedFilePath = cFilePath;
        //        File.Copy(_editingSchedule.FilePath, Path.Combine(PlatformConfig.ServerCloudDirectoryPath, Path.GetFileName(_editingSchedule.FilePath)), true);
        //        File.Copy(_editingSchedule.FilePath, Path.Combine(PlatformConfig.ServerPlutoDirectoryPath, Path.GetFileName(_editingSchedule.FilePath)), true);
        //    }
        //}


        //private void GetMediaFiles(ScheduleViewModel schedule)
        //{
        //    string scheduleParsePath;

        //    scheduleParsePath = schedule.FilePath;

        //    bool bSuccess;
        //    PlayProgramXml programXml = new PlayProgramXml(scheduleParsePath, XmlFile.XmlFileFlag.XmlExisting, out bSuccess);
        //    if (!bSuccess)
        //    {
        //        return;
        //    }

        //    List<string> mediaFileNameList;
        //    programXml.GetAllFileListInPlaylist(out mediaFileNameList);
        //    if (mediaFileNameList == null && mediaFileNameList.Count == 0)
        //    {
        //        return;
        //    }
        //    foreach (var item in mediaFileNameList)
        //    {
        //        DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
        //         {
        //             MediaViewModel media = new MediaViewModel();
        //             media.Name = Path.GetFileName(item);
        //             EditingSchedule.MediaList.Add(media);
        //         }));

        //    }
        //}

        #endregion
    }

    public class RemoteScheduleInfo : ViewModelBase
    {
        private string _displayScheduleName;
        private string _originalScheduleName = string.Empty;
        private string _optimizedScheduleName = string.Empty;
        private string _contrastFileName = string.Empty;
        private bool _supportReadback = true;
        private ScheduleType _scheduleType;

        public RemoteScheduleInfo(StatusData status)
        {
            if (status == null)
            {
                App.Log(new Exception("初始化播放方案信息失败。(RemoteScheduleInfo)"));
                return;
            }

            if (status.StatusType != InquireStatusTypes.downloadProg && status.StatusType != InquireStatusTypes.lastPubSucProg)
            {
                App.Log(new Exception("初始化播放方案信息时，状态类型错误。(RemoteScheduleInfo)"));
                return;
            }

            if (string.IsNullOrEmpty(status.Info))
                return;

            if (status.Info.Split(new char[]{'+'})[0] == "-1")
            {
                if (status.Info.Split((new char[] { '+' })).Length != 2)
                    return;
                _originalScheduleName = status.Info.Split(new char[] { '+' })[1];
                if(string.IsNullOrEmpty(_originalScheduleName))
                    return;
                if (Path.GetExtension(_originalScheduleName) == ".plym")
                {
                    _scheduleType = ScheduleType.Common;
                }
                else
                {
                    _scheduleType = ScheduleType.PC;
                }
                _supportReadback = false;
            }
            else
            {
                if (status.Info.Split((new char[] { '+' })).Length != 2)
                    return;
                _originalScheduleName = status.Info.Split(new char[] { '+' })[1];
                if (string.IsNullOrEmpty(_originalScheduleName))
                    return;
                if(string.IsNullOrEmpty(status.Status))
                {
                    return;
                }

                Cmd cmd;
                using (var mStream = new MemoryStream(Encoding.UTF8.GetBytes(status.Status)))
                {
                    var js = new DataContractJsonSerializer(typeof(Cmd));
                    cmd = js.ReadObject(mStream) as Cmd;
                }

                if (cmd == null || string.IsNullOrEmpty(cmd.Para))
                {
                    App.Log(new Exception("初始化播放方案信息时，协议内容错误。(RemoteScheduleInfo)"));
                    return;
                }

                string[] splitParas = cmd.Para.Split(new char[] { '+' });
                if (splitParas.Length != 5)
                {
                    App.Log(new Exception(string.Format("初始化播放方案信息时，协议内容错误。({0})(RemoteScheduleInfo)", cmd.Para)));
                    return;
                }

                _originalScheduleName = splitParas[0];

                if (Path.GetExtension(_originalScheduleName) == ".plym")
                {
                    _scheduleType = ScheduleType.Common;
                }
                else
                {
                    _scheduleType = ScheduleType.PC;
                }

                if (string.IsNullOrEmpty(splitParas[1]))
                    return;

                _optimizedScheduleName = splitParas[1].Split(new Char[] { '*' })[0];
                _contrastFileName = splitParas[1].Split(new Char[] { '*' })[1];
            }
        }

       
        public string OriginalScheduleName
        {
            get
            {
                return _originalScheduleName;
            }
            set
            {
                if (value == _originalScheduleName)
                    return;
                _originalScheduleName = value;
                RaisePropertyChanged("OriginalScheduleName");
                RaisePropertyChanged("DisplayScheduleName");
            }
        }
        public string DisplayScheduleName
        {
            get
            {
                return StringHelper.TrimMD5String(_originalScheduleName);
            }
        }
        public string OptimizedScheduleName
        {
            get
            {
                return _optimizedScheduleName;
            }
            set
            {
                if (value == _optimizedScheduleName)
                    return;
                _optimizedScheduleName = value;
                RaisePropertyChanged("OptimizedScheduleName");
            }
        }

        public string ContrastFileName
        {
            get
            {
                return _contrastFileName;
            }
            set
            {
                if (value == _contrastFileName)
                    return;
                _contrastFileName = value;
                RaisePropertyChanged("ContrastFileName");
            }
        }

        public bool SupportReadback
        {
            get
            {
                return _supportReadback;
            }
            set
            {
                if (value == _supportReadback)
                    return;
                _supportReadback = value;
                RaisePropertyChanged("SupportReadback");
            }
        }

        public ScheduleType Type
        {
            get
            {
                return _scheduleType;
            }
            set
            {
                if (value == _scheduleType)
                    return;
                _scheduleType = value;
                RaisePropertyChanged("Type");
            }
        }
    }
}
