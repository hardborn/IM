using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.NovaWebMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nova.NovaWeb.Protocol;
using Nova.NovaWeb.McGo.Common;
using GalaSoft.MvvmLight.Threading;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class MonitorManageViewModel : ViewModelBase
    {
        private RelayCommand _refreshTerminalListCommand;
        private RelayCommand _snapshotCommand;
        private RelayCommand _setSnapshotCommand;
        private RelayCommand _monitorInfoCommand;
        private RelayCommand _alarmMonitorCommand;

        public MonitorManageViewModel()
        {
        }

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

        public RelayCommand SnapshotCommand
        {
            get
            {
                if (_snapshotCommand == null)
                {
                    _snapshotCommand = new RelayCommand(
                        () => Snapshot(),
                        () =>
                    {
                        var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
                        if (terminalCollection.SelectedTerminals.Count == 1)
                        {
                            if (terminalCollection.SelectedTerminals[0].IsScreenShot == false && terminalCollection.SelectedTerminals[0].EnableIPCam != true)
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
                return _snapshotCommand;
            }
        }

        public RelayCommand SetSnapshotCommand
        {
            get
            {
                if (_setSnapshotCommand == null)
                {
                    _setSnapshotCommand = new RelayCommand(
                        () => SetSnapshot(),
                        () =>
                    {
                        var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
                        if (terminalCollection.SelectedTerminals.Count > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    });
                }
                return _setSnapshotCommand;
            }
        }


        public RelayCommand MonitorInfoCommand
        {
            get
            {
                if (_monitorInfoCommand == null)
                {
                    _monitorInfoCommand = new RelayCommand(
                        () => MonitorInfo(),
                        () =>
                    {
                        var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
                        if (terminalCollection.SelectedTerminals.Count == 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
                return _monitorInfoCommand;
            }
        }
        public RelayCommand AlarmMonitorCommand
        {
            get
            {
                if (_alarmMonitorCommand == null)
                {
                    _alarmMonitorCommand = new RelayCommand(
                        () => AlarmMonitor(),
                        () =>
                    {
                        var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
                        if (terminalCollection.SelectedTerminals.Count == 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
                return _alarmMonitorCommand;
            }
        }

        private void RefreshTerminalList()
        {
            NotificationMessage<string> notification = new NotificationMessage<string>(string.Empty, "RefreshTerminal");
            Messenger.Default.Send<NotificationMessage<string>>(notification, "RefreshTerminal");
        }

        private void AlarmMonitor()
        {
             var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            var platformService = AppEnvionment.Default.Get<IPlatformService>();

            AlarmView.MainWindow alarmView = new AlarmView.MainWindow(new List<TerminalModel>(){ terminalCollection.SelectedTerminals[0].CurrentTerminal},
               platformService.GetPlatformLanguage(),
               PlatformConfig.LanguageResourcePath);
            alarmView.Owner = App.Current.MainWindow;
            alarmView.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            alarmView.ShowInTaskbar = false;
            alarmView.ShowDialog();
        }
        public void MonitorInfo()
        {
            var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            var platformService = AppEnvionment.Default.Get<IPlatformService>();

            try
            {
                frmViewMonitor frmMonitor = new frmViewMonitor(
                terminalCollection.SelectedTerminals[0].CurrentTerminal,
                 PlatformConfig.LanguageResourcePath,
                    platformService.GetPlatformLanguage());
                //ClientManager.UserConfig.UserPassword);
                frmMonitor.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                frmMonitor.ShowInTaskbar = false;
                frmMonitor.ShowDialog(new Wfp32Window(App.Current.MainWindow));
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, e.Message);
                }));
            }
        }



        private void Snapshot()
        {
            var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            var platformService = AppEnvionment.Default.Get<IPlatformService>();

            try
            {
                //Modify-lixc
                //terminalCollection.SelectedTerminals[0].CurrentTerminal.CurrentTerminal.IPCam = terminalCollection.SelectedTerminals[0].CurrentTerminal.CurrentTerminal.IPCam.Replace("IPC...1:","").Replace("IP...2:","");

                //string[] strSplit =  terminalCollection.SelectedTerminals[0].CurrentTerminal.CurrentTerminal.IPCam.Split(new char[] { '+' });
                //if (strSplit == null || strSplit.Length != 2)
                //    return;

                //string ipCam = string.Empty;

                //if (string.IsNullOrEmpty(strSplit[0]) && string.IsNullOrEmpty(strSplit[1]))
                //    ipCam = string.Empty;
                //else
                //    ipCam = terminalCollection.SelectedTerminals[0].CurrentTerminal.CurrentTerminal.IPCam.Replace("IpCamera1:", "").Replace("IpCamera2:", "");

                //if (!string.IsNullOrEmpty(ipCam))
                //{
                //    string[] tempStrs = ipCam.Split(new char[] { '+' });
                //    if (tempStrs == null || tempStrs.Length != 2)
                //        return;
                //    //if (string.IsNullOrWhiteSpace(tempStrs[0]) && string.IsNullOrWhiteSpace(tempStrs[1]))
                //    //    ipCam = string.Empty;
                //    if (string.IsNullOrWhiteSpace(tempStrs[0]))
                //        ipCam = tempStrs[1];
                //    if (string.IsNullOrWhiteSpace(tempStrs[1]))
                //        ipCam = tempStrs[0];
                //}


                //TerminalModel terminalMode = new TerminalModel()
                //{
                //    CurrentTerminal = new Site()
                //    {
                //        Mac = terminalCollection.SelectedTerminals[0].CurrentTerminal.Mac,
                //        Sitename = terminalCollection.SelectedTerminals[0].CurrentTerminal.Name,
                //        IPCam = ipCam
                //    },
                //    GroupId = terminalCollection.SelectedTerminals[0].CurrentTerminal.GroupId,
                //    GroupName = terminalCollection.SelectedTerminals[0].CurrentTerminal.GroupName
                //};

                //var terminalModel = new TerminalModel(
                //    new Group() { GroupID=terminalCollection.SelectedTerminals[0].GroupId, GroupName=terminalCollection.SelectedTerminals[0].GroupName },
                //     terminalCollection.SelectedTerminals[0].CurrentTerminal.CurrentTerminal,
                //     terminalCollection.SelectedTerminals[0].CurrentTerminal.TerminalStatus);

                //terminalModel.CurrentTerminal.IPCam = ipCam;

                ImageMontiorFrmMain imageMonitor = new ImageMontiorFrmMain(
               terminalCollection.SelectedTerminals[0].CurrentTerminal, //terminalModel,// terminalMode, //terminalCollection.SelectedTerminals[0].CurrentTerminal,
                terminalCollection.SelectedTerminals[0].IsScreenShot,
                PlatformConfig.ImageMonitorDirectoryPath,
                PlatformConfig.LanguageResourcePath,
                    platformService.GetPlatformLanguage());

                imageMonitor.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                imageMonitor.ShowInTaskbar = false;
                imageMonitor.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                Messenger.Default.Register<NotificationMessage>(this, "LogOutMessage", message =>
                {
                    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                    {
                        imageMonitor.Close();
                    }));
                });
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, e.Message);
                }));
            }
        }

        private void SetSnapshot()
        {
            var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            var platformService = AppEnvionment.Default.Get<IPlatformService>();

            try
            {
                //Modify-lixc
                List<TerminalModel> terminals = new List<TerminalModel>();
                terminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));
                Frm_CommandControl commandControlView = new Frm_CommandControl(
                 PlatformConfig.LanguageResourcePath,
                    platformService.GetPlatformLanguage(),
                 0,
                // ClientManager.UserConfig.UserPassword,
                new List<CommandControlType>() { CommandControlType.imageMonitorSet },
                CommandControlType.imageMonitorSet,
                terminals);
                commandControlView.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                commandControlView.ShowInTaskbar = false;
                commandControlView.ShowDialog(new Wfp32Window(App.Current.MainWindow));
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, e.Message);
                }));
            }
        }
    }
}
