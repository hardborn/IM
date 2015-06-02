using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class DialogViewModelBase: ViewModelBase
    {

        private bool? _dialogResult = false;

        private RelayCommand _okCommand;
        private RelayCommand _cancleCommand;
        private RelayCommand _closeCommand;


        public RelayCommand OKCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(() => Close());
                }
                return _okCommand;
            }
        }

        public RelayCommand CancleCommand
        {
            get
            {
                if (_cancleCommand == null)
                {
                    _cancleCommand = new RelayCommand(() => Close());
                }
                return _cancleCommand;
            }
        }

        public RelayCommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(() => Close());
                }
                return _closeCommand;
            }
        
        }

        public Guid MessageID
        {
            get;
            private set;
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }

            set
            {
                if (_dialogResult == value)
                    return;

                _dialogResult = value;

                RaisePropertyChanged("DialogResult");
            }
        }


        public DialogViewModelBase()
        {
            MessageID = Guid.NewGuid();

            _closeCommand = new RelayCommand(() => Close());
            _okCommand = new RelayCommand(() => Close());
            _cancleCommand = new RelayCommand(() => Close());
        }

        protected virtual void Close()
        {
            this.DialogResult = false;
        }            

        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
