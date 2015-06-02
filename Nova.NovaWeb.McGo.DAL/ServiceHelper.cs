using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.OperationCommon;
using Nova.NovaWeb.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Serialization;
using Nova.Security;

namespace Nova.NovaWeb.McGo.DAL
{
    public class ServiceHelper
    {
        private static string PhpConfigPath = String.Format(@"{0}\MC-go\PhpConfig.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        private static IPlatformService _platformService;
        private static ILog _log;

        public static PhpServiceInterface PhpServiceInterface;



        static ServiceHelper()
        {
            _log = AppEnvionment.Default.Get<ILog>();
            _platformService = AppEnvionment.Default.Get<IPlatformService>();

            bool bPhpSuccess;
            PhpConfigParser phpConfigParser = new PhpConfigParser(PhpConfigPath, out bPhpSuccess);
            if (bPhpSuccess)
            {
                PhpServiceInterface = new PhpServiceInterface(phpConfigParser);
                if (_log != null)
                    _log.Log("Log：成功获取PHP服务页面列表");
            }
            else
            {
                if (AppDomain.CurrentDomain.GetData("IsDebugModel") != null && (bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    if (_log != null)
                        _log.Log("Debug：获取PHP服务页面列表失败");
                }
            }

            if (_platformService == null)
            {
                if (AppDomain.CurrentDomain.GetData("IsDebugModel") != null && (bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    if (_log != null)
                        _log.Log("Debug：无法获取服务器信息服务{serverInfoProvider}");
                }
            }
        }

        /// <summary>
        /// 获取当前PHP服务页面URL
        /// </summary>
        /// <param name="webServerAddress"></param>
        /// <param name="phpPageName"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public static string GetCurrentPhpServiceURL(string webServerAddress, ProtocolID protocolId, string customerId)
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();
            AppData appData = _platformService.GetAppData();
            if (serverInfo == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    if (_log != null)
                        _log.Log("Debug：无法获取服务器信息服务{serverInfoProvider}");
                }
                return string.Empty;
            }
            string phpServiceURL = string.Empty;
            //http://localhost:8080/NovaCloud/Deploy/index.php/API/Index/index?id=101
            //if (string.IsNullOrEmpty(serverInfo.CustomerId))
            //{
            //    phpServiceURL = phpPageName.Replace(PhpServiceInterface.UserIdentifierName + "/", string.Empty);
            //}
            //else
            //{
            //    phpServiceURL = phpPageName.Replace(PhpServiceInterface.UserIdentifierName, customerId);
            //}

            return String.Format("{0}/{1}/index.php/API/Index/index?id={2}&token={3}", webServerAddress, customerId, (int)protocolId, appData.Token);
        }


        public static string GetCurrentPhpServiceURL(ProtocolID protocolId)
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();
            return GetCurrentPhpServiceURL(serverInfo.ServerAddress, protocolId, serverInfo.CustomerId);
        }


