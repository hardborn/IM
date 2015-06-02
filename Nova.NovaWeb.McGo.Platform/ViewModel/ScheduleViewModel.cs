using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Nova.Globalization;
using Nova.MediaItem;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.Utilities;
using Nova.NovaWeb.McGo.Platform.View;
using Nova.Xml;
using Nova.Xml.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class ScheduleViewModel : ViewModelBase
    {
        private ScheduleSourceType _sourceType;
        private string _downloadedConvertedFilePath;
        private string _downloadedFilePath;
        private bool _supportReadback;
        private string _downloadPathInServer;
        private string _mdcorFilePath = string.Empty;
        private SchedulePublishPathInfo _publishPathInfo;

        private ScheduleModel _scheduleModel;
        private long _mediaSize;
        private bool _fromTheServer;
        private bool _isGroupsExpanded;
        private ScheduleType _type;

        public FrmPlayProgramEditor fEditor = null;

        //public ScheduleViewModel()
        //{
        //    _schedule = new ScheduleModel();
        //    _publishPathInfo = new SchedulePublishPathInfo();
        //}

        //public ScheduleViewModel(ScheduleModel schedule)
        //    : this(schedule, ScheduleSourceType.LocalSource)
        //{
        //}


        public ScheduleViewModel(ScheduleModel scheduleModel, RemoteScheduleInfo remoteinfo = null)
        {
            if (scheduleModel == null)
                throw new ArgumentNullException("schedule");

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

            _scheduleModel = scheduleModel;
            RemoteInfo = remoteinfo;
            ModelToViewModel(scheduleModel);
            InitializeSchedulePublishPathInfo();
        }

        public override void Cleanup()
        {
            // Clean up if needed
            base.Cleanup();
            Messenger.Default.Unregister(this);
        }

        //public ScheduleViewModel(ScheduleModel schedule, bool fromTheServer)
        //{
        //    if (schedule == null)
        //    {
        //        throw new ArgumentNullException();
        //    }
        //    _scheduleModel = schedule;

        //    _fromTheServer = fromTheServer;

        //    ModelToViewModel(_scheduleModel);

        //    if (_fromTheServer)
        //    {
        //        if (!string.IsNullOrEmpty(_filePath))
        //        {
        //            _filePath = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, Path.GetFileName(_filePath));
        //            _downloadedFilePath = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, Path.GetFileName(_filePath));
        //        }
        //        if (!string.IsNullOrEmpty(_convertedFilePath))
        //        {
        //            _convertedFilePath = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, Path.GetFileName(_convertedFilePath));
        //            _downloadedConvertedFilePath = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, Path.GetFileName(_convertedFilePath));
        //        }
        //        if (!string.IsNullOrEmpty(_mdcorFilePath))
        //            _mdcorFilePath = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, Path.GetFileName(_convertedFilePath));
        //    }

        //    SetPublishPathInfo(_filePath, _convertedFilePath, _mdcorFilePath, _sourceType);
        //}
        private void ModelToViewModel(ScheduleModel model)
        {
            Name = model.Name;
            FilePath = model.FilePath;
            ConvertedFilePath = model.ConvertedFilePath;
            MdcorFilePath = model.MdcorFilePath;
            ScreenHeight = model.ScreenHeight;
            ScreenWidth = model.ScreenWidth;
            ScreenX = model.ScreenX;
            ScreenY = model.ScreenY;
            Type = model.Type;
            LastEditTime = model.LastEditTime;
            SupportReadback = model.SupportReadback;
            DownloadPathInServer = model.DownloadPathInServer;
            SourceType = model.SourceType;

            MediaSize = model.GetAllMediaSize();

            var mediaList = model.GetMediaList();
            if (mediaList == null)
                MediaList = new ObservableCollection<MediaViewModel>();
            else
            {
                var mediaViewModelList = mediaList.Select(m => new MediaViewModel(m));
                if (mediaViewModelList == null)
                    MediaList = new ObservableCollection<MediaViewModel>();
                else
                    MediaList = new ObservableCollection<MediaViewModel>(mediaViewModelList);
            }
        }

        private void InitializeSchedulePublishPathInfo()
        {
            FileExit();
            _publishPathInfo = new SchedulePublishPathInfo();
            if (_sourceType == ScheduleSourceType.LocalSource)
            {
                if (_type == ScheduleType.Common)
                {
                    if (!Directory.Exists(PlatformConfig.LocalPlutoPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.LocalPlutoPublicationDirectoryPath);
                    }
                    if (!Directory.Exists(PlatformConfig.LocalCloudPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.LocalCloudPublicationDirectoryPath);
                    }
                    string plutoPublicationFilePath = Path.Combine(PlatformConfig.LocalPlutoPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".plym");
                    string cloudPublicationFilePath = Path.Combine(PlatformConfig.LocalCloudPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".plym");
                    File.Copy(FilePath, plutoPublicationFilePath, true);
                    File.Copy(FilePath, cloudPublicationFilePath, true);
                    _publishPathInfo.PlutoFilePath = plutoPublicationFilePath;
                    _publishPathInfo.CloudFilePath = cloudPublicationFilePath;
                }
                else
                {
                    string cloudPublicationFilePath = Path.Combine(PlatformConfig.LocalCloudPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".cplym");
                    if (!Directory.Exists(Path.GetDirectoryName(cloudPublicationFilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cloudPublicationFilePath));
                    }
                    File.Copy(FilePath, cloudPublicationFilePath, true);
                    _publishPathInfo.CloudFilePath = cloudPublicationFilePath;
                }
            }
            else
            {
                if (_type == ScheduleType.Common)
                {
                    if (!Directory.Exists(PlatformConfig.RemotePlutoPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.RemotePlutoPublicationDirectoryPath);
                    }
                    if (!Directory.Exists(PlatformConfig.RemoteCloudPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.RemoteCloudPublicationDirectoryPath);
                    }
                    
                    string plutoPublicationFilePath = Path.Combine(PlatformConfig.RemotePlutoPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".plym");
                    string cloudPublicationFilePath = Path.Combine(PlatformConfig.RemoteCloudPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".plym");
                    
                    File.Copy(FilePath, plutoPublicationFilePath, true);
                    File.Copy(FilePath, cloudPublicationFilePath, true);
                    _publishPathInfo.PlutoFilePath = plutoPublicationFilePath;
                    _publishPathInfo.CloudFilePath = cloudPublicationFilePath;
                }
                else
                {
                    if (!Directory.Exists(PlatformConfig.RemoteCloudPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.RemoteCloudPublicationDirectoryPath);
                    }
                    string cloudPublicationFilePath = Path.Combine(PlatformConfig.RemoteCloudPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".cplym");
                    File.Copy(FilePath, cloudPublicationFilePath, true);
                    _publishPathInfo.CloudFilePath = cloudPublicationFilePath;
                }
            }
        }
        //private void SetPublishPathInfo(string filePath, string convertedFilePath, string mdcorFilePath, ScheduleSourceType sourceType)
        //{
        //    if (string.IsNullOrEmpty(filePath))
        //        return ;

        //    string fileDirectoryPath = Path.GetDirectoryName(filePath);

        //    _publishPathInfo = new SchedulePublishPathInfo();
        //    if (sourceType == ScheduleSourceType.RemoteSource)
        //    {
        //        _publishPathInfo.CloudFilePath = Path.Combine(fileDirectoryPath, Path.GetFileName(filePath));
        //        _publishPathInfo.PlutoFilePath = Path.Combine(fileDirectoryPath, Path.GetFileName(filePath));

        //        if (!string.IsNullOrEmpty(convertedFilePath))
        //            _publishPathInfo.PlutoConverterFilePath = Path.Combine(fileDirectoryPath, Path.GetFileName(convertedFilePath));
        //        else
        //            _publishPathInfo.PlutoConverterFilePath = Path.Combine(fileDirectoryPath, Path.ChangeExtension(Path.GetFileName(filePath), "plpym"));

        //        if (!string.IsNullOrEmpty(mdcorFilePath))
        //            _publishPathInfo.PlutoMdcorFilePath = Path.Combine(fileDirectoryPath, Path.GetFileName(mdcorFilePath));
        //        else
        //            _publishPathInfo.PlutoMdcorFilePath = Path.Combine(fileDirectoryPath, Path.ChangeExtension(Path.GetFileName(filePath), "mdcor"));
        //    }
        //    else
        //    {
        //        _publishPathInfo.CloudFilePath = Path.Combine(PlatformConfig.PlayCloudDirectoryPath, Path.GetFileName(filePath));
        //        _publishPathInfo.PlutoFilePath = Path.Combine(PlatformConfig.PlayPlutoDirectoryPath, Path.GetFileName(filePath));

        //        if (!string.IsNullOrEmpty(convertedFilePath))
        //            _publishPathInfo.PlutoConverterFilePath = Path.Combine(PlatformConfig.PlayPlutoDirectoryPath, Path.GetFileName(convertedFilePath));
        //        else
        //            _publishPathInfo.PlutoConverterFilePath = Path.Combine(PlatformConfig.PlayPlutoDirectoryPath, Path.ChangeExtension(Path.GetFileName(filePath), "plpym"));

        //        if (!string.IsNullOrEmpty(mdcorFilePath))
        //            _publishPathInfo.PlutoMdcorFilePath = Path.Combine(PlatformConfig.PlayPlutoDirectoryPath, Path.GetFileName(mdcorFilePath));
        //        else
        //            _publishPathInfo.PlutoMdcorFilePath = Path.Combine(PlatformConfig.PlayPlutoDirectoryPath, Path.ChangeExtension(Path.GetFileName(filePath), "mdcor"));
        //    }
        //}

        private void PreparingPublicationFiles()
        {
            FileExit();
            if (_sourceType == ScheduleSourceType.LocalSource)
            {
                if (_type == ScheduleType.Common)
                {
                    if (!Directory.Exists(PlatformConfig.LocalPlutoPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.LocalPlutoPublicationDirectoryPath);
                    }
                    if (!Directory.Exists(PlatformConfig.LocalCloudPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.LocalCloudPublicationDirectoryPath);
                    }

                    string plutoPublicationFilePath = Path.Combine(PlatformConfig.LocalPlutoPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".plym");
                    string cloudPublicationFilePath = Path.Combine(PlatformConfig.LocalCloudPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".plym");
                    File.Copy(FilePath, plutoPublicationFilePath, true);
                    File.Copy(FilePath, cloudPublicationFilePath, true);
                    _publishPathInfo.PlutoFilePath = plutoPublicationFilePath;
                    _publishPathInfo.CloudFilePath = cloudPublicationFilePath;
                }
                else
                {
                    string cloudPublicationFilePath = Path.Combine(PlatformConfig.LocalCloudPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".cplym");
                    if (!Directory.Exists(Path.GetDirectoryName(cloudPublicationFilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cloudPublicationFilePath));
                    }
                    File.Copy(FilePath, cloudPublicationFilePath, true);
                    _publishPathInfo.CloudFilePath = cloudPublicationFilePath;
                }
            }
            else
            {
                if (_type == ScheduleType.Common)
                {
                    if (!Directory.Exists(PlatformConfig.RemotePlutoPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.RemotePlutoPublicationDirectoryPath);
                    }
                    if (!Directory.Exists(PlatformConfig.RemoteCloudPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.RemoteCloudPublicationDirectoryPath);
                    }

                    string plutoPublicationFilePath = Path.Combine(PlatformConfig.RemotePlutoPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".plym");
                    string cloudPublicationFilePath = Path.Combine(PlatformConfig.RemoteCloudPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".plym");
                    File.Copy(FilePath, plutoPublicationFilePath, true);
                    File.Copy(FilePath, cloudPublicationFilePath, true);
                    _publishPathInfo.PlutoFilePath = plutoPublicationFilePath;
                    _publishPathInfo.CloudFilePath = cloudPublicationFilePath;
                }
                else
                {
                    if (!Directory.Exists(PlatformConfig.RemoteCloudPublicationDirectoryPath))
                    {
                        Directory.CreateDirectory(PlatformConfig.RemoteCloudPublicationDirectoryPath);
                    }
                    string cloudPublicationFilePath = Path.Combine(PlatformConfig.RemoteCloudPublicationDirectoryPath, Path.GetFileNameWithoutExtension(FilePath) + ".cplym");
                    File.Copy(FilePath, cloudPublicationFilePath, true);
                    _publishPathInfo.CloudFilePath = cloudPublicationFilePath;
                }
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == _name)
                {
                    return;
                }
                _name = value;
                RaisePropertyChanged("Name");
                RaisePropertyChanged("DisplayScheduleName");
            }
        }

        private string _filePath = string.Empty;
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                if (value == _filePath)
                {
                    return;
                }
                _filePath = value;
                RaisePropertyChanged("FilePath");
            }
        }

        private string _convertedFilePath = string.Empty;

        public string ConvertedFilePath
        {
            get
            {
                return _convertedFilePath;
            }
            set
            {
                if (value == _convertedFilePath)
                {
                    return;
                }
                _convertedFilePath = value;
                RaisePropertyChanged("ConvertedFilePath");
            }
        }

        public string MdcorFilePath
        {
            get
            {
                return _mdcorFilePath;
            }
            set
            {
                if (value == _mdcorFilePath)
                {
                    return;
                }
                _mdcorFilePath = value;
                RaisePropertyChanged("MdcorFilePath");
            }
        }

        public string DownloadedFilePath
        {
            get
            {
                return _downloadedFilePath;
            }
            set
            {
                if (value == _downloadedFilePath)
                {
                    return;
                }
                _downloadedFilePath = value;
                RaisePropertyChanged("DownloadedFilePath");
            }
        }

        public string DownloadedConvertedFilePath
        {
            get
            {
                return _downloadedConvertedFilePath;
            }
            set
            {
                if (_downloadedConvertedFilePath == value)
                    return;

                _downloadedConvertedFilePath = value;
                RaisePropertyChanged("DownloadedConvertedFilePath");
            }
        }


        public RemoteScheduleInfo RemoteInfo { get; private set; }

        public ScheduleType Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (value == _type)
                {
                    return;
                }
                _type = value;

                RaisePropertyChanged("Type");
            }
        }

        public ScheduleSourceType SourceType
        {
            get
            {
                return _sourceType;
            }
            set
            {
                if (value == _sourceType)
                    return;
                _sourceType = value;
                RaisePropertyChanged("SourceType");
            }
        }

        public string DisplayScheduleName
        {
            get
            {
                return StringHelper.TrimMD5String(_name);
            }
        }

        public string GuideName
        {
            get
            {
                string info = "";
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_PublishWizard", out info);
                return info + "-" + DisplayScheduleName;
            }
        }

        private DateTime _lastEditTime;
        public DateTime LastEditTime
        {
            get
            {
                return _lastEditTime;
            }
            set
            {
                if (value == _lastEditTime)
                {
                    return;
                }
                _lastEditTime = value;
                RaisePropertyChanged("LastEditTime");
            }
        }

        private int _screenWidth;
        public int ScreenWidth
        {
            get
            {
                return _screenWidth;
            }
            set
            {
                if (value == _screenWidth)
                {
                    return;
                }
                _screenWidth = value;
                RaisePropertyChanged("ScreenWidth");
                RaisePropertyChanged("ScreenSize");
            }
        }

        private int _screenHight;
        public int ScreenHeight
        {
            get
            {
                return _screenHight;
            }
            set
            {
                if (value == _screenHight)
                {
                    return;
                }
                _screenHight = value;
                RaisePropertyChanged("ScreenHeight");
                RaisePropertyChanged("ScreenSize");
            }
        }

        public string ScreenSize
        {
            get
            {
                return string.Format("{0}*{1}", _scheduleModel.ScreenWidth, _scheduleModel.ScreenHeight);
            }
        }


        private int _screenX;
        public int ScreenX
        {
            get
            {
                return _screenX;
            }
            set
            {
                if (_screenX == value)
                {
                    return;
                }
                _screenX = value;
                RaisePropertyChanged("ScreenX");
            }
        }

        private int _screenY;
        public int ScreenY
        {
            get
            {
                return _screenY;
            }
            set
            {
                if (_screenY == value)
                {
                    return;
                }
                _screenY = value;
                RaisePropertyChanged("ScreenY");
            }
        }

        public long MediaSize
        {
            get
            {
                return _mediaSize;
            }
            set
            {
                if (value == _mediaSize)
                    return;
                _mediaSize = value;
                RaisePropertyChanged("MediaSize");
            }
        }

        public SchedulePublishPathInfo PublishPathInfo
        {
            get
            {
                return _publishPathInfo;
            }
            set
            {
                if (_publishPathInfo == value)
                    return;
                _publishPathInfo = value;
                RaisePropertyChanged("PublishPathInfo");
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isGroupsExpanded;
            }
            set
            {
                _isGroupsExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        private ObservableCollection<MediaViewModel> _mediaViewModelList = new ObservableCollection<MediaViewModel>();
        public ObservableCollection<MediaViewModel> MediaList
        {
            get
            {
                return _mediaViewModelList;
            }
            set
            {
                if (_mediaViewModelList == value)
                {
                    return;
                }
                _mediaViewModelList = value;
                RaisePropertyChanged("MediaList");
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
                if (_supportReadback == value)
                    return;
                _supportReadback = value;
                RaisePropertyChanged("SupportReadback");
            }
        }

        public string DownloadPathInServer
        {
            get
            {
                return _downloadPathInServer;
            }
            set
            {
                if (_downloadPathInServer == value)
                    return;
                _downloadPathInServer = value;
                RaisePropertyChanged("DownloadPathInServer");
            }
        }
        ///// <summary>
        ///// 获取播放方案中的媒体列表
        ///// </summary>
        ///// <param name="filePath"></param>
        ////public void GetMediaFiles(string filePath)
        ////{
        ////    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
        ////    {
        ////        MediaList.Clear();
        ////    }));

        ////    bool bSuccess;
        ////    PlayProgramXml programXml = new PlayProgramXml(filePath, XmlFile.XmlFileFlag.XmlExisting, out bSuccess);
        ////    if (!bSuccess)
        ////    {
        ////        return;
        ////    }

        ////    List<string> mediaFileNameList;

        ////   // PlayProgramParser.GetAllMedia(filePath, out _mediaList);
        ////    programXml.GetAllFileListInPlaylist(out mediaFileNameList);
        ////    if (mediaFileNameList == null && mediaFileNameList.Count == 0)
        ////    {
        ////        return;
        ////    }
        ////    foreach (var item in mediaFileNameList)
        ////    {
        ////        DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
        ////        {
        ////            MediaViewModel media = new MediaViewModel();
        ////            media.Name = Path.GetFileName(item);
        ////            MediaList.Add(media);
        ////        }));
        ////    }
        ////}



        //public void ParaseSchedule()
        //{
        //    _scheduleModel = new ScheduleModel(FilePath, ConvertedFilePath);

        //    ModelToViewModel(_scheduleModel);

        //    //var mediaList = _schedule.GetMediaList();
        //    //if (mediaList == null)
        //    //    MediaList = new ObservableCollection<MediaViewModel>();
        //    //else
        //    //{
        //    //    var mediaViewModelList = mediaList.Select(m => new MediaViewModel(m));
        //    //    if (mediaViewModelList == null)
        //    //        MediaList = new ObservableCollection<MediaViewModel>();
        //    //    else
        //    //        MediaList = new ObservableCollection<MediaViewModel>(mediaViewModelList);
        //    //}
        //}



        private RelayCommand _publishScheduleCommand;
        public ICommand PublishScheduleCommand
        {
            get
            {
                if (_publishScheduleCommand == null)
                {
                    _publishScheduleCommand = new RelayCommand(
                        () => PublishSchedule(),
                        null
                        );
                }
                return _publishScheduleCommand;
            }
        }

        private void PublishSchedule()
        {
            if (this.Type == ScheduleType.Common && this.RemoteInfo != null && string.IsNullOrEmpty(this.RemoteInfo.OptimizedScheduleName))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_NoSupportAsynTerminal", "此播放方案不支持对异步终端进行发布！"),
                    string.Empty,
                    MessageBoxButton.OK);
            }
            if (!ValidationMediaList())
            {
                if (Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_MediaListEmpty", "播放方案中没有添加任何媒体，发布至终端将导致终端黑屏播放，是否继续发布？"),
                    string.Empty,
                    MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
            }
            PreparingPublicationFiles();
            WizardViewModel publishWizardViewModel = new WizardViewModel(this, null);
            PublishWizardDialog publishWizardDialog = new PublishWizardDialog();
            publishWizardViewModel.RequestClose += (s, i) =>
            {
                FileHelper.DeleteFiles(PlatformConfig.RemotePlutoPublicationDirectoryPath);
                publishWizardDialog.Close();
                Messenger.Default.Send<string>(string.Empty, "CloseReadbackView");
            };
            publishWizardDialog.Owner = App.Current.MainWindow;
            publishWizardDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            publishWizardDialog.DataContext = publishWizardViewModel;
            publishWizardDialog.ShowInTaskbar = false;
            publishWizardDialog.ShowDialog();
        }

        private RelayCommand _editScheduleCommand;
        public ICommand EditScheduleCommand
        {
            get
            {
                if (_editScheduleCommand == null)
                {
                    _editScheduleCommand = new RelayCommand(
                      () => EditSchedule(),
                        null
                        );
                }
                return _editScheduleCommand;
            }
        }

        public string TrimMD5String(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            string[] splitStr = str.Split(new Char[] { '.' });
            if (splitStr[splitStr.Length - 2] != null && splitStr[splitStr.Length - 2].Length == 32)
            {
                return str.Remove(str.LastIndexOf('.') - 33, 33);
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 改了工作目录，判断文件是否存在，若不存在就新建
        /// </summary>
        public void FileExit()
        {
            if(!Directory.Exists(PlatformConfig.EmergencyListDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.EmergencyListDirectoryPath);
            }
            if(!Directory.Exists(PlatformConfig.ImageMonitorDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.ImageMonitorDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.LogDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.LogDirectoryPath);
            }


            if (!Directory.Exists(PlatformConfig.LocalDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.LogDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.LocalPublicationDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.LocalPublicationDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.LocalPlutoPublicationDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.LocalPlutoPublicationDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.LocalCloudPublicationDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.LocalCloudPublicationDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.LocalScheduleDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.LocalScheduleDirectoryPath);
            }

            if (!Directory.Exists(PlatformConfig.RemoteDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.RemoteDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.RemotePublicationDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.RemotePublicationDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.RemotePlutoPublicationDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.RemotePlutoPublicationDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.RemoteCloudPublicationDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.RemoteCloudPublicationDirectoryPath);
            }
            if (!Directory.Exists(PlatformConfig.RemoteScheduleDirectoryPath))
            {
                Directory.CreateDirectory(PlatformConfig.RemoteScheduleDirectoryPath);
            }
        }


        public delegate void EditCompletedEventHandler();//(object sender, EventArgs e);
      
        public event EditCompletedEventHandler EditCompletedEvent;
        private void EditSchedule()
        {
            //CopyScheduleToWorkspace();

            //var  scheduleEditPath = FilePath;
            FileExit();

            var _viewModelLocator = ViewModelLocator.Instance;

            var _platformService = AppEnvionment.Default.Get<IPlatformService>();

            var terminals = from item in _viewModelLocator.TerminalCollection.TerminalList
                            select item.CurrentTerminal.CurrentTerminal;
            string editScheduleDirectory = string.Empty;
            string editScheduleFilePath = string.Empty;
            if (this._sourceType == ScheduleSourceType.RemoteSource)
            {
                editScheduleDirectory = PlatformConfig.RemoteScheduleDirectoryPath;

                if (string.IsNullOrEmpty(RemoteInfo.OptimizedScheduleName))
                {
                    if (Path.GetFileName(this.FilePath) == RemoteInfo.OriginalScheduleName)
                        editScheduleFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, RemoteInfo.OriginalScheduleName);
                    else
                        editScheduleFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, Path.GetFileName(this.FilePath));
                }
                else
                {
                    if (File.Exists(this.FilePath) && (Path.GetExtension(this.FilePath) == ".plym"))
                    {
                        editScheduleFilePath = this.FilePath;
                    }
                    else
                    {
                        editScheduleFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, RemoteInfo.OptimizedScheduleName);
                        editScheduleFilePath = Path.ChangeExtension(editScheduleFilePath, ".plym");
                        File.Copy(Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, RemoteInfo.OptimizedScheduleName), editScheduleFilePath, true);
                    }
                }
            }
            else
            {
                editScheduleDirectory = PlatformConfig.LocalScheduleDirectoryPath;
                editScheduleFilePath = Path.Combine(PlatformConfig.LocalScheduleDirectoryPath, FilePath);
            }

            try
            {
                fEditor = new FrmPlayProgramEditor(
                 editScheduleDirectory,
                  editScheduleFilePath,
                  PlatformConfig.LanguageResourcePath,
                  _platformService.GetPlatformLanguage(),
                  new System.Drawing.Size(ScreenWidth, ScreenHeight),
                  false,
                  terminals.ToList(),
                  Type == ScheduleType.PC ? false : true);
                    fEditor.PlayProgramSaveFinishedEvent += (s, i) =>
                    {
                        if (EditCompletedEvent != null)
                        {
                            EditCompletedEvent();
                        }
                        string newFilePath = fEditor.ProgramFilePath;
                        RefreshSchedule(newFilePath, _sourceType);
                        Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(string.Empty, "RefreshSchedule"), "RefreshSchedule");
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


        //private void CopyScheduleToWorkspace()
        //{
        //    //if(this._type == ScheduleType.PC)
        //    //{
        //    //    string sourceFileName = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, remoteScheduleInfo.OriginalScheduleName);
        //    //    string destFileName = Path.Combine(PlatformConfig.ServerCloudDirectoryPath, remoteScheduleInfo.OriginalScheduleName);
        //    //    File.Copy(sourceFileName, destFileName, true);
        //    //}

        //    // if (remoteScheduleInfo.Type == NovaWeb.Common.ScheduleType.PC)
        //    // {
        //    //     string sourceFileName = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, remoteScheduleInfo.OriginalScheduleName);
        //    //     string destFileName = Path.Combine(PlatformConfig.ServerCloudDirectoryPath, remoteScheduleInfo.OriginalScheduleName);
        //    //     File.Copy(sourceFileName, destFileName, true);
        //    // }
        //    if (this._sourceType == ScheduleSourceType.RemoteSource && this._type == ScheduleType.Common && !string.IsNullOrEmpty(this._convertedFilePath))
        //    {
        //        string originalFileName = Path.GetFileName(this._convertedFilePath);
        //        string optimizedFileName = Path.GetFileName(this._filePath);

        //        string destOriginalFileNameInEdit = Path.Combine(PlatformConfig.ServerEditDirectoryPath, Path.ChangeExtension(originalFileName, ".plym"));
        //        string destOptimizedFileNameInEdit = Path.Combine(PlatformConfig.ServerEditDirectoryPath, Path.ChangeExtension(optimizedFileName, ".plpym"));

        //        //string sourceOriginalFileName = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, remoteScheduleInfo.OriginalScheduleName);
        //        //string destOriginalFileNameInCloud = Path.Combine(PlatformConfig.ServerCloudDirectoryPath, Path.ChangeExtension(remoteScheduleInfo.OptimizedScheduleName, ".plym"));
        //        //string destOriginalFileNameInPluto = Path.Combine(PlatformConfig.ServerPlutoDirectoryPath, Path.ChangeExtension(remoteScheduleInfo.OptimizedScheduleName, ".plym"));
        //        File.Copy(this._filePath, destOptimizedFileNameInEdit, true);
        //        File.Copy(this._convertedFilePath, destOriginalFileNameInEdit, true);

        //        this._filePath = destOriginalFileNameInEdit;
        //        this._convertedFilePath = destOptimizedFileNameInEdit;
        //        
        //        //string sourceOptimizedFileName = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, remoteScheduleInfo.OptimizedScheduleName);
        //        //string destOptimizedFileNameInPluto = Path.Combine(PlatformConfig.ServerPlutoDirectoryPath, Path.ChangeExtension(remoteScheduleInfo.OriginalScheduleName, ".plpym"));
        //        //File.Copy(sourceOptimizedFileName, destOptimizedFileNameInPluto, true);
        //    }
        //}

        private void RefreshSchedule(string newFilePath, ScheduleSourceType sourceType)
        {

            //string filePath = string.Empty;
            //if(_sourceType == ScheduleSourceType.RemoteSource)
            //filePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, newFilePath);
            //else
            //    filePath = Path.Combine(PlatformConfig.LocalScheduleDirectoryPath,newFilePath);
            Schedule schedule = new Schedule();
            schedule.Type = _type;
            bool bSuccess;
            PlayProgramXml playProgramXml = new PlayProgramXml(
                newFilePath,
                XmlFile.XmlFileFlag.XmlExisting,
                out bSuccess);
            if (bSuccess)
            {
                schedule.FilePath = newFilePath;
                schedule.Name = Path.GetFileName(newFilePath);
                schedule.ScreenHeight = playProgramXml.ScreenHeight;
                schedule.ScreenWidth = playProgramXml.ScreenWidth;
                schedule.ScreenX = playProgramXml.ScreenX;
                schedule.ScreenY = playProgramXml.ScreenY;
                schedule.LastEditTime = File.GetLastWriteTime(newFilePath);
            }
            _scheduleModel = new ScheduleModel(schedule, _sourceType);

            ModelToViewModel(_scheduleModel);

            //SetPublishPathInfo(_filePath, _convertedFilePath, _mdcorFilePath, sourceType);

        }

        public bool ValidationMediaList()
        {
            List<IMedia> mediaList;
            PlayProgramParser.GetAllMedia(FilePath, out mediaList);
            return mediaList.Count == 0 ? false : true;
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


        public ScheduleViewModel Clone()
        {
            return new ScheduleViewModel(this._scheduleModel, this.RemoteInfo);
        }
    }
}