using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.Common;
using TimeZoneInfo = Nova.NovaWeb.Common.TimeZoneInfo;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class PublishDataViewModel : ViewModelBase
    {
        private static PublishDataViewModel uniqueInstance;
        private static readonly object locker = new object();

        private PublishDataViewModel()
        {
        }

        public static PublishDataViewModel Instance
        {
            get
            {
                if (uniqueInstance == null)
                {
                    lock (locker)
                    {
                        if (uniqueInstance == null)
                        {
                            uniqueInstance = new PublishDataViewModel();
                        }
                    }
                }
                return uniqueInstance;
            }
        }

        private ScheduleViewModel _schedule;
        private TimeZoneInfo _timeZone;
        private ObservableCollection<PublishTerminalInfo> _publishInfoList = new ObservableCollection<PublishTerminalInfo>();
        private Common.PublishStatus _currentPublishStatus;

        public ScheduleViewModel Schedule
        {
            get
            {
                return _schedule;
            }
            set
            {
                if (_schedule == value)
                {
                    return;
                }
                _schedule = value;
                RaisePropertyChanged("Schedule");
            }
        }
        public ObservableCollection<PublishTerminalInfo> PublishTerminalInfoList
        {
            get
            {
                return _publishInfoList;
            }
            set
            {
                if (_publishInfoList == value)
                    return;
                _publishInfoList = value;
                //if(_publishInfoList != null)
                //{
                //    foreach (var item in _publishInfoList)
                //    {
                //        item.OnResultChanged += item_OnResultChanged;
                //    }
                //}
                RaisePropertyChanged("PublishTerminalInfoList");
            }
        }

        //void item_OnResultChanged(object sender, ResultEventArgs e)
        //{
        //   if(e == null)
        //        return;
            
        //    bool result = true;
        //   foreach (var item in _publishInfoList)
        //   {
        //        result &= item.PublishResult; 
        //   }
        //    CurrentPublishStatus = result?Common.PublishStatus.PublishSuccess:Common.PublishStatus.PublishFailed;
        //}
        public TimeZoneInfo TimeZone
        {
            get
            {
                return _timeZone;
            }
            set
            {
                if (_timeZone == value)
                    return;
                _timeZone = value;
                RaisePropertyChanged("TimeZone");
            }
        }
        

        public Common.PublishStatus CurrentPublishStatus
        {
            get
            {
                return _currentPublishStatus;
            }
            internal set
            {
                if (_currentPublishStatus == value)
                    return;
                _currentPublishStatus = value;
                AppMessages.PublishStatusMessage.Send(_currentPublishStatus);
                RaisePropertyChanged("CurrentPublishStatus");
            }
        }

        public bool IsIncludingTerminals(TerminalType tType)
        {
            if (_publishInfoList == null || _publishInfoList.Count == 0)
                return false;
            else
            {
                foreach (PublishTerminalInfo pubInfo in _publishInfoList)
                    if (pubInfo.TerminalInfo != null && pubInfo.TerminalInfo.Type == tType)
                        return true;
                return false;
            }
        }

        public void ResetData()
        {
            //_isEnableTimeZone = false;
            _schedule = null;
            _timeZone = null;
            _publishInfoList = new ObservableCollection<PublishTerminalInfo>();
            uniqueInstance = null;
        }
    }
    public class PublishTerminalInfo : ViewModelBase
    {
        private TimeZoneInfo _timeZone;
        private TerminalViewModel _ter;
        private PublishSettingInfo _pubInfo = new PublishSettingInfo();
       private PublishResult _pubResult = new PublishResult();
        private ListTransmiteRes _transmiteResult ;
        private CommandSendResult _commandSendResult ;
        private int _progressPercentage;
        private int _dataItemCount = 0;
        private int _currentDataItemIndex = 0;
        private bool _result = false;
        private bool _finishedAndSendOK = false;
        public event EventHandler<ResultEventArgs> OnResultChanged;


        //public bool PublishResult
        //{
        //    get
        //    {
        //        return _result;
        //    }
        //    set
        //    {
        //        if(_result == value)
        //            return;
        //        _result = value;
        //        if (OnResultChanged != null)
        //            OnResultChanged(this, new ResultEventArgs(_result));
        //        RaisePropertyChanged("PublishResult");
        //    }
        //}

        /// <summary>
        /// 发送完成且命令发送成功的标志位，防止停止后重复发送
        /// </summary>
        public bool FinishedAndSendOK
        {
            get
            {
                return _finishedAndSendOK;
            }
            set
            {
                if (_finishedAndSendOK == value)
                    return;
                _finishedAndSendOK = value;
                RaisePropertyChanged("FinishedAndSendOK");
            }
        }


        public TerminalViewModel TerminalInfo
        {
            get
            {
                return _ter;
            }
            set
            {
                if (_ter == value)
                    return;
                _ter = value;
                RaisePropertyChanged("TerminalInfo");
            }
        }

        public PublishSettingInfo PubSettingInfo
        {
            get
            {
                return _pubInfo;
            }
            set
            {
                if (_pubInfo == value)
                    return;
                _pubInfo = value;
                RaisePropertyChanged("PubSettingInfo");
            }
        }

        public PublishResult PubResult
        {
            get
            {
                return _pubResult;
            }
            set
            {
                if (_pubResult == value)
                {
                    return;
                }
                _pubResult = value;
                RaisePropertyChanged("PubResult");
            }
        }

        //public ListTransmiteRes TransmiteResult
        //{
        //    get
        //    {
        //        return _transmiteResult;
        //    }
        //    set
        //    {
        //        if (value == _transmiteResult)
        //        {
        //            return;
        //        }
        //        _transmiteResult = value;
               
        //        RaisePropertyChanged("TransmiteResult");
        //    }
        //}

        //public CommandSendResult CommandSendResult
        //{
        //    get
        //    {
        //        return _commandSendResult;
        //    }
        //    set
        //    {
        //        if (value == _commandSendResult)
        //        {
        //            return;
        //        }
        //        _commandSendResult = value;
        //        RaisePropertyChanged("CommandSendResult");
        //    }
        //}


        public int ProgressPercentage
        {
            get
            {
                return _progressPercentage;
            }
            set
            {
                /*防止传输过程中进度大于100，强制置为100*/
                if(value > 100)
                {
                    value = 100;
                }

                if (_progressPercentage == value)
                    return;
                _progressPercentage = value;
                RaisePropertyChanged("ProgressPercentage");
            }
        }

        public int DataItemCount
        {
            get
            {
                return _dataItemCount;
            }
            set
            {
                if (_dataItemCount == value)
                    return;
                _dataItemCount = value;
                RaisePropertyChanged("DataItemCount");
            }
        }

        public int CurrentDataItemIndex
        {
            get
            {
                return _currentDataItemIndex;
            }
            set
            {
                if (_currentDataItemIndex == value)
                    return;
                _currentDataItemIndex = value;
                RaisePropertyChanged("CurrentDataItemIndex");
            }
        }
    }

    public class ResultEventArgs : EventArgs
    {
        public ResultEventArgs(bool result)
        {
            this.Result = result;
        }

        public bool Result { get; set; }

    }	
    public class PublishSettingInfo : ViewModelBase
    {
        private CmdModeTypes _cmdMode = CmdModeTypes.Default;
        private CmdTypes _cmdType = CmdTypes.downloadPlaySchedule;
        private TimeZoneInformation _timeZoneInfo;
        private DateTime? _executeTime;
        private DateTime? _overdueTime;
        private DateTime _publishTime;

        public PublishSettingInfo()
        {
        }

        public CmdModeTypes CmdMode
        {
            get
            {
                return _cmdMode;
            }
            set
            {
                if(value == _cmdMode)
                    return;
                _cmdMode = value;
                RaisePropertyChanged("CmdMode");
            }
        }

        public CmdTypes CmdType
        {
            get
            {
                return _cmdType;
            }
            set
            {
                if (value == _cmdType)
                    return;
                _cmdType = value;
                RaisePropertyChanged("CmdType");
            }
        }

        public TimeZoneInformation TimeZoneInfo
        {
            get
            {
                return _timeZoneInfo;
            }
            set
            {
                if(value == _timeZoneInfo)
                    return;
                _timeZoneInfo = value;
                RaisePropertyChanged("TimeZoneInfo");
            }
        }

        public DateTime? ExecuteTime
        {
            get
            {
                return _executeTime;
            }
            set
            {
                if(value == _executeTime)
                    return;
                _executeTime = value;
                RaisePropertyChanged("ExecuteTime");
            }
        }

        public DateTime? OverdueTime
        {
            get
            {
                return _overdueTime;
            }
            set
            {
                if(value == _overdueTime)
                    return;
                _overdueTime = value;
                RaisePropertyChanged("OverdueTime");
            }
        }

        public DateTime PublishTime
        {
            get
            {
                return _publishTime;
            }
            set
            {
                if(value == _publishTime)
                    return;
                _publishTime = value;
                RaisePropertyChanged("PublishTime");
            }
        }
    }

    public class PublishResult : ViewModelBase
    {
        private ListTransmiteRes? _transmiteResult = null;
        private CommandSendResult? _commandSendResult = null;

        public PublishResult()
        {
        }

        //public PublishResult(ListTransmiteRes? transmiteResult, CommandSendResult? commandSendResult)
        //{
        //    TransmiteResult = transmiteResult;
        //    CommandSendResult = commandSendResult;
        //}
        public ListTransmiteRes? TransmiteResult
        {
            get
            {
                return _transmiteResult;
            }
            set
            {
                if (value == _transmiteResult)
                {
                    return;
                }
                _transmiteResult = value;
                RaisePropertyChanged("TransmiteResult");
            }
        }

        public CommandSendResult? CommandSendResult
        {
            get
            {
                return _commandSendResult;
            }
            set
            {
                if (value == _commandSendResult)
                {
                    return;
                }
                _commandSendResult = value;
                RaisePropertyChanged("CommandSendResult");
            }
        }

        public bool MyProperty { get; set; }
    }

    public enum CommandSendResult
    {
        SendOk,
        SendNone,
        SendFailure
    }
}
