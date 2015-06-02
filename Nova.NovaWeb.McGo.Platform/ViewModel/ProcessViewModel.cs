using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public abstract class ProcessViewModel : ViewModelBase
    {

        private BackgroundWorker _worker;
        private int _iterations = 50;
        private int _progressPercentage = 0;
        private string _output;
        private bool _startEnabled = true;
        private bool _cancelEnabled = false;

        private bool _isOperationFinished = false;


        private ICommand _operationWorkCommand;


        public int Iterations
        {
            get { return _iterations; }
            set
            {
                if (_iterations != value)
                {
                    _iterations = value;
                    RaisePropertyChanged("Iterations");
                }
            }
        }

        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                if (_progressPercentage != value)
                {
                    _progressPercentage = value;
                    RaisePropertyChanged("ProgressPercentage");
                }
            }
        }

        public string Output
        {
            get { return _output; }
            set
            {
                if (_output != value)
                {
                    _output = value;
                    RaisePropertyChanged("Output");
                }
            }
        }

        public bool StartEnabled
        {
            get { return _startEnabled; }
            set
            {
                if (_startEnabled != value)
                {
                    _startEnabled = value;
                    RaisePropertyChanged("StartEnabled");
                }
            }
        }

        public bool CancelEnabled
        {
            get { return _cancelEnabled; }
            set
            {
                if (_cancelEnabled != value)
                {
                    _cancelEnabled = value;
                    RaisePropertyChanged("CancelEnabled");
                }
            }
        }


        public bool IsOperationFinished
        {
            get { return _isOperationFinished; }
            set
            {
                if (value != _isOperationFinished)
                {
                    _isOperationFinished = value;

                    RaisePropertyChanged("IsOperationFinished");
                }
            }
        }

        public ICommand RunOperationCommand
        {
            get
            {
                return _operationWorkCommand;
            }

        }


        public ProcessViewModel()
        {
            //_worker = new BackgroundWorker()
            //{
            //    WorkerReportsProgress = true,
            //    WorkerSupportsCancellation = true
            //};
            //_worker.DoWork += worker_DoWork;
            //_worker.ProgressChanged += worker_ProgressChanged;
            //_worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            Initilize();
        }

        protected void Initilize()
        {
            this._operationWorkCommand = new RelayCommand(() => _worker.RunWorkerAsync());

            _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += this.RunWorkerCompleted;
        }


        protected void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                Output = args.Error.Message;
            }
            else if (args.Cancelled)
            {
                Output = "Cancelled";
            }
            else
            {
                Output = args.Result.ToString();
                ProgressPercentage = 0;
            }
            StartEnabled = !_worker.IsBusy;
            CancelEnabled = _worker.IsBusy;

            IsOperationFinished = false;
            
            _operationWorkCommand = null;
            _worker.Dispose();
            _worker = null;
        }


        public abstract void DoOperation();


        public void CancelProcess()
        {
            _worker.CancelAsync();
        }


        #region BackgroundWorker Events

        // Note: This event fires on the background thread.
        protected void worker_DoWork(object sender, DoWorkEventArgs args)
        {
            IsOperationFinished = true;
            DoOperation();
        }

        // Note: This event fires on the UI thread.
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Output = e.Error.Message;
            }
            else if (e.Cancelled)
            {
                Output = "Cancelled";
            }
            else
            {
                Output = e.Result.ToString();
                ProgressPercentage = 0;
            }
            StartEnabled = !_worker.IsBusy;
            CancelEnabled = _worker.IsBusy;
        }

        // Note: This event fires on the UI thread.
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
            Output = (string)e.UserState;
        }

        #endregion
    }
}
