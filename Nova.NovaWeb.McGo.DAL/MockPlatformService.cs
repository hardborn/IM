using System;
using System.Collections.Generic;
using System.Linq;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;

namespace Nova.NovaWeb.McGo.DAL
{
    public class MockPlatformService : IPlatformService
    {
        public Common.Account GetPlatformAccount()
        {
            return new Account(){Name = "user" , Password = "123456"};
        }

        public Common.ServerInfo GetPlatformServerInfo()
        {
            return new ServerInfo() { CustomerId = "Deploy",ServerAddress = "http://192.168.0.99:81"};
        }

        public string GetPlatformLanguage()
        {
            return "zh-cn";
        }

        public string GetWorkspacePath()
        {
            return string.Empty;
        }

        public Common.AppData GetAppData()
        {
            PlatformConfig platformConfig = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            if (platformConfig == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    var _log = AppEnvionment.Default.Get<ILog>();
                    _log.Log("Debug：获取平台数据失败{GetAppData}");
                }
                return null;
            }

            return platformConfig.AppDataInfo;
        }

        public void Save()
        {
            //throw new NotImplementedException();
        }


        public TimeZoneTable GetTimeZoneTable()
        {
            PlatformConfig platformConfig = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            if (platformConfig == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    var _log = AppEnvionment.Default.Get<ILog>();
                    _log.Log("Debug：获取平台数据失败{GetTimeZoneTable}");
                }
                return null;
            }
            return platformConfig.TimeZoneTable;
        }
    }
}
