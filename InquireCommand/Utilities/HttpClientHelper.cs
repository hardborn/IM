using Nova.Globalization;
using Nova.Net.Http;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Nova.NovaWeb.Windows.Utilities
{
    public static class HttpClientHelper
    {
        public static bool Post(string url, string requestData, out string replyData)
        {
            HttpClient httpClient = new HttpClient();
            Exception exception ;

            httpClient.Post(url, requestData, out replyData, out exception);

            if (IsReplyRight(replyData, exception))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsReplyRight(string replyData, Exception errorData)
        {
            if (errorData != null)
            {
                System.Net.WebException webException = errorData as System.Net.WebException;
                if (webException != null)
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                    MessageBox.Show(errorInfo);
                    return false;
                }
                System.Net.ProtocolViolationException protocolException = errorData as System.Net.ProtocolViolationException;
                if (protocolException != null)
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                    MessageBox.Show(errorInfo);
                    return false;
                }
            }

            if (IsSysError(replyData))
            {
                return false;
            }

            return true;
        }

        private static bool IsSysError(string replyData)
        {

            bool bSysError = false;
            SystemError systemError;
            systemError = new SystemError(replyData, out bSysError);
            SysErrorCode errCode;
            string errorInfo;
            if (systemError.GetErrCode(out errCode))
            {
                errorInfo = GetLocalizationBussnissInfo(errCode.ToString(), errCode.ToString());
                MessageBox.Show(errorInfo);
                return true;
            }
            return false;
        }

        private static string GetLocalizationBussnissInfo(string key, string defaultInfo)
        {
            string itemResult;
            if (MultiLanguageUtils.GetLanguageString(key, out itemResult))
            {
                return itemResult;
            }
            else
            {
                return itemResult = defaultInfo;
            }
        }

    }
}
