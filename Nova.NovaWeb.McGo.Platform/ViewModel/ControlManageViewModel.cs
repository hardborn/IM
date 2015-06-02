using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.Protocol;
using Nova.NovaWeb.Windows;
using Nova.NovaWeb.Windows.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class ControlManageViewModel : ViewModelBase
    {

        private ViewModelLocator _viewModelLocator;
        private IPlatformService _platformService;
        private ITransmissionInfoService _transmissionInfoService;

        public ControlManageViewModel()
        {
            _viewModelLocator = ViewModelLocator.Instance;
            _platformService = AppEnvionment.Default.Get<IPlatformService>();
            _transmissionInfoService = AppEnvionment.Default.Get<ITransmissionInfoService>();
        }

        private RelayCommand _refreshTerminalListCommand;
        public RelayCommand RefreshTerminalListCommand
        {
            get
            {
                if (_refreshTerminalListCommand == null)
                {
                    _refreshTerminalListCommand = new RelayCommand(
                        () => RefreshTerminalList(),
                        null);
                }
                return _refreshTerminalListCommand;
            }
        }

        private RelayCommand<object> _controlCMDCommand;
        public RelayCommand<object> ControlCMDCommand
        {
            get
            {
                if (_controlCMDCommand == null)
                {
                    _controlCMDCommand = new RelayCommand<object>(
                        (T) => ControlCMD(T),
                        (T) =>
                        {
                            if (_viewModelLocator.TerminalCollection.SelectedTerminals.Count > 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        });
                }
                return _controlCMDCommand;
            }
        }

        private ICommand _inquiryCMDCommand;
        public ICommand InquiryCMDCommand
        {
            get
            {
                if (_inquiryCMDCommand == null)
                {
                    _inquiryCMDCommand = new RelayCommand(
                        () => InquiryCMD(),
                        () =>
                        {
                            if (_viewModelLocator.TerminalCollection.SelectedTerminals.Count > 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }

                        });
                }
                return _inquiryCMDCommand;
            }
        }



        private void RefreshTerminalList()
        {
            NotificationMessage<string> notification = new NotificationMessage<string>(string.Empty, "RefreshTerminal");
            Messenger.Default.Send<NotificationMessage<string>>(notification, "RefreshTerminal");
        }


        private void InquiryCMD()
        {
            List<TerminalModel> terminals = new List<TerminalModel>();
            _viewModelLocator.TerminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

            try
            {
                //Modify-lixc
                InquiryWindowViewModel inquiryViewModel = new InquiryWindowViewModel(
                    terminals,
                    GetAllCommandTypes(),
                     new List<CmdTypes>(),
                      PlatformConfig.LanguageResourcePath,
                    _platformService.GetPlatformLanguage(),
                    true
                    //GetRequireCMDListURL(),
                    //GetRequireCMDRecordURL()
                    );//Path.Combine(ClientManager.LanguageResourcePath, @"zh-CN\TerminalControl.zh-CN.resources")
                InquireCommandWindow inquireCommandWindow = new InquireCommandWindow();
                inquireCommandWindow.DataContext = inquiryViewModel;
                inquireCommandWindow.Owner = App.Current.MainWindow;
                inquireCommandWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                inquireCommandWindow.ShowInTaskbar = false;
                inquireCommandWindow.ShowDialog();
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message);
                }));
            }

        }

        private Visibility _showTerminalLock = Visibility.Visible;

        public Visibility ShowTerminalLock
        {
            get
            {
                return _showTerminalLock;
            }
            set
            {
                if (value == _showTerminalLock)
                    return;
                _showTerminalLock = value;
                RaisePropertyChanged("ShowTerminalLock");
            }

        }

        private Visibility _showplutoPowerPlanSet = Visibility.Visible;

        public Visibility ShowPlutoPowerPlanSet
        {
            get
            {
                return _showplutoPowerPlanSet;
            }
            set
            {
                if (value == _showplutoPowerPlanSet)
                    return;
                _showplutoPowerPlanSet = value;
                RaisePropertyChanged("ShowPlutoPowerPlanSet");
            }

        }


        public enum terminalListType
        {
            allSyn,
            allAsyn,
            allType
        };

        public terminalListType JugeType()
        {
            terminalListType terlistType;
            int sysCount = 0;
            int asysCount = 0;
            foreach (var terminal in _viewModelLocator.TerminalCollection.SelectedTerminals)
            {

                TerminalViewModel objTerminal = terminal as TerminalViewModel;
                if (objTerminal.Type == TerminalType.Embedded)
                    asysCount++;
                else
                    sysCount++;
            }
            if (sysCount == _viewModelLocator.TerminalCollection.SelectedTerminals.Count)
                terlistType = terminalListType.allSyn;
            else
                if (asysCount == _viewModelLocator.TerminalCollection.SelectedTerminals.Count)
                    terlistType = terminalListType.allAsyn;
                else
                    terlistType = terminalListType.allType;
            return terlistType;
        }

        public void ControlCMD(object obj)
        {
            string param;

            if (obj == null)
            {
                param = string.Empty;
            }
            else
            {
                param = obj as string;
            }


            List<CommandControlType> cmdTypeList = new List<CommandControlType>();
            CommandControlType selectedCmdType = CommandControlType.brightnessAdjust;

            if (string.IsNullOrEmpty(param))
            {
                for (int i = 0; i < 13; i++)
                {
                    cmdTypeList.Add((CommandControlType)i);
                }
                if (EntryPoint.IsDebugModel == true)
                {
                    cmdTypeList.Add(CommandControlType.uploadSysLog);
                    cmdTypeList.Add(CommandControlType.uploadConfig);
                }

                //cmdTypeList.Add(CommandControlType.MonitorAlarmConfig);
                cmdTypeList.Remove(CommandControlType.imageMonitorSet);
                selectedCmdType = cmdTypeList[0];
            }
            else if (GetCommandTypeBusiness("uploadLogParaSet") == param)
            {
                cmdTypeList.Add(CommandControlType.playLogUploadParaSet);
                selectedCmdType = CommandControlType.playLogUploadParaSet;
            }
            else if (GetCommandTypeBusiness("screenShotPeriodSet") == param)
            {
                cmdTypeList.Add(CommandControlType.imageMonitorSet);
                selectedCmdType = CommandControlType.imageMonitorSet;
            }
            else if (GetCommandTypeBusiness("soundSchSet") == param)
            {
                cmdTypeList.Add(CommandControlType.volAdjust);
                selectedCmdType = CommandControlType.volAdjust;
            }
            else if (GetCommandTypeBusiness("lightSchSet") == param)
            {
                cmdTypeList.Add(CommandControlType.brightnessAdjust);
                selectedCmdType = CommandControlType.brightnessAdjust;
            }
            else if (GetCommandTypeBusiness("mediaClearup") == param)
            {
                cmdTypeList.Add(CommandControlType.mediaClearup);
                selectedCmdType = CommandControlType.mediaClearup;
            }
            else if (GetCommandTypeBusiness("displayModeSet") == param)
            {
                cmdTypeList.Add(CommandControlType.screenDisplay);
                //cmdTypeList.Add(CommandControlType.switchLed);
                selectedCmdType = CommandControlType.screenDisplay;
            }
            else if (GetCommandTypeBusiness("mediaSync") == param)
            {
                cmdTypeList.Add(CommandControlType.terminalSync);
                selectedCmdType = CommandControlType.terminalSync;
            }
            else if (GetCommandTypeBusiness("lockUnlockTC") == param)
            {
                cmdTypeList.Add(CommandControlType.terminalLock);
                selectedCmdType = CommandControlType.terminalLock;
            }
            else if (GetCommandTypeBusiness("terminalRestart") == param)
            {
                cmdTypeList.Add(CommandControlType.terminalRestart);
                selectedCmdType = CommandControlType.terminalRestart;
            }
            else if (GetCommandTypeBusiness("terminalConfig") == param)
            {
                cmdTypeList.Add(CommandControlType.terminalConfig);
                selectedCmdType = CommandControlType.terminalConfig;
            }
            else if (GetCommandTypeBusiness("terminalupdateSoftware") == param)
            {
                cmdTypeList.Add(CommandControlType.terminalUpdate);
                selectedCmdType = CommandControlType.terminalUpdate;
            }
            else if (GetCommandTypeBusiness("plutoPowerPlanSet") == param)
            {
                cmdTypeList.Add(CommandControlType.plutoPowerPlanSet);
                selectedCmdType = CommandControlType.plutoPowerPlanSet;
            }
            else if (GetCommandTypeBusiness("switchLed") == param)
            {
                cmdTypeList.Add(CommandControlType.switchLed);
                selectedCmdType = CommandControlType.switchLed;
            }
            else if (GetCommandTypeBusiness("uploadSysLog") == param)
            {
                cmdTypeList.Add(CommandControlType.uploadSysLog);
                selectedCmdType = CommandControlType.uploadSysLog;
            }
            else if (GetCommandTypeBusiness("TerminalConfigUpload") == param)
            {
                cmdTypeList.Add(CommandControlType.uploadConfig);
                selectedCmdType = CommandControlType.uploadConfig;
            }

            terminalListType terlistType = JugeType();

            if (terlistType == terminalListType.allSyn)
            {
                cmdTypeList.Remove(CommandControlType.plutoPowerPlanSet);
            }
            else
                if (terlistType == terminalListType.allAsyn)
                {
                    cmdTypeList.Remove(CommandControlType.terminalLock);
                }



            //Modify-lixc
            try
            {
                List<TerminalModel> terminals = new List<TerminalModel>();
                _viewModelLocator.TerminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));
                Frm_CommandControl commandControlView = new Frm_CommandControl(
                    PlatformConfig.LanguageResourcePath,//Path.Combine(ClientManager.LanguageResourcePath, GetLanguageResourcePath("CommandControl")),
                    _platformService.GetPlatformLanguage(),
                    0,
                    cmdTypeList,
                    selectedCmdType,
                    terminals);
                commandControlView.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                commandControlView.ShowInTaskbar = false;
                commandControlView.ShowDialog(new Wfp32Window(App.Current.MainWindow));
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, e.Message);
                }));
            }


        }

        private string GetCommandTypeBusiness(string key)
        {
            string result;
            MultiLanguageUtils.GetLanguageString(key, out result);
            return result;
        }

        private List<CmdTypes> GetAllCommandTypes()
        {
            List<CmdTypes> allCommandList = new List<CmdTypes>();

            allCommandList = new List<CmdTypes>{ CmdTypes.uploadLogParaSet,
                CmdTypes.soundSchSet,
                CmdTypes.lightSchSet,               
                CmdTypes.switchLed,
                CmdTypes.mediaSync,
                CmdTypes.cleanMediasPeriodSet,
                CmdTypes.cleanMedias,
                CmdTypes.lockUnlockTC,
                CmdTypes.restartTeminal,
                CmdTypes.autoRestartTerminalSet,
                CmdTypes.terminalConfig,
                CmdTypes.terminalupdateSoftware,
                CmdTypes.plutoPowerPlanSet,
                CmdTypes.displayModeSet
                };

            if (EntryPoint.IsDebugModel == true)
            {
                allCommandList.AddRange(new List<CmdTypes>{ CmdTypes.downloadPlaylist,
                CmdTypes.uploadLog,
                CmdTypes.heartBeatPeriodSet,
                CmdTypes.heartBeatPeriodRead,
                CmdTypes.cleanMediasPeriodRead,
                CmdTypes.brightnessSet,
                CmdTypes.uploadLogParaRead,
                CmdTypes.uploadDirSet,
                CmdTypes.updateSoftware,
                CmdTypes.emergencyMessage,
                CmdTypes.detectPoint,
                CmdTypes.readLedInfo,
                CmdTypes.setVolume,
                CmdTypes.playListSync,
                CmdTypes.enableReadMonitorInfo,
                CmdTypes.readSoftwareVersion,
                CmdTypes.readScreenShot,
                CmdTypes.screenShotPeriodSet,
                CmdTypes.readCameraInfo,
                CmdTypes.downloadSchedule,
                CmdTypes.readScrResolution,
                CmdTypes.dviEncrypt,
                CmdTypes.modifyDviEncryptPwd,
                CmdTypes.enableDviEncrypt,
                CmdTypes.readDviEncryptConfig,
                CmdTypes.readDviEncryptStatus,
                CmdTypes.uploadSysLog,
                CmdTypes.emergencyPlaylist,
                CmdTypes.updateVideoList,
                CmdTypes.readEnabledVideoList,
                CmdTypes.updateSSL,
                CmdTypes.downLoadPlayProgam,
                CmdTypes.updateServerTimeZone,
                CmdTypes.updateTcTimeZone,
                CmdTypes.autoRestartTerminalRead,
                CmdTypes.plutoPowerPlanRead,
                CmdTypes.equipCodeRead,
                CmdTypes.mediaSyncRead,
                CmdTypes.brightnessRead,
                CmdTypes.volumeRead,
                CmdTypes.lockUnlockTCRead,
                CmdTypes.switchLedRead,
                CmdTypes.displayModeRead,
                CmdTypes.lightSchRead,
                CmdTypes.soundSchRead,
                CmdTypes.monitorAlarmSet,
                CmdTypes.monitorAlarmRead,
                CmdTypes.serProtocolVer,
                CmdTypes.downloadPlaySchedule,
                CmdTypes.downloadPeriodSet,
                CmdTypes.downloadPeriodRead,
                CmdTypes.downloadDir,
                CmdTypes.downloadEmergencyPlaylist,
                CmdTypes.SSLRead,
                CmdTypes.terminalTimeZoneRead,
                CmdTypes.TerminalConfigUpload
                });
            }
            return allCommandList;
        }

    }
}
