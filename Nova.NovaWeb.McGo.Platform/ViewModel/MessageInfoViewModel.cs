using GalaSoft.MvvmLight;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class MessageInfoViewModel : ViewModelBase
    {
        public MessageInfoViewModel(MessageType type,string messageText)
        {
            _type = type;
            _messageText = messageText;
        }

        private string _messageText;
        private MessageType _type = MessageType.Information;
        public MessageType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                RaisePropertyChanged("Type");
            }
        }

        public string MessageText
        {
            get
            {
                return _messageText;
            }
            set
            {
                _messageText = value;
                RaisePropertyChanged("MessageText");
            }
        }
    }
}