        public static string GetCurrentPhpServiceURL()
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();
            AppData appData = _platformService.GetAppData();
            if (serverInfo == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    if (_log != null)
                        _log.Log("Debug：无法获取服务器信息服务{serverInfoProvider}");
                }
                return string.Empty;
            }
            return String.Format("{0}/{1}/index.php/API/Index/index?token={2}", serverInfo.ServerAddress, serverInfo.CustomerId, appData.Token);
        }

        public static string GetCurrentPhpServiceURL(string webServerAddress, string phpPageName, string customerId)
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();

            if (serverInfo == null)
            {
                if ((bool)AppDomain.CurrentDomain.GetData("IsDebugModel"))
                {
                    if (_log != null)
                        _log.Log("Debug：无法获取服务器信息服务{serverInfoProvider}");
                }
                return string.Empty;
            }

            string phpServiceURL = string.Empty;

            if (string.IsNullOrEmpty(serverInfo.CustomerId))
            {
                phpServiceURL = phpPageName.Replace(PhpServiceInterface.UserIdentifierName + "/", string.Empty);
            }
            else
            {
                phpServiceURL = phpPageName.Replace(PhpServiceInterface.UserIdentifierName, customerId);
            }

            return String.Format("{0}/{1}", webServerAddress, phpServiceURL);
        }


        public static string GetCurrentPhpServiceURL(string phpPageName)
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();
            return GetCurrentPhpServiceURL(serverInfo.ServerAddress, phpPageName, serverInfo.CustomerId);
        }

        /// <summary>
        /// 获取终端列表数据请求的Post数据
        /// </summary>
        /// <returns></returns>
        public static string GetTerminalListRequestPostData()
        {
            string requestStr = string.Empty;

            SiteListInquire siteListInquire = GetSiteListInquire();

            string siteListInquireStr = SerializeObjectWithJson(siteListInquire, true);

            requestStr = GetRequestPacket(ProtocolID.sitelistinquire, siteListInquireStr);

            return requestStr;
        }

        public static SiteListInquire GetSiteListInquire()
        {
            var token = _platformService.GetAppData().Token;

            SiteListInquire siteListInquire = new SiteListInquire();
            siteListInquire.Token = token;
            return siteListInquire;
        }

        private static string GetRequestPacket(ProtocolID protocolId, string siteListInquireStr)
        {
            string requestPacket = string.Empty;
            ProtocolRequest protocolRequest = new ProtocolRequest();
            protocolRequest.Id = (int)protocolId;
            protocolRequest.Para = siteListInquireStr;
            requestPacket = SerializeObjectWithJson(protocolRequest, false);
            return requestPacket;
        }

        /// <summary>
        /// 获取终端状态数据请求的Post数据
        /// </summary>
        /// <returns></returns>
        public static string GetTerminalStatusRequestPostData()
        {
            string requestStr = string.Empty;
            SiteStatusInquire siteStatusInquire = GetSiteStatusInquire();

            string siteStatusInquireStr = SerializeObjectWithJson(siteStatusInquire, true);

            requestStr = GetRequestPacket(ProtocolID.siteStatusInquire, siteStatusInquireStr);

            return requestStr;
        }

        public static SiteStatusInquire GetSiteStatusInquire()
        {
            var token = _platformService.GetAppData().Token;

            SiteStatusInquire siteStatusInquire = new SiteStatusInquire();
            siteStatusInquire.Token = token;
            siteStatusInquire.InquireStatusList = new List<InquireStatusTypes>() {
                InquireStatusTypes.online,
                InquireStatusTypes.downloadProg,
                InquireStatusTypes.lastPubSucProg,
                InquireStatusTypes.screenShot,
                InquireStatusTypes.monitorStatus,
                InquireStatusTypes.MonitorAlarmStatus };
            return siteStatusInquire;
        }

        /// <summary>
        /// 获取账户验证请求的Post数据
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetVerifyAccountRequestPostData(string userName, string password)
        {
            string requestStr = string.Empty;

            LoginRequest loginRequest = GetLoginRequest(userName, password);

            string loginRequestStr = SerializeObjectWithJson(loginRequest, true);

            requestStr = GetRequestPacket(ProtocolID.userLogin, loginRequestStr);

            return requestStr;
        }

        public static LoginRequest GetLoginRequest(string userName, string password)
        {
            LoginRequest loginRequest = new LoginRequest();
            loginRequest.Acnt = userName;
            loginRequest.Pwd = password;
            return loginRequest;
        }


        /// <summary>
        /// 获取时间同步请求的Post数据
        /// </summary>
        /// <returns></returns>
        public static string GetTimeSyncRequestPostData()
        {
            string requestStr = string.Empty;

            TimeSyncRequest timeSyncRequest = GetTimeSyncRequest();

            string timeSyncRequestStr = SerializeObjectWithJson(timeSyncRequest, true);

            requestStr = GetRequestPacket(ProtocolID.timeSync, timeSyncRequestStr);

            return requestStr;
        }

        public static LogoffRequest GetLogoutRequest()
        {
            LogoffRequest logoffRequest = new LogoffRequest();
            return logoffRequest;
        }

        public static TimeSyncRequest GetTimeSyncRequest()
        {
            var token = _platformService.GetAppData().Token;

            TimeSyncRequest timeSyncRequest = new TimeSyncRequest();
            timeSyncRequest.Token = token;
            return timeSyncRequest;
        }

        public static string GetModifyAccountRequestPostData(string userName, string oldPassword, string newPassword)
        {
            string requestStr = string.Empty;

            ModifyAccRequest modifyAccRequest = GetModifyAccRequest(oldPassword, newPassword);

            string modifyAccRequestStr = SerializeObjectWithJson(modifyAccRequest, true);

            requestStr = GetRequestPacket(ProtocolID.userPwdModify, modifyAccRequestStr);

            return requestStr;
        }

        public static ModifyAccRequest GetModifyAccRequest(string oldPassword, string newPassword)
        {
            var token = _platformService.GetAppData().Token;

            ModifyAccRequest modifyAccRequest = new ModifyAccRequest();
            modifyAccRequest.Token = token;
            modifyAccRequest.NewPwd = newPassword;
            modifyAccRequest.Pwd = oldPassword;
            return modifyAccRequest;
        }

        public static string GetTerDataTransInfoRequestPostData(List<string> groupIdList)
        {
            string requestStr = string.Empty;

            SiteTransInfoInquire siteTransInfoInquire = GetSiteTransInfoInquire(groupIdList);

            string siteTransInfoInquireStr = SerializeObjectWithJson(siteTransInfoInquire, true);

            requestStr = GetRequestPacket(ProtocolID.siteTransinfoInquire, siteTransInfoInquireStr);

            return requestStr;
        }

        public static SiteTransInfoInquire GetSiteTransInfoInquire(List<string> groupIdList)
        {
            var token = _platformService.GetAppData().Token;

            SiteTransInfoInquire siteTransInfoInquire = new SiteTransInfoInquire();
            siteTransInfoInquire.Token = token;
            siteTransInfoInquire.GroupIdList = groupIdList.Where(g => !string.IsNullOrEmpty(g)).Select(g => Convert.ToInt32(g)).ToList();
            return siteTransInfoInquire;
        }

        public static string GetPlatformDataTransInfoRequestPostData()
        {
            string requestStr = string.Empty;

            PlatTransInfoInquire platTransInfoInquire = GetPlatTransInfoInquire();

            string platTransInfoInquireStr = SerializeObjectWithJson(platTransInfoInquire, true);

            requestStr = GetRequestPacket(ProtocolID.siteTransinfoInquire, platTransInfoInquireStr);

            return requestStr;
        }

        public static PlatTransInfoInquire GetPlatTransInfoInquire()
        {
            var token = _platformService.GetAppData().Token;

            PlatTransInfoInquire platTransInfoInquire = new PlatTransInfoInquire();
            platTransInfoInquire.Token = token;
            return platTransInfoInquire;
        }

        public static string SerializeObjectWithJson(object obj, bool isEncrypted)
        {
            if (obj == null)
                return string.Empty;
            string jsonStr = string.Empty;
            using (var mStream = new MemoryStream())
            {
                var js = new DataContractJsonSerializer(obj.GetType());
                js.WriteObject(mStream, obj);
                if (isEncrypted)
                    jsonStr = AESEncryption.AES_encrypt(Encoding.UTF8.GetString(mStream.ToArray()));
                else
                    jsonStr = Encoding.UTF8.GetString(mStream.ToArray());
            }
            return jsonStr;
        }

        public static object DeserializeObjectWithJson(string jsonString, Type objectType)
        {
            if (string.IsNullOrEmpty(jsonString))
                return null;
            object targetObject = null;
            using (var mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                var js = new DataContractJsonSerializer(objectType);
                targetObject = js.ReadObject(mStream);
            }
            return targetObject;
        }
    }
}
