using System.Runtime.Serialization.Json;
using GalaSoft.MvvmLight.Messaging;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Nova.NovaWeb.Protocol;
using Nova.Security;

namespace Nova.NovaWeb.McGo.DAL
{
    public class HttpGroupRepositoryProvider:IGroupRepositoryProvider
    {
        private List<Group> _groupList = new List<Group>();

        private IPlatformService _platformService;

        public HttpGroupRepositoryProvider()
        {
            _platformService = AppEnvionment.Default.Get<IPlatformService>();
        }

        public IEnumerable<Group> FindAllGroup()
        {
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
                AppMessages.RaiseMessageInfoMessage.Send(new NotificationMessage<MessageType>(MessageType.Error, errorInfo));
            }
            else if (responseData.WebRequestRes == WebRequestRes.OK)
            {
                try
                {
                    var siteListReply = responseData.ReplyObj as SiteListReply;
                    if (siteListReply == null)
                        return null;
                    return siteListReply.GroupList;                   
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

            return null;
        }

        public IEnumerable<Group> GroupFilterBy(System.Linq.Expressions.Expression<Func<Group, object>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
