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
using Nova.NovaWeb.UI;
using Nova.NovaWeb.Windows;
using Nova.NovaWeb.Windows.ViewModel;
using Nova.Pluto.PlutoManager.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class PublishManageViewModel : ViewModelBase
    {
        private ITerminalRepositoryProvider _terminalRepositoryProvider;
        private IPlatformService _platformService;
        private ViewModelLocator _viewModelLocator;

        private bool _isShowCommand = false;

        private ICommand _openPublishWizardCommand;
        private ICommand _refreshTerminalListCommand;
        private ICommand _selectionChangeCommand;

        private ICommand _openEmergencyPlaylistCommand;
        private RelayCommand _inquiryPublishHistoryCommand;
        private RelayCommand _openInstantNotificationCommand;
        private RelayCommand _inquiryEmergencyHistoryCommand;
        private RelayCommand _inquiryInstantHistoryCommand;
        //public FrmSetEmergyMsg frmSetEmergyMsg = null;
        public PublishManageViewModel()
        {
            _viewModelLocator = ViewModelLocator.Instance; 
            _platformService = AppEnvionment.Default.Get<IPlatformService>();

            //Messenger.Default.Register<NotificationMessage>(this, "AutoLogoutAccout", message =>
            //{
            //    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
            //    {
            //        if (frmSetEmergyMsg != null)
            //        {
            //            frmSetEmergyMsg.Close();
            //        }
            //    }));
            //});
        }

        public ICommand OpenPublishWizardCommand
        {
            get
            {
                if (_openPublishWizardCommand == null)
                {
                    _openPublishWizardCommand = new RelayCommand(
                        () => OpenPublishWizard(),
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
                return _openPublishWizardCommand;
            }
        }

        public ICommand OpenEmergencyPlaylistCommand
        {
            get
            {
                if (_openEmergencyPlaylistCommand == null)
                {
                    _openEmergencyPlaylistCommand = new RelayCommand(
                        () => OpenEmergencyPlaylistView(),
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
                return _openEmergencyPlaylistCommand;
            }
        }

        public ICommand OpenInstantNotificationCommand
        {
            get
            {
                if (_openInstantNotificationCommand == null)
                {
                    _openInstantNotificationCommand = new RelayCommand(
                        () => OpenInstantNotification(),
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
                return _openInstantNotificationCommand;
            }
        }
        public ICommand RefreshTerminalListCommand
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

        public ICommand InquiryPublishHistoryCommand
        {
            get
            {
                if (_inquiryPublishHistoryCommand == null)
                {
                    _inquiryPublishHistoryCommand = new RelayCommand(
                        () => InquiryPublishHistroy(),
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
                return _inquiryPublishHistoryCommand;
            }
        }

        public ICommand InquiryEmergencyHistoryCommand
        {
            get
            {
                if (_inquiryEmergencyHistoryCommand == null)
                {
                    _inquiryEmergencyHistoryCommand = new RelayCommand(
                       () => InquiryEmergencyHistory(),
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
                return _inquiryEmergencyHistoryCommand;
            }
        }

        public ICommand InquiryInstantHistoryCommand
        {
            get
            {
                if (_inquiryInstantHistoryCommand == null)
                {
                    _inquiryInstantHistoryCommand = new RelayCommand(
                        () => InquiryInstantHistory(),
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
                return _inquiryInstantHistoryCommand;
            }
        }

        /// <summary>
        /// 批处理标志位
        /// </summary>
        private bool _isBatch = true;
        public bool IsBatch
        {
            get
            {
                return _isBatch;
            }
            set
            {
                if (value.Equals(_isBatch))
                    return;
                _isBatch = value;
                if (_isBatch == false)
                {
                    ShowCheckBox = Visibility.Visible;
                    SelectModelHint = MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_Batch", "批量模式");
                } 
                else
                {
                    ShowCheckBox = Visibility.Collapsed;
                    SelectModelHint = MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_NoBatch", "非批量模式");
                }
                    
                RaisePropertyChanged("IsBatch");
            }
        }

        /// <summary>
        /// 气泡提示
        /// </summary>
        private string _selectModelHint = MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_NoBatch", "非批量模式");
        public string SelectModelHint
        {
            get
            {
                return _selectModelHint;
            }
            set
            {
                if (value == _selectModelHint)
                    return;
                _selectModelHint = value;
                RaisePropertyChanged("SelectModelHint");
            }

        }

        private Visibility _showCheckBox = Visibility.Collapsed;
        public Visibility ShowCheckBox
        {
            get
            {
                return _showCheckBox;
            }
            set
            {
                if (value.Equals(_showCheckBox))
                    return;
                _showCheckBox = value;
                RaisePropertyChanged("ShowCheckBox");
            }
        }




        private RelayCommand<object> _changeBatchMode;
        public RelayCommand<object> ChangeBatchMode
        {

            get
            {
                if (_changeBatchMode == null)
                {
                    _changeBatchMode = new RelayCommand<object>(
                         (T) =>
                         {
                             IsBatch = !IsBatch;

                             System.Windows.Controls.DataGrid terminalDataGrid = App.Current.MainWindow.Resources["dataGrid"] as System.Windows.Controls.DataGrid;
                             if (terminalDataGrid == null)
                                 return;
                             TerminalRepositoryViewModel repositoryViewModel = terminalDataGrid.DataContext as TerminalRepositoryViewModel;
                             if (repositoryViewModel == null)
                                 return;

                             if(IsBatch == false)
                             {   
                                 terminalDataGrid.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.Collapsed;
                             }
                             else
                             {
                                 if (terminalDataGrid.SelectedItems != null && terminalDataGrid.SelectedItems.Count > 1)
                                 {
                                     terminalDataGrid.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.Collapsed;
                                 }
                                 else
                                 {
                                     terminalDataGrid.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                                 }
                             }


                             foreach (var item in terminalDataGrid.Items)
                             {
                                
                                 TerminalViewModel terminalViewModel = item as TerminalViewModel;
                                 if (terminalViewModel == null)
                                     continue;
                                 terminalViewModel.CheckBoxChecked = false;
                             }
                             terminalDataGrid.SelectedItems.Clear();
                             repositoryViewModel.SelectedTerminals.Clear();

                            
                         }, null);
                }
                return _changeBatchMode;
            }
        }
        private void InquiryPublishHistroy()
        {
            List<TerminalModel> terminals = new List<TerminalModel>();
            _viewModelLocator.TerminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

            try
            {
                InquiryWindowViewModel inquiryViewModel = new InquiryWindowViewModel(
                terminals,
                new List<CmdTypes>() { CmdTypes.downloadPlaySchedule },
                new List<CmdTypes>() { CmdTypes.downloadPlaySchedule },
                 PlatformConfig.LanguageResourcePath,
                _platformService.GetPlatformLanguage(),
                true
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
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,e.Message);
                }));
            }
            
        }

        private void InquiryEmergencyHistory()
        {
            List<TerminalModel> terminals = new List<TerminalModel>();
            _viewModelLocator.TerminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

            try
            {
                //Modify-lixc
                InquiryWindowViewModel inquiryViewModel = new InquiryWindowViewModel(
                terminals,
                new List<CmdTypes>() { CmdTypes.downloadEmergencyPlaylist },
                new List<CmdTypes>() { CmdTypes.downloadEmergencyPlaylist },
                 PlatformConfig.LanguageResourcePath,
                _platformService.GetPlatformLanguage(),
                false);//Path.Combine(ClientManager.LanguageResourcePath, @"zh-CN\TerminalControl.zh-CN.resources")
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
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,e.Message);
                }));
            }
        }

        private void InquiryInstantHistory()
        {
            //Modify-lixc
            List<TerminalModel> terminals = new List<TerminalModel>();
            _viewModelLocator.TerminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

            try
            {
                InquiryWindowViewModel inquiryViewModel = new InquiryWindowViewModel(
                terminals,
                new List<CmdTypes>() { CmdTypes.emergencyMessage },
                new List<CmdTypes>() { CmdTypes.emergencyMessage },
                PlatformConfig.LanguageResourcePath,
                _platformService.GetPlatformLanguage(),
                false);//Path.Combine(ClientManager.LanguageResourcePath, @"zh-CN\TerminalControl.zh-CN.resources")
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
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,e.Message);
                }));
            }
        }

        public bool IsShowCommand
        {
            get
            {
                return _isShowCommand;
            }
            set
            {
                if (value == _isShowCommand)
                {
                    return;
                }
                _isShowCommand = value;
                RaisePropertyChanged("IsShowCommand");
            }
        }

        private void OpenPublishWizard()
        {
            WizardViewModel publishWizardViewModel = new WizardViewModel(null, _viewModelLocator.TerminalCollection.SelectedTerminals);
            Nova.NovaWeb.McGo.Platform.View.PublishWizardDialog publishWizardDialog = new Nova.NovaWeb.McGo.Platform.View.PublishWizardDialog();
            publishWizardViewModel.RequestClose += (s, i) =>
            {
                publishWizardDialog.Close();
                Messenger.Default.Send<string>(string.Empty, "CloseReadbackView");
            };
            publishWizardDialog.Owner = App.Current.MainWindow;
            publishWizardDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            publishWizardDialog.DataContext = publishWizardViewModel;
            publishWizardDialog.ShowInTaskbar = false;
            publishWizardDialog.ShowDialog();
        }

        private void RefreshTerminalList()
        {
            NotificationMessage<string> notification = new NotificationMessage<string>(string.Empty, "RefreshTerminal");
            Messenger.Default.Send<NotificationMessage<string>>(notification, "RefreshTerminal");
        }

        private void OpenEmergencyPlaylistView()
        {
            try
            {
                List<TerminalModel> terminals = new List<TerminalModel>();
                _viewModelLocator.TerminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

                Frm_EmergencyPlaylist frmPlaylist =
                new Frm_EmergencyPlaylist(
                    PlatformConfig.LanguageResourcePath,
                    _platformService.GetPlatformLanguage(),
                    terminals,
                    PlatformConfig.EmergencyListDirectoryPath);
                frmPlaylist.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                frmPlaylist.ShowInTaskbar = false;
                frmPlaylist.ShowDialog(new Wfp32Window(App.Current.MainWindow));
            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,e.Message);
                }));
            }
        }

        public void OpenInstantNotification()
        {
            try
            {
                //Modify-lixc
                List<TerminalModel> terminals = new List<TerminalModel>();
                _viewModelLocator.TerminalCollection.SelectedTerminals.ToList().ForEach(e => terminals.Add(e.CurrentTerminal));

                FrmSetEmergyMsg frmSetEmergyMsg = new FrmSetEmergyMsg(
                terminals,
                 PlatformConfig.LanguageResourcePath,
                    _platformService.GetPlatformLanguage());
                //frmSetEmergyMsg.FormClosing += frmSetEmergyMsg_FormClosing;
                frmSetEmergyMsg.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                frmSetEmergyMsg.SetScreenTopMost(false);
                frmSetEmergyMsg.ShowInTaskbar = false;
                frmSetEmergyMsg.ShowDialog(new Wfp32Window(App.Current.MainWindow));

            }
            catch (Exception e)
            {
                DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow,e.Message);
                }));
            }
        }
    }
}
