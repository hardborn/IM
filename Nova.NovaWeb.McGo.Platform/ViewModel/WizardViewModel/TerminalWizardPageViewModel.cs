using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Nova.Globalization;
using Nova.NovaWeb.McGo.BLL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class TerminalWizardPageViewModel : PublishWizardPageViewModelBase
    {
        private ObservableCollection<TerminalViewModel> _allTerminalList = new ObservableCollection<TerminalViewModel>();
        private ObservableCollection<TerminalViewModel> _selectedTerminalList = new ObservableCollection<TerminalViewModel>();
        private ScheduleViewModel _schedule;
        private ICommand _selectionChangedCommand;


        public TerminalWizardPageViewModel(ScheduleViewModel schedule)
        {
            //_schedule = schedule;
            //InitialPublishData();
            //GetTerminalList(schedule);
        }

        private void GetTerminalList()
        {
            TerminalRepositoryViewModel terminalRepositoryViewModel = AppEnvionment.Default.Get<TerminalRepositoryViewModel>();
            if (PublishDataViewModel.Instance.Schedule.Type == NovaWeb.Common.ScheduleType.Common)
            {
                if (PublishDataViewModel.Instance.Schedule.RemoteInfo != null && string.IsNullOrEmpty(PublishDataViewModel.Instance.Schedule.RemoteInfo.OptimizedScheduleName))
                {
                    AllTerminalList = new ObservableCollection<TerminalViewModel>(terminalRepositoryViewModel.TerminalList.Where(t => t.Type == Common.TerminalType.PC));
                }
                else
                AllTerminalList = new ObservableCollection<TerminalViewModel>(terminalRepositoryViewModel.TerminalList);
            }
            else if (PublishDataViewModel.Instance.Schedule.Type == NovaWeb.Common.ScheduleType.PC)
            {
                AllTerminalList = new ObservableCollection<TerminalViewModel>(terminalRepositoryViewModel.TerminalList.Where(t => t.Type == Common.TerminalType.PC));
            }
        }

        public TerminalWizardPageViewModel()
        {
            //_selectedTerminalList.CollectionChanged += _selectedTerminalList_CollectionChanged;
            GetTerminalList();
        }

        //void _selectedTerminalList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    //if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        //    //{
        //        _publishDataViewModel.PublishTerminalInfoList = new ObservableCollection<PublishTerminalInfo>(e.NewItems as List<PublishTerminalInfo>);
        //  //  }
        //}


        public ObservableCollection<TerminalViewModel> SelectedTerminalList
        {
            get { return _selectedTerminalList; }
            set
            {
                if (_selectedTerminalList == value)
                {
                    return;
                }
                _selectedTerminalList = value;
                RaisePropertyChanged("SelectedTerminalList");
            }
        }

        public ObservableCollection<TerminalViewModel> AllTerminalList
        {
            get { return _allTerminalList; }
            set
            {
                if (_allTerminalList == value)
                {
                    return;
                }
                _allTerminalList = value;
                RaisePropertyChanged("AllTerminalList");
            }
        }


        public override string DisplayName
        {
            get {
                string info;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_UI_TerminalList", out info);
                return info;
            }
        }

        internal override bool IsValid()
        {
            if (_selectedTerminalList != null && _selectedTerminalList.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ICommand SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    _selectionChangedCommand = new RelayCommand<object>((T) => TerminalSelectionChaned(T), null);
                }
                return _selectionChangedCommand;
            }
        }


        //public void InitialPublishData()
        //{
        //    PublishDataInfo = new PublishDataViewModel();
        //    PublishDataInfo.Schedule = _schedule;
        //}

        private void TerminalSelectionChaned(object sender)
        {
            SelectionChangedEventArgs e = sender as SelectionChangedEventArgs;
            if (e == null)
            {
                return;
            }
            DataGrid dataGrid = e.Source as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            //if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 1)
            //{
            //    dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            //}
            //else
            //{
            //    dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            //}
            
            if(e.AddedItems != null)
            {
                foreach (TerminalViewModel item in e.AddedItems)
                {
                    if (!_selectedTerminalList.Contains(item))
                    {
                        //item.PublishingSchedule = _schedule;
                        _selectedTerminalList.Add(item);
                        PublishDataViewModel.Instance.PublishTerminalInfoList.Add(new PublishTerminalInfo() { TerminalInfo = item });
                    }
                }
            }

            if(e.RemovedItems != null && e.RemovedItems.Count != _selectedTerminalList.Count)
            {
                foreach (TerminalViewModel item in e.RemovedItems)
                {
                    if (_selectedTerminalList.Contains(item) )
                    {
                        //item.PublishingSchedule = _schedule;
                        _selectedTerminalList.Remove(item);

                        var firstInfo = PublishDataViewModel.Instance.PublishTerminalInfoList.FirstOrDefault(t => t.TerminalInfo.Mac == item.Mac);
                        PublishDataViewModel.Instance.PublishTerminalInfoList.Remove(firstInfo);
                    }
                }
            }
        
            e.Handled = true;
        }
    }
}

