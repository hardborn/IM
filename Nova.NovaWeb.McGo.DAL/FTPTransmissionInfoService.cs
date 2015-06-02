using System.Runtime.Serialization.Json;
using GalaSoft.MvvmLight.Messaging;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Nova.NovaWeb.Protocol;
using Nova.Security;

namespace Nova.NovaWeb.McGo.DAL
{
    public class FTPTransmissionInfoService : ITransmissionInfoService
    {
        private IPlatformService _platformService;

        public FTPTransmissionInfoService()
        {
            _platformService = AppEnvionment.Default.Get<IPlatformService>();
        }

        public TransmissionInfo GetPlatformDataTransInfo()
        {

            TransmissionInfo platformDataTranssionInfo = new TransmissionInfo();
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();

            string webServerUrl = serverInfo.ServerAddress;
            string customerId = serverInfo.CustomerId;

            string platformDataTransInfoServiceUrl = ServiceHelper.GetCurrentPhpServiceURL(ProtocolID.platTransInfoInquire);

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.platTransInfoInquire,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(PlatTransInfoReply),
                RequestDataObj = ServiceHelper.GetPlatTransInfoInquire()
            };

            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                string errorInfo;
                MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error, errorInfo));
            }
            else if (responseData.WebRequestRes == WebRequestRes.OK)
            {

                try
                {
                    var platTransInfoReply = responseData.ReplyObj as PlatTransInfoReply;

                    if (platTransInfoReply == null && platTransInfoReply.TransInfoList[0] == null)
                    {
                        return new TransmissionInfo();
                    }
                    else
                    {
                        return platTransInfoReply.TransInfoList[0];
                    }

                }
                catch (Exception ex)
                {
                    return new TransmissionInfo();
                }
            }
            else
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error, errorInfo));
                return null;
            }

            return platformDataTranssionInfo;
        }

        public TransmissionInfo GetTerminalDataTransInfo(string groupId)
        {
            TransmissionInfo terminalDataTranssionInfo = new TransmissionInfo();

            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();

            string webServerUrl = serverInfo.ServerAddress;
            string customerId = serverInfo.CustomerId;

            string terDataTransInfoServiceUrl = ServiceHelper.GetCurrentPhpServiceURL();

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.siteTransinfoInquire,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(SiteTransInfoReply),
                RequestDataObj = ServiceHelper.GetSiteTransInfoInquire(new List<string>() { groupId })
            };

            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                string errorInfo;
                MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error, errorInfo));
            }
            else if (responseData.WebRequestRes == WebRequestRes.OK)
            {
                try
                {
                    var siteTransInfoReply = responseData.ReplyObj as SiteTransInfoReply;

                    if (siteTransInfoReply == null || siteTransInfoReply.GroupTransInfoList == null || siteTransInfoReply.GroupTransInfoList.Count == 0)
                        return null;

                    if (siteTransInfoReply.GroupTransInfoList[0].TransInfoList == null || siteTransInfoReply.GroupTransInfoList[0].TransInfoList.Count == 0)
                        return null;

                    return siteTransInfoReply.GroupTransInfoList[0].TransInfoList[0];
                  
                }
                catch (Exception ex)
                {
                    return new TransmissionInfo();
                }

            }
            else
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error, errorInfo));
            }
            return terminalDataTranssionInfo;
        }
    }
}
