using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.DAL
{
    public class PlatformService: IPlatformService
    {

        public PlatformService()
        {
        }

        public Account GetPlatformAccount()
        {
            PlatformConfig platformConfig = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            if (platformConfig == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    var _log = AppEnvionment.Default.Get<ILog>();
                    _log.Log("Debug：获取平台配置失败{GetPlatformAccount}");
                }
                return null;
            }

           return platformConfig.PlatformAccount;
        }

        public ServerInfo GetPlatformServerInfo()
        {
            PlatformConfig platformConfig = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            if (platformConfig == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    var _log = AppEnvionment.Default.Get<ILog>();
                    _log.Log("Debug：获取平台配置失败{GetPlatformServerInfo}");
                }
                return null;
            }

            return platformConfig.ServerInfo;
        }


        public string GetPlatformLanguage()
        {
            PlatformConfig platformConfig = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            if (platformConfig == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    var _log = AppEnvionment.Default.Get<ILog>();
                    _log.Log("Debug：获取平台语言失败{GetPlatformLanguage}");
                }
                return null;
            }

            return platformConfig.PlatformLanguage;
        }


        public string GetWorkspacePath()
        {
            PlatformConfig platformConfig = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            if (platformConfig == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    var _log = AppEnvionment.Default.Get<ILog>();
                    _log.Log("Debug：获取平台工作路径失败{GetWorkspacePath}");
                }
                return null;
            }

            return platformConfig.WorkspacePath;
        }


        public AppData GetAppData()
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
            PlatformConfig platformConfig = AppEnvionment.Default["PlatformConfig"] as PlatformConfig;
            if (platformConfig == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    var _log = AppEnvionment.Default.Get<ILog>();
                    _log.Log("Debug：获平台数据失败{Save}");
                }
                return;
            }
            platformConfig.Save();
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
