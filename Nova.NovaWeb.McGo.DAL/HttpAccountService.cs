using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using GalaSoft.MvvmLight.Messaging;
using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Nova.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;

namespace Nova.NovaWeb.McGo.DAL
{
    public class HttpAccountService : IAccountAuthenticationService
    {
        private IPlatformService _platformService;
        private IPermission _permissionService;

        public HttpAccountService()
        {
            _platformService = AppEnvionment.Default.Get<IPlatformService>();
            _permissionService = AppEnvionment.Default.Get<IPermission>();
        }

        public bool VerifyPlatformAccount(Account account)
        {
            ServerInfo serverInfo = _platformService.GetPlatformServerInfo();

            return Verify(account);
        }

        private bool Verify(Account account)
        {
            string accountVerityServiceUrl = ServiceHelper.GetCurrentPhpServiceURL(ProtocolID.userLogin);


            #region ------- 备注 -------

            //"http://localhost:8080/NovaCloud/Deploy/index.php/API/Index/index?id=101"
            //accountVerityServiceUrl = "http://192.168.0.9:8080/NovaCloud/index.php/API/Ter/LoginRequest";

            #endregion

            WebServerRequest webServerRequest = new WebServerRequest();
            ProtocolRequestData requestData = new ProtocolRequestData()
            {
                PID = ProtocolID.userLogin,
                Url = ServiceHelper.GetCurrentPhpServiceURL(),
                ReplyDataType = typeof(LoginReply),
                RequestDataObj = ServiceHelper.GetLoginRequest(account.Name, account.Password)
            };

            System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();

            RequestInfo responseData = null;
            Debug.WriteLine("++++++++URL+++++++" + requestData.Url.ToString() + "+++++++++++++++++");
            webServerRequest.Post(requestData, out responseData);
            
            if (responseData.WebRequestRes == WebRequestRes.SysError)
            {
                //To do something....
                string errorInfo;
                MultiLanguageUtils.GetLanguageString(responseData.SysCode.ToString(), out errorInfo);
                NotificationMessage<string> notification = new NotificationMessage<string>("Exception", errorInfo);
                Messenger.Default.Send<NotificationMessage<string>>(notification, "Login");
                return false;
            }
            else
                if (responseData.WebRequestRes == WebRequestRes.OK)
                {
                    try
                    {
                        var loginReply = responseData.ReplyObj as LoginReply;
                        if (loginReply != null)
                        {
                            _platformService.GetAppData().Token = loginReply.Token;
                            _permissionService.PrivilegeList = loginReply.Privilegelist;
                            return true;
                        }
                        else
                            return false;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                else
                {
                    string errorInfo;
                    MultiLanguageUtils.GetLanguageString("ManangementCenter_Business_WebException", out errorInfo);
                    NotificationMessage<string> notification = new NotificationMessage<string>("Exception", errorInfo);
                    Messenger.Default.Send<NotificationMessage<string>>(notification, "Login");
                    return false;
                }
        }

        //private LoginReply ParseVerifyAccountReplyProtocol(RequestInfo responseData)
        //{
        //    try
        //    {
        //        string replayDate = AESEncryption.AES_decrypt(responseData.ReplyDate);
        //        //var obj = new JavaScriptSerializer().Deserialize<LoginReply>(replayDate);

        //        using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(replayDate)))
        //        {
        //            var serializer = new DataContractJsonSerializer(typeof(List<object>), null, int.MaxValue, false, new DateTimeDataContractSurrogate(), true);
        //            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(LoginReply));
        //            var loginReply = dcjs.ReadObject(stream) as LoginReply;
        //            _platformService.GetAppData().Token = loginReply.Token;
        //            return loginReply;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

    }

    public class DateTimeDataContractSurrogate : IDataContractSurrogate
    {
        private static readonly Regex dateRegex = new Regex(@"/Date\((\d+)([-+])(\d+)\)/");
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            // not used
            return null;
        }

        public object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType)
        {
            // not used
            return null;
        }

        public Type GetDataContractType(Type type)
        {
            // not used
            return type;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            // for debugging
            //Console.WriteLine("GetDeserializedObject: obj = {0} ({1}), targetType = {2}", obj, obj.GetType(), targetType);

            // only act on List<object> types
            if (obj.GetType() == typeof(List<object>))
            {
                var objList = (List<object>)obj;

                List<object> copyList = new List<object>(); // a list to copy values into. this will be the list returned.
                foreach (var item in objList)
                {
                    string s = item as string;
                    if (s != null)
                    {
                        // check if we match the DateTime format
                        Match match = dateRegex.Match(s);
                        if (match.Success)
                        {
                            // try to parse the string into a long. then create a datetime and convert to local time.
                            long msFromEpoch;
                            if (long.TryParse(match.Groups[1].Value, out msFromEpoch))
                            {
                                TimeSpan fromEpoch = TimeSpan.FromMilliseconds(msFromEpoch);
                                copyList.Add(System.TimeZoneInfo.ConvertTimeFromUtc(epoch.Add(fromEpoch), System.TimeZoneInfo.Local));
                                continue;
                            }
                        }
                    }

                    copyList.Add(item); // add unmodified
                }

                return copyList;
            }

            return obj;
        }

        public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
        {
            // not used   
        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            // for debugging
            //Console.WriteLine("GetObjectToSerialize: obj = {0} ({1}), targetType = {2}", obj, obj.GetType(), targetType);
            return obj;
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            // not used
            return null;
        }

        public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration typeDeclaration, System.CodeDom.CodeCompileUnit compileUnit)
        {
            // not used
            return typeDeclaration;
        }
    }

    public class MyPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint,
          X509Certificate certificate, WebRequest request,
          int certificateProblem)
        {
            //Return True to force the certificate to be ac
            return true;
        }
    }
}
