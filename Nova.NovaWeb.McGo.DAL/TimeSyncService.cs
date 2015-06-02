using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using GalaSoft.MvvmLight.Messaging;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.Protocol;
using Nova.Security;
using Nova.NovaWeb.McGo.Utilities;

namespace Nova.NovaWeb.McGo.DAL
{
    public class TimeSyncService : ITimeSyncService
    {
        private IPlatformService _platformService;

        public TimeSyncService()
        {
            _platformService = AppEnvionment.Default.Get<IPlatformService>();
        }

        public DateTime SynchronizeTime()
        {
            AppData serverInfo = _platformService.GetAppData();

            string accountVerityServiceUrl = ServiceHelper.GetCurrentPhpServiceURL(ProtocolID.timeSync);

            #region ------- 备注 -------
            //"http://localhost:8080/NovaCloud/Deploy/index.php/API/Index/index?id=101"
            //accountVerityServiceUrl = "http://192.168.0.9:8080/NovaCloud/index.php/API/Ter/LoginRequest";
            #endregion

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.timeSync,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(SystemError),
                RequestDataObj = ServiceHelper.GetTimeSyncRequest()
            };

            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                string errorInfo;
                MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                NotificationMessage<string> notification = new NotificationMessage<string>("Exception", errorInfo);
                Messenger.Default.Send<NotificationMessage<string>>(notification, "Login");
                return DateTime.Now;
            }
            else if (responseData.WebRequestRes == WebRequestRes.OK)
            {
                try
                {
                    var timeSyncReply = responseData.ReplyObj as TimeSyncReply;
                    if (timeSyncReply != null)
                    {
                        timeSyncReply.Time = timeSyncReply.Time;
                        DateTimeHelper.SetSystemClock(
                            (short)timeSyncReply.Time.Year,
                            (short)timeSyncReply.Time.Month,
                            (short)timeSyncReply.Time.Day,
                            (short)timeSyncReply.Time.Hour,
                            (short)timeSyncReply.Time.Minute,
                            (short)timeSyncReply.Time.Second);
                    }
                }
                catch (Exception ex)
                {
                    return DateTime.Now;
                }
            }
            else
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                NotificationMessage<string> notification = new NotificationMessage<string>("Exception", errorInfo);
                Messenger.Default.Send<NotificationMessage<string>>(notification, "Login");
                return DateTime.Now;
            }
            return DateTime.Now;
        }
    }
}
