using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.Model
{
    [Serializable]
    public class AppInfo
    {
        public AppInfo()
        {
        }

        public Account FTPServiceAccount { get; set; }

        public Account PlatformAccount { get; set; }

        public bool EnableSSL { get; set; }

    }
}
