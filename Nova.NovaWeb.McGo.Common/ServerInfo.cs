using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Common
{
    [Serializable]
    public class ServerInfo
    {
        public ServerInfo() { }

        public string ServerAddress { get; set; }

        public string CustomerId { get; set; }

    }
}
