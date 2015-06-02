using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public abstract class PublishWizardPageViewModelBase : ViewModelBase
    {
        bool _isCurrentPage;
        private bool _isPublishPage = false;
        //private ObservableCollection<TerminalViewModel> _currentTerminals;
        //private ScheduleViewModel _currentSchedule;
        private PublishDataViewModel _publishDataInfo;

        protected PublishWizardPageViewModelBase()
        {
            
        }

        public abstract string DisplayName { get; }



        public bool IsCurrentPage
        {
            get { return _isCurrentPage; }
            set
            {
                if (value == _isCurrentPage) return;

                _isCurrentPage = value;
                RaisePropertyChanged("IsCurrentPage");
            }
        }

        public bool IsPublishPage
        {
            get { return _isPublishPage; }
            set
            {
                if (value == _isPublishPage)
                    return;

                _isPublishPage = value;
                RaisePropertyChanged("IsPublishPage");
            }
        }

        //public PublishDataViewModel PublishDataInfo
        //{
        //    get { return _publishDataInfo; }
        //    set
        //    {
        //        if (value == _publishDataInfo)
        //        {
        //            return;
        //        }
        //        _publishDataInfo = value;
        //        RaisePropertyChanged("PublishDataInfo");
        //    }
        //}
        internal abstract bool IsValid();
    }
}
