using GalaSoft.MvvmLight.Messaging;
using Nova.NovaWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Common
{
    public static class AppMessages
    {
        enum MessageTypes
        {
           HeartBeat,
           ChangeLanguage,
           RaiseSystemError,
            PublishStatus,
            RaiseMessageInfo
        }

        public static class HeartBeatMessage
        {
        //    public static void Send(string )
        }

        public static class ChangeLanguageMessage
        {
            public static void Send(string languageName)
            {
                Messenger.Default.Send<string>(languageName,MessageTypes.ChangeLanguage);
            }

            public static void Register(object recipient, Action<string> action)
            {
                Messenger.Default.Register<string>(recipient,MessageTypes.ChangeLanguage,action);
            }
        }

        public static class RaiseSystemErrorMessage
        {
            public static  void Send(SysErrorCode errorCode)
            {
                Messenger.Default.Send<SysErrorCode>(errorCode,MessageTypes.RaiseSystemError);
            }

            public static void Register(object recipient,Action<SysErrorCode> action)
            {
                Messenger.Default.Register<SysErrorCode>(recipient,MessageTypes.RaiseSystemError,action);
            }
        }

        public static class RaiseMessageInfoMessage
        {
            public static void Send(NotificationMessage<MessageType> message)
            {
                Messenger.Default.Send<NotificationMessage<MessageType>>(message, MessageTypes.RaiseMessageInfo);
            }

            public static void Register(object recipient, Action<NotificationMessage<MessageType>> action)
            {
                Messenger.Default.Register<NotificationMessage<MessageType>>(recipient, MessageTypes.RaiseMessageInfo, action);
            }
        }

        public static class PublishStatusMessage
        {
            public static void Send(PublishStatus status)
            {
                Messenger.Default.Send<PublishStatus>(status,MessageTypes.PublishStatus);
            }

            public static void Register(object recipient,Action<PublishStatus> action)
            {
                Messenger.Default.Register<PublishStatus>(recipient,MessageTypes.PublishStatus,action);
            }
        }

    }
}
