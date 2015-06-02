using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface IPlatformService
    {
        Account GetPlatformAccount();
        ServerInfo GetPlatformServerInfo();
        string GetPlatformLanguage();
        string GetWorkspacePath();
        AppData GetAppData();
        TimeZoneTable GetTimeZoneTable();
        void Save();
    }
}
