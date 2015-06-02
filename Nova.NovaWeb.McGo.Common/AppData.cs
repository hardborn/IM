using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Common
{
    [Serializable]
    public class AppData
    {
        public AppData()
        {
        }

        public string Token { get; set; }

        public bool IsAutoStatusRefresh { get; set; }

        public double RefreshFrequency { get; set; }

        public bool IsEnableFTPS { get; set; }

        public string TerminalSortField { get; set; }

        public Size ScheduleScreenSize { get; set; }

        
    }
}
