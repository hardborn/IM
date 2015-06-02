using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Nova.NovaWeb.McGo.DAL
{
    [Serializable]
    [XmlRoot("verifyAccReply")]
    public class VerifyAccountReply
    {
        public VerifyAccountReply() { }

        [XmlElement("protocolVer")]
        public string ProtocolVer { get; set; }

        [XmlElement("username")]
        public string UserName { get; set; }

        [XmlElement("rightlist")]
        public int RightList { get; set; }

        //[XmlArray("medLibList")]
        //[XmlArrayItem("lib")]
        //public List<MediaLibraryInfo> MediaLibraryInfoList { get; set; }
    }
}
