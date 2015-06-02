using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.OperationCommon;
using Nova.NovaWeb.Protocol;
using Nova.Security;
using Nova.Xml;
using System.Xml.Serialization;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.Common;
using Nova.Globalization;
using GalaSoft.MvvmLight.Messaging;
using System.Threading;

namespace Nova.NovaWeb.McGo.DAL
{
    public class HttpTerminalRepositoryProvider : ITerminalRepositoryProvider
    {
        private List<SiteStatus> _terminalStatusList = new List<SiteStatus>();


        private IPlatformService _platformService;


        public HttpTerminalRepositoryProvider()
        {
            _platformService = AppEnvionment.Default.Get<IPlatformService>();
        }

        public IEnumerable<Site> FindAllTerminalBaseInfo()
        {
            List<Site> _terminalList = new List<Site>();

            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();

            string webServerUrl = serverInfo.ServerAddress;
            string customerId = serverInfo.CustomerId;

            string rawDomainObjectServiceUrl = ServiceHelper.GetCurrentPhpServiceURL(ProtocolID.sitelistinquire);

            //if (!System.Text.RegularExpressions.Regex.IsMatch(rawDomainObjectServiceUrl, @"https://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"))
            //{
            //    Debug.WriteLine(string.Format("{0} - 无效的网络地址！！", rawDomainObjectServiceUrl));
            //}

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.sitelistinquire,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(SiteListReply),
                RequestDataObj = ServiceHelper.GetSiteListInquire()
            };

            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                string errorInfo;
                MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error,errorInfo));
            }
            else if (responseData.WebRequestRes == WebRequestRes.OK)
            {
                try
                {
                    var siteListReply = responseData.ReplyObj as SiteListReply;
                    if (siteListReply == null)
                        return null;
                    _terminalList.Clear();
                    foreach (var groupItem in siteListReply.GroupList)
                    {
                        // groupItem.SiteList.ForEach(t => t.GroupName = groupItem.Name);
                        _terminalList.AddRange(groupItem.SiteList);
                    }                   
                }
                catch (Exception ex)
                {
                    return null;
                }

            }
            else
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error, errorInfo));
            }

            return _terminalList;
        }


        public IEnumerable<SiteStatus> FindAllTerminalStatusInfo()
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();

            string webServerUrl = serverInfo.ServerAddress;
            string customerId = serverInfo.CustomerId;

            string terminalStatusServiceUrl = ServiceHelper.GetCurrentPhpServiceURL(ProtocolID.siteStatusInquire);

            //if (!System.Text.RegularExpressions.Regex.IsMatch(terminalStatusServiceUrl, @"https://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"))
            //{
            //    Debug.WriteLine(string.Format("{0} - 无效的网络地址！！", terminalStatusServiceUrl));
            //}

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.siteStatusInquire,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(SiteStatusReply),
                RequestDataObj = ServiceHelper.GetSiteStatusInquire()
            };

            RequestInfo responseData = null;

            webServerRequest.Post(requestData, out responseData);

            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                //string errorInfo;
                //MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                //NotificationMessage<string> notification = new NotificationMessage<string>("001", errorInfo);
                //Messenger.Default.Send<NotificationMessage<string>>(notification, "TerminalList");
            }
            else if (responseData.WebRequestRes == WebRequestRes.OK)
            {
                try
                {
                    var siteStatusReply = responseData.ReplyObj as SiteStatusReply;
                    if (siteStatusReply == null)
                        return null;

                    _terminalStatusList.Clear();
                    _terminalStatusList.AddRange(siteStatusReply.SiteStatusList);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                //string errorInfo;
                //MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                //NotificationMessage<string> notification = new NotificationMessage<string>("001", errorInfo);
                //Messenger.Default.Send<NotificationMessage<string>>(notification, "TerminalList");
            }
            return _terminalStatusList;
        }

        public void UpdateTerminalBaseInfo(Site Site)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Site> TerminalBaseInfoFilterBy(System.Linq.Expressions.Expression<Func<Site, object>> expression)
        {
            throw new NotImplementedException();
        }

    }


}
