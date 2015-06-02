using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface IServerInfoProvider
    {
        ServerInfo GetCurrentServerInfo();
    }
}
