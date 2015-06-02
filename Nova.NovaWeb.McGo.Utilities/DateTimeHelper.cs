using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Nova.NovaWeb.McGo.Utilities
{
    public class DateTimeHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime([In] ref SYSTEMTIME st);

        public static void SetSystemClock(short year, short month, short day, short hour, short min, short sec)
        {
            var st = new SYSTEMTIME();
            st.wYear = year; // must be short 
            st.wMonth = month;
            st.wDay = day;
            st.wHour = hour;
            st.wMinute = min;
            st.wSecond = sec;

            SetSystemTime(ref st);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        public static DateTime UTCToDateTime(double l)
        {
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            dtZone = dtZone.AddSeconds(l);
            return dtZone.ToLocalTime();
        }
      
        public static double DateTimeToUTC(DateTime vDate)
        {
            vDate = vDate.ToUniversalTime();
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            return vDate.Subtract(dtZone).TotalSeconds;
        }

    }
}
