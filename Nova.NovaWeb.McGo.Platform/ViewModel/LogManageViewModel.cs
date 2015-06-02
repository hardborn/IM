using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.Protocol;
using Nova.NovaWeb.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class LogManageViewModel : ViewModelBase
    {
        private RelayCommand _refreshTerminalListCommand;
        private RelayCommand _viewLogCommand;
        private RelayCommand _exportLogCommand;
        private RelayCommand _setLogCommand;

        public LogManageViewModel()
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

        public RelayCommand ViewLogCommand
        {
            get
            {
                if (_viewLogCommand == null)
                {
                    _viewLogCommand = new RelayCommand(
                       () => ViewLog(),
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
                return _viewLogCommand;
            }
        }

        public RelayCommand ExportLogCommand
        {
            get
            {
                if (_exportLogCommand == null)
                {
                    _exportLogCommand = new RelayCommand(
                        () => ExportLog(),
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
                return _exportLogCommand;
            }
        }

        public RelayCommand SetLogCommand
        {
            get
            {
                if (_setLogCommand == null)
                {
                    _setLogCommand = new RelayCommand(
                        () => SetLog(),
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
                return _setLogCommand;
            }
        }


        private void RefreshTerminalList()
        {
            NotificationMessage<string> notification = new NotificationMessage<string>(string.Empty, "RefreshTerminal");
            Messenger.Default.Send<NotificationMessage<string>>(notification, "RefreshTerminal");
        }

        public void ViewLog()
        {
            var transService = AppEnvionment.Default.Get<ITransmissionInfoService>();
            var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            TransmissionInfo info = transService.GetTerminalDataTransInfo(terminalCollection.SelectedTerminals[0].GroupId.ToString());

            var platformService = AppEnvionment.Default.Get<IPlatformService>();
            ServerInfo serverInfo = platformService.GetPlatformServerInfo();


            List<TerminalModel> terminals = new List<TerminalModel>();
            terminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

            //Modify-lixc
            if (terminals.Count == 1)
            {
                try
                {
                    Nova.NovaWeb.UI.PlayLogView playLogView = new Nova.NovaWeb.UI.PlayLogView(
                    terminals[0],
                    PlatformConfig.LogDirectoryPath,
                    PlatformConfig.LanguageResourcePath,
                    platformService.GetPlatformLanguage());
                    playLogView.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                    playLogView.ShowInTaskbar = false;
                   // frmSetEmergyMsg.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                    playLogView.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                    //Messenger.Default.Register<NotificationMessage>(this, "LogOutMessage", message =>
                    //{
                    //    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                    //    {
                    //        playLogView.Close();
                    //    }));
                       
                    //});
                }
                catch (Exception e)
                {
                    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(App.Current.MainWindow, e.StackTrace);
                    }));
                }
            }
        }

        private void ExportLog()
        {
            var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();

            var platformService = AppEnvionment.Default.Get<IPlatformService>();
            ServerInfo serverInfo = platformService.GetPlatformServerInfo();

            //Modify-lixc
            List<TerminalModel> terminals = new List<TerminalModel>();
            terminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

            try
            {
                ExportLogFrm exportLogFrm = new ExportLogFrm(
                terminals,
                 PlatformConfig.LogDirectoryPath,
                    PlatformConfig.LanguageResourcePath,
                    platformService.GetPlatformLanguage());
                exportLogFrm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                exportLogFrm.ShowInTaskbar = false;
                exportLogFrm.ShowDialog(new Wfp32Window(App.Current.MainWindow));
                exportLogFrm.Activate();
                Messenger.Default.Register<NotificationMessage>(this, "LogOutMessage", message =>
                {
                    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                    {
                        exportLogFrm.Close();
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

        private void SetLog()
        {
            var terminalCollection = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();

            var platformService = AppEnvionment.Default.Get<IPlatformService>();
            ServerInfo serverInfo = platformService.GetPlatformServerInfo();

            //Modify-lixc
            List<TerminalModel> terminals = new List<TerminalModel>();
            terminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

            try
            {
                Frm_CommandControl commandControlView = new Frm_CommandControl(
                    PlatformConfig.LanguageResourcePath,
                    platformService.GetPlatformLanguage(),
                0,
                // ClientManager.UserConfig.UserPassword,
                new List<CommandControlType>() { CommandControlType.playLogUploadParaSet },
                CommandControlType.playLogUploadParaSet,
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
