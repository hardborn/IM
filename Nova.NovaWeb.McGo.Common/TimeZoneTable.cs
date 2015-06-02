using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeZone = Nova.NovaWeb.TimeZoneParser;
using TimeZoneInfo = Nova.NovaWeb.Common.TimeZoneInfo;
using System.Threading;
using Nova.Systems;

namespace Nova.NovaWeb.McGo.Common
{
    [Serializable]
    public class TimeZoneTable
    {
        public delegate void InvalidTimeZoneEventHandler(object serder, EventArgs e);
        public event InvalidTimeZoneEventHandler InvalidTimeZoneEvent;

        public TimeZoneTable()
        {
            var timeZones =  TimeZone.GetTimeZones();
            if (timeZones == null)
                return ;
            foreach (var item in timeZones.Keys)
            {
                // var timeZoneItem = FromTimeZoneInformation(timeZones[item]);
                _timeZoneInfoList.Add(new TimeZoneInformation(timeZones[item]));
            }
        }

        [NonSerialized]
        private List<TimeZoneInformation> _timeZoneInfoList = new List<TimeZoneInformation>();
        public List<TimeZoneInformation> TimeZoneInfoList
        {
            get
            {
                return _timeZoneInfoList;
            }
            set
            {
                _timeZoneInfoList = value;
            }
        }

        public TimeZoneInformation GetTimeZoneInfoById(string id)
        {
            if (_timeZoneInfoList == null)
                return  null;
            var info = _timeZoneInfoList.FirstOrDefault(t => t.Id == id);
            if (info == null)
            {
                if (InvalidTimeZoneEvent != null)
                    InvalidTimeZoneEvent(this, new EventArgs());
            }
            return info;
        }

        //private TimeZoneInfo FromTimeZoneInformation(Nova.Systems.TimeZone.TimeZoneInformation timeZoneInformation)
        //{
        //    TimeZoneInfo result = new TimeZoneInfo();
        //    result.Bias = timeZoneInformation.bias;
        //    result.DaylightBias = timeZoneInformation.daylightBias;
        //    result.DaylightDate = ConverteSYSTEMTIME(timeZoneInformation.daylightDate);
        //    result.DaylightName = timeZoneInformation.daylightName;
        //    result.DisplayName = timeZoneInformation.displayName;
        //    result.Id = timeZoneInformation.id;
        //    result.StandardBias = timeZoneInformation.standardBias;
        //    result.StandardDate = ConverteSYSTEMTIME(timeZoneInformation.standardDate);
        //    result.StandardName = timeZoneInformation.standardName;
        //    return result;
        //}

        //private string ConverteSYSTEMTIME(TimeZone.SYSTEMTIME systemTime)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    builder.AppendFormat("{0}+{1}+{2}+{3}+{4}+{5}+{6}+{7}", 
        //                              systemTime.wYear, 
        //                              systemTime.wMonth, 
        //                              systemTime.wDayOfWeek, 
        //                              systemTime.wDay, 
        //                              systemTime.wHour, 
        //                              systemTime.wMinute, 
        //                              systemTime.wSecond, 
        //                              systemTime.wMilliseconds);
        //    return builder.ToString();
        //}
    }

    public class TimeZoneInformation
    {
        private string _displayName;
        private string _id;
        private int _daylightBias;
        private SYSTEMTIME _daylightDate;
        private string _daylightName;
        private int _standardBias;
        private SYSTEMTIME _standardDate;
        private string _standardName;
        private int _bias;

        public TimeZoneInformation(Nova.Systems.TimeZoneInformation timeZoneInfo)
        {
            _bias = timeZoneInfo.bias;
            _displayName = timeZoneInfo.displayName;
            _id = timeZoneInfo.id;
            _daylightBias = timeZoneInfo.daylightBias;
            _daylightDate = timeZoneInfo.daylightDate;
            _daylightName = timeZoneInfo.daylightName;
            _standardBias = timeZoneInfo.standardBias;
            _standardDate = timeZoneInfo.standardDate;
            _standardName = timeZoneInfo.standardName;
        }

        public int Bias
        {
            get
            {
                return _bias;
            }
            set
            {
                _bias = value;
            }
        }

        public string StandardName
        {
            get
            {
                return _standardName;
            }
            set
            {
                _standardName = value;
            }
        }

        public SYSTEMTIME StandardDate
        {
            get
            {
                return _standardDate;
            }
            set
            {
                _standardDate = value;
            }
        }
        public int StandardBias
        {
            get
            {
                return _standardBias;
            }
            set
            {
                _standardBias = value;
            }
        }

        public string DaylightName
        {
            get
            {
                return _daylightName;
            }
            set
            {
                _daylightName = value;
            }
        }

        public SYSTEMTIME DaylightDate
        {
            get
            {
                return _daylightDate;
            }
            set
            {
                _daylightDate = value;
            }
        }

        public int DaylightBias
        {
            get
            {
                return _daylightBias;
            }
            set
            {
                _daylightBias = value;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }

        private DateTime ConvertToDST(DateTime date)
        {
            DateTime begDST = new DateTime(date.Year, 1, 1);
            if (_standardDate.wMonth != 0)
                begDST.AddMonths(_standardDate.wMonth - 1);
            if (_standardDate.wDay != 0) begDST.AddDays((_standardDate.wDay - 1) * 7);
            while (true)
            {
                if (_standardDate.wDayOfWeek == (int)begDST.DayOfWeek)
                    break;
                begDST = begDST.AddDays(1);
            }
            DateTime endDST = new DateTime(DateTime.Now.Year, 1, 1);
            if (_daylightDate.wMonth != 0)
                endDST.AddMonths(_daylightDate.wMonth - 1);
            if (_daylightDate.wDay != 0) endDST.AddDays((_daylightDate.wDay - 1) * 7);
            while (true)
            {
                if (_daylightDate.wDayOfWeek == (int)endDST.DayOfWeek)
                    break;
                endDST = endDST.AddDays(1);
            }

            if (begDST.DayOfWeek == DayOfWeek.Sunday)
                begDST = begDST.AddDays(7);
            else
                begDST = new DateTime(DateTime.Now.Year, 3, 15 - (int)begDST.DayOfWeek);

            if (endDST.DayOfWeek != DayOfWeek.Sunday)
                endDST = new DateTime(DateTime.Now.Year, 11, 8 - (int)endDST.DayOfWeek);

            if (date > begDST && date < endDST)
            {
                return date.AddHours(1);
            }
            return date;
        }
        public DateTime GetLocalTime(DateTime utcTime, bool isEnableDaylight)
        {
            if (!isEnableDaylight)
            {
                return utcTime.Subtract(new TimeSpan(0, _bias, 0));
            }
            return ConvertToDST(utcTime.Subtract(new TimeSpan(0, _bias, 0)));
        }
        public DateTime GetSystemTime(DateTime localTime, bool isEnableDaylight)
        {
            if (!isEnableDaylight)
            {
                return localTime.AddMinutes(_bias);
            }
            return ConvertToDST(localTime.AddMinutes(_bias));
        }
    }
}
