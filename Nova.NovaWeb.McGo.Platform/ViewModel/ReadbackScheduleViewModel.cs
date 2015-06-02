using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.Xml;
using Nova.Xml.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class ReadbackScheduleViewModel : ViewModelBase
    {
        private RelayCommand<object> _loadedWindowCommand;
        private RelayCommand _cancelReadbackCommand;
        private BackgroundWorker _downloadWorker;
        private FileSetDownLoader _fileSetDownloader;
        private ITransmissionInfoService _transmissionInfoService;

        private bool _isScheduleDownloading;
        private TerminalViewModel _terminalViewModel;
        private ILog _log;

        public ReadbackScheduleViewModel(TerminalViewModel terminalViewModel)
        {
            _terminalViewModel = terminalViewModel;
            _transmissionInfoService = AppEnvionment.Default.Get<ITransmissionInfoService>();
            ClearOldSchedules();
            if (_transmissionInfoService == null)
                throw new NotImplementedException("_transmissionInfoService");

            _log = AppEnvionment.Default.Get<ILog>();
        }


        public bool IsScheduleDownloading
        {
            get
            {
                return _isScheduleDownloading;
            }
            set
            {
                if (_isScheduleDownloading == value)
                    return;

                _isScheduleDownloading = value;
                RaisePropertyChanged("IsScheduleDownloading");
            }
        }

        public TerminalViewModel CurrentTerminal
        {
            get
            {
                return _terminalViewModel;
            }
        }

        public RelayCommand<object> LoadedWindowCommand
        {
            get
            {
                if (_loadedWindowCommand == null)
                {
                    _loadedWindowCommand = new RelayCommand<object>((o) =>
                    {
                        LoadedWindow();
                    },
                    null);
                }
                return _loadedWindowCommand;
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

        private void ClearOldSchedules()
        {
            DeleteFiles(PlatformConfig.RemoteScheduleDirectoryPath);
            DeleteFiles(PlatformConfig.RemotePublicationDirectoryPath);
            DeleteFiles(PlatformConfig.RemotePlutoPublicationDirectoryPath);
            DeleteFiles(PlatformConfig.RemoteCloudPublicationDirectoryPath);
        }

        private void DeleteFiles(string directoryPath)
        {
            if(Directory.Exists(directoryPath) == false)
            {
                return;
            }

            DirectoryInfo fdir = new DirectoryInfo(directoryPath);
            FileInfo[] file = fdir.GetFiles();
            if (file == null)
                return;
            foreach (var item in file)
            {
                File.Delete(item.FullName);
            }
        }

        private void LoadedWindow()
        {
            _downloadWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _downloadWorker.DoWork += (o, ea) =>
            {
                DownloadScheduleFormServer();
            };

            _downloadWorker.RunWorkerCompleted += (o, ea) =>
            {
            };
            IsScheduleDownloading = true;
            _downloadWorker.RunWorkerAsync();
        }
        private void CancelReadback()
        {
            IsScheduleDownloading = false;
            if (_downloadWorker != null)
                _downloadWorker.CancelAsync();
            CancelScheduleDownload();
        }

        public void CancelScheduleDownload()
        {
            if (_fileSetDownloader != null)
            {
                _fileSetDownloader.CancelTransmite();
            }
        }

        private void DownloadScheduleFormServer()
        {
            var downloadTransmissionInfo = _transmissionInfoService.GetPlatformDataTransInfo();
            if (downloadTransmissionInfo.Type == null)
            {
                downloadTransmissionInfo = null;
                FileSetTransmiteCompleteEventArgs e = new FileSetTransmiteCompleteEventArgs( FileSetTransmiteRes.ConnectToServerFailed, null, null);
                fileDownLoader_FileSetDownLoadCompleteEvent(null, e);
            }
            if (downloadTransmissionInfo == null)
                throw new NotImplementedException("downloadTransmissionInfo");

            // App.Log()

            var serverPath = GetDownloadServerPath();

            string downloadUrl = serverPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);//.Replace(":", ":/");

            _fileSetDownloader = new FileSetDownLoader(new Uri(downloadUrl), downloadTransmissionInfo.Account, downloadTransmissionInfo.Password, Encoding.UTF8);

            var appData = AppEnvionment.Default.Get<IPlatformService>();
            if (appData == null)
                throw new NotImplementedException("appData");

            _fileSetDownloader.IsEnableSsl = appData.GetAppData().IsEnableFTPS;

            _fileSetDownloader.FileSetDownLoadCompleteEvent += fileDownLoader_FileSetDownLoadCompleteEvent;

            List<string> downloadFilePathList = new List<string>();
            if (_terminalViewModel.Type == TerminalType.PC)
            {
                downloadFilePathList.Add(Path.GetFileName(_terminalViewModel.PlayingScheduleInfo.OriginalScheduleName));
            }
            else
            {
                downloadFilePathList.Add(Path.GetFileName(_terminalViewModel.PlayingScheduleInfo.OriginalScheduleName));
                downloadFilePathList.Add(Path.GetFileName(_terminalViewModel.PlayingScheduleInfo.OptimizedScheduleName));
            }
            //if (File.Exists(PlatformConfig.RemoteScheduleDirectoryPath))

            _fileSetDownloader.DownLoadFileSet(downloadFilePathList, ".temp", true, true, PlatformConfig.RemoteScheduleDirectoryPath, null);
        }

        private string GetDownloadServerPath()
        {
            var transimissionService = AppEnvionment.Default.Get<ITransmissionInfoService>();
            var transimissionInfo = transimissionService.GetPlatformDataTransInfo();
            App.Log(string.Format("Schedule下载地址为：{0}", Path.Combine(transimissionInfo.DestinationAddress, "PlayProgram")));
            return Path.Combine(transimissionInfo.DestinationAddress, "PlayProgram");
        }

        private void fileDownLoader_FileSetDownLoadCompleteEvent(object sender, FileSetTransmiteCompleteEventArgs e)
        {
            App.Log(string.Format("Schedule下载结果：{0}", e.TransmiteRes));
            switch (e.TransmiteRes)
            {
                case FileSetTransmiteRes.Cancelled:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_Cancelled", "下载被取消"));
                    }));
                    break;
                case FileSetTransmiteRes.ConnectToServerFailed:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_ConnectToServerFailed", "连接服务器失败"));

                    }));
                    break;
                case FileSetTransmiteRes.FileSizeConflict:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_FileSizeConflict", "文件大小冲突"));

                    }));
                    break;
                case FileSetTransmiteRes.NotAllFileExistInServer:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_NotAllFileExistInServer", "不是所有的文件都存在于服务器"));

                    }));
                    break;
                case FileSetTransmiteRes.OK:
                    _terminalViewModel.EditingSchedule = GenerateEditingSchedule(_terminalViewModel.PlayingScheduleInfo);
                    //CopyScheduleToWorkspace();
                    //_terminalViewModel.EditingSchedule.ParaseSchedule();
                    //RaisePropertyChanged("CurrentTerminal");
                    break;
                case FileSetTransmiteRes.PathNotExistInServer:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_PathNotExistInServer", "下载路径不存在于服务器"));

                    }));
                    break;
                case FileSetTransmiteRes.WrongUserNameOrPwd:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_WrongUserNameOrPwd", "用户名或密码错误"));

                    }));
                    break;
                case FileSetTransmiteRes.UnknowErr:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_UnknowErr", "下载过程中出现未知错误"));

                    }));
                    break;
                case FileSetTransmiteRes.InsufficientHarddiskSpaceLocal:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_InsufficientHarddiskSpaceLocal", "本地磁盘空间不足"));

                    }));
                    break;
                case FileSetTransmiteRes.InsufficientHarddiskSpaceServer:
                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, MultiLanguageUtils.GetLanguageString("Result_FileSetTransmiteRes_InsufficientHarddiskSpaceServer", "服务器磁盘空间不足"));

                    }));
                    _fileSetDownloader.CancelTransmite();
                    break;
            }

            DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            {
                IsScheduleDownloading = false;
            }));
            
           if( e.TransmiteRes != FileSetTransmiteRes.OK)
           {
               Messenger.Default.Send<string>(string.Empty, "CloseReadbackView");
           }
        }

        public ScheduleViewModel GenerateEditingSchedule(RemoteScheduleInfo remoteScheduleInfo)
        {
            if (remoteScheduleInfo == null)
            {
                App.Log(new Exception(string.Format("播放方案信息不存在。(GenerateEditingSchedule)")));
                return null;
            }


            string filePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, remoteScheduleInfo.OriginalScheduleName);

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                App.Log(new Exception(string.Format("播放方案文件{0}不存在。(GenerateEditingSchedule)", filePath)));
                return null;
            }
            Schedule schedule = new Schedule();

            string editFilePath = string.Empty;
            if (remoteScheduleInfo.Type == ScheduleType.PC || _terminalViewModel.Type == TerminalType.PC)
            {
                editFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, remoteScheduleInfo.OriginalScheduleName);

                schedule.Type = remoteScheduleInfo.Type;

                bool bSuccess;
                PlayProgramXml playProgramXml = new PlayProgramXml(
                    editFilePath, 
                    Nova.Xml.XmlFile.XmlFileFlag.XmlExisting,
                    out bSuccess);
                if (bSuccess)
                {
                    schedule.FilePath = editFilePath;
                    schedule.Name = Path.GetFileName(editFilePath);
                    schedule.ScreenHeight = playProgramXml.ScreenHeight;
                    schedule.ScreenWidth = playProgramXml.ScreenWidth;
                    schedule.ScreenX = playProgramXml.ScreenX;
                    schedule.ScreenY = playProgramXml.ScreenY;
                    schedule.LastEditTime = File.GetLastWriteTime(editFilePath);
                }
            }
            else
            {
                editFilePath = Path.Combine(PlatformConfig.RemoteScheduleDirectoryPath, remoteScheduleInfo.OptimizedScheduleName);

                bool bSuccess;
                PlayProgramXml playProgramXml = new PlayProgramXml(
                    editFilePath,
                    Nova.Xml.XmlFile.XmlFileFlag.XmlExisting,
                    out bSuccess);
                if (bSuccess)
                {
                    schedule.FilePath = editFilePath;
                    schedule.Name = Path.GetFileName(editFilePath);
                    schedule.ScreenHeight = playProgramXml.ScreenHeight;
                    schedule.ScreenWidth = playProgramXml.ScreenWidth;
                    schedule.ScreenX = playProgramXml.ScreenX;
                    schedule.ScreenY = playProgramXml.ScreenY;
                    schedule.Type = ScheduleType.Common;
                    schedule.LastEditTime = File.GetLastWriteTime(editFilePath);
                }
            }

            ScheduleModel scheduleModel = new ScheduleModel(schedule, ScheduleSourceType.RemoteSource);
            return new ScheduleViewModel(scheduleModel, remoteScheduleInfo); //是否需要初始化回读播放方案发布信息
        }

        private void CopyScheduleToWorkspace(RemoteScheduleInfo remoteScheduleInfo)
        {
            //if (remoteScheduleInfo == null)
            //    return;

            //if (remoteScheduleInfo.Type == NovaWeb.Common.ScheduleType.PC)
            //{
            //    string sourceFileName = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, remoteScheduleInfo.OriginalScheduleName);
            //    string destFileName = Path.Combine(PlatformConfig.ServerCloudDirectoryPath, remoteScheduleInfo.OriginalScheduleName);
            //    File.Copy(sourceFileName, destFileName, true);
            //}
            //else
            //{
            //    string sourceOriginalFileName = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, remoteScheduleInfo.OriginalScheduleName);
            //    string destOriginalFileNameInCloud = Path.Combine(PlatformConfig.ServerCloudDirectoryPath, Path.ChangeExtension(remoteScheduleInfo.OptimizedScheduleName, ".plym"));
            //    string destOriginalFileNameInPluto = Path.Combine(PlatformConfig.ServerPlutoDirectoryPath, Path.ChangeExtension(remoteScheduleInfo.OptimizedScheduleName, ".plym"));
            //    File.Copy(sourceOriginalFileName, destOriginalFileNameInCloud, true);
            //    File.Copy(sourceOriginalFileName, destOriginalFileNameInPluto, true);

            //    string sourceOptimizedFileName = Path.Combine(PlatformConfig.ServerPlayDirectoryPath, remoteScheduleInfo.OptimizedScheduleName);
            //    string destOptimizedFileNameInPluto = Path.Combine(PlatformConfig.ServerPlutoDirectoryPath, Path.ChangeExtension(remoteScheduleInfo.OriginalScheduleName, ".plpym"));
            //    File.Copy(sourceOptimizedFileName, destOptimizedFileNameInPluto, true);

            //}
        }
    }
}
