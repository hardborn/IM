using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Nova.NovaWeb.McGo.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class QuitViewModel : ViewModelBase
    {
        public QuitViewModel()
        {
           
        }

        private RelayCommand<object> _logoutCommand; 
        private RelayCommand<object> _exitCommand;
        private QuitEnum _model = QuitEnum.Cancel;
        public QuitEnum Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (value == _model)
                    return ;
                _model = value;
                RaisePropertyChanged("Model");
            }
        }

        public RelayCommand<object> LogoutCommand
        {
            get
            {
                if(_logoutCommand == null)
                {
                    _logoutCommand = new RelayCommand<object>((o) =>
                    {
                        
                        var modalWindow = o as IModalWindow;
                        Model = QuitEnum.Logout;
                        modalWindow.Close();
                    }, 
                    null);
                }
                return _logoutCommand;
            }
        }

        public RelayCommand<object> ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new RelayCommand<object>((o) =>
                    {
                        var modalWindow = o as IModalWindow;
                        Model = QuitEnum.Exit;
                        modalWindow.Close();
                    },
                    null);
                }
                return _exitCommand;
            }
        }
    }

    public enum QuitEnum
    {
        Logout,
        Exit,
        Cancel
    }
}
