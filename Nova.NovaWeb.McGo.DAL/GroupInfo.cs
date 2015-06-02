using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Nova.NovaWeb.Protocol;
using Nova.NovaWeb.Common;

namespace Nova.NovaWeb.McGo.DAL
{
    [Serializable]
    public class GroupInfo
    {
        public GroupInfo()
        {
            
        }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlArray("TransmissionInfoList")]
        [XmlArrayItem("TransmissionInfo")]
        public List<TransmissionInfo> TransmissionInfoList { get; set; }
    }
}
