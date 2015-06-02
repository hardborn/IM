using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Nova.NovaWeb.McGo.DAL
{
    [Serializable]
    public class MediaLibraryInfo
    {
        public MediaLibraryInfo() { }

        [XmlElement("url")]
        public string URL { get; set; }

        [XmlElement("user")]
        public string Account { get; set; }

        [XmlElement("PW")]
        public string Password { get; set; }
    }
}
