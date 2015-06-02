using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Nova.Globalization;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class ConfigWorkspaceViewModel : ViewModelBase, IDataErrorInfo
    {
        private bool _isAutoStatusRefresh;
        private IPlatformService _platformService;


        public ConfigWorkspaceViewModel(IPlatformService service)
        {
            _platformService = service;
            _workspacePath = _platformService.GetWorkspacePath();
            _isAutoStatusRefresh = _platformService.GetAppData().IsAutoStatusRefresh;
            _refreshFrequency = (int)_platformService.GetAppData().RefreshFrequency;
        }


        private string _workspacePath;

        public string WorkspacePath
        {
            get
            {
                return _workspacePath;
            }
            set
            {
                if (_workspacePath == value)
                    return;
                _workspacePath = value;
                RaisePropertyChanged("WorkspacePath");
            }
        }

        private int _refreshFrequency;

        public int RefreshFrequency
        {
            get
            {
                return _refreshFrequency;
            }
            set
            {
                if (_refreshFrequency == value)
                    return;
                _refreshFrequency = value;
                RaisePropertyChanged("RefreshFrequency");
            }
        }

        public bool IsAutoStatusRefresh
        {
            get
            {
                return _isAutoStatusRefresh;
            }
            set
            {
                if (_isAutoStatusRefresh == value)
                    return;
                _isAutoStatusRefresh = value;
                RaisePropertyChanged("IsAutoStatusRefresh");
                RaisePropertyChanged("RefreshFrequency");
            }
        }

        private RelayCommand<object> _openFileCommand;

        public RelayCommand<object> OpenFileCommand
        {
            get
            {
                if (_openFileCommand == null)
                {
                    _openFileCommand = new RelayCommand<object>((o) =>
                    {
                        OpenFileView(o);
                    },
                        null);
                }
                return _openFileCommand;
            }
        }

        private RelayCommand<object> _okButtonCommand;

        public RelayCommand<object> OkButtonCommand
        {
            get
            {
                if (_okButtonCommand == null)
                {
                    _okButtonCommand = new RelayCommand<object>((t) =>
                    {
                        OKSubmit(t);
                    },
                    (t) =>
                    {
                        if (Directory.Exists(WorkspacePath) && CanApply)
                            return true;
                        else
                            return false;
                    });
                }
                return _okButtonCommand;
            }
        }


        private RelayCommand<object> _cancelButtonCommand;

        public RelayCommand<object> CancelButtonCommand
        {
            get
            {
                if (_cancelButtonCommand == null)
                {
                    _cancelButtonCommand = new RelayCommand<object>((t) =>
                    {
                        CancelConfig(t);
                    },
                    null);
                }
                return _cancelButtonCommand;
            }
        }


        private void OpenFileView(object o)
        {
            Nova.NovaWeb.McGo.Platform.View.ConfigWorkspaceView workspaceView = o as Nova.NovaWeb.McGo.Platform.View.ConfigWorkspaceView;
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = _workspacePath;
            System.Windows.Interop.HwndSource source = System.Windows.PresentationSource.FromVisual(workspaceView) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);
            bool canWrite = true;
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    File.Create(dlg.SelectedPath + "\\new123.tmp");
                }
                catch(Exception e)
                {
                    canWrite = false;
                }

                if(canWrite == false)
                {
                    DispatcherHelper.UIDispatcher.BeginInvoke((Action)(() =>
                    {
                        string errorInfo;
                        MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_UnauthorizedAccessException", out errorInfo);
                        MessageBox.Show(errorInfo);
                    }));
                }
                else
                {

                    //File.SetAttributes(WorkspacePath + "\\new123.tmp", FileAttributes.Hidden);
                    WorkspacePath = dlg.SelectedPath;
                }
            }
        }

        private void OKSubmit(object t)
        {
            var platformConfig = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            platformConfig.WorkspacePath = WorkspacePath;
            platformConfig.AppDataInfo.IsAutoStatusRefresh = IsAutoStatusRefresh;
            if(IsAutoStatusRefresh)
                platformConfig.AppDataInfo.RefreshFrequency = RefreshFrequency;
            Messenger.Default.Send<NotificationMessage<AppData>>(new NotificationMessage<AppData>(platformConfig.AppDataInfo, "RefreshFrequency"), "RefreshFrequency");
            platformConfig.Save();

            var modalWindow = t as IModalWindow;
            modalWindow.Close();
        }

        private void CancelConfig(object t)
        {
            var modalWindow =  t as IModalWindow;
            modalWindow.Close();
        }



        private bool _canApply;
        /// <summary>
        /// 是否通过验证并使能确定按钮
        /// </summary>
        public bool CanApply
        {
            get
            {
                return _canApply;
            }
            set
            {
                if (_canApply != value)
                {
                    _canApply = value;
                    RaisePropertyChanged("CanApply");
                }
            }
        }

        #region Validate 验证

        static readonly string[] ValidatedProperties = { "RefreshFrequency"
        };

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                return GetValidationError(columnName);
            }
        }

        protected string[] _error;

        private string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string result = null;

            if (_error == null)
                _error = new string[1];

            switch (propertyName)
            {
                case "RefreshFrequency":
                    result = this.ValidateRefreshFrequency(this.RefreshFrequency);
                    _error[0] = result;
                    break;
                default:
                    break;
            }

            CanApply = ValidateFields(_error);

            return result;
        }

        internal static bool ValidateFields(string[] value)
        {
            foreach (string s in value)
                if (!string.IsNullOrEmpty(s))
                    return false;
            return true;
        }

        private string ValidateRefreshFrequency(double refreshFrequency)
        {
            if(!_isAutoStatusRefresh)
                return null;
            if (refreshFrequency < 10 || refreshFrequency > 3600)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_RefreshFrequencyError1", out errorInfo);
                return errorInfo;
            }
            return null;
        }


        #endregion


    }

    public class OldWindow : System.Windows.Forms.IWin32Window
    {
        IntPtr _handle;
        public OldWindow(IntPtr handle)
        {
            _handle = handle;
        }

        #region IWin32Window Members

        IntPtr System.Windows.Forms.IWin32Window.Handle
        {
            get { return _handle; }
        }

        #endregion
    }
}
