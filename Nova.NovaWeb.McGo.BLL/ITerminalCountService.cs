using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface ITerminalCountService
    {
        int GetTerminalOnlineCount();
        int GetTerminalOfflineCount();
        int GetTerminalTotalCount();
    }
}
