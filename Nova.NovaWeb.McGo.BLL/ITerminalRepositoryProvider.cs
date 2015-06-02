using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface ITerminalRepositoryProvider
    {
        IEnumerable<Site> FindAllTerminalBaseInfo();

        void UpdateTerminalBaseInfo(Site Site);

        IEnumerable<Site> TerminalBaseInfoFilterBy(System.Linq.Expressions.Expression<Func<Site, object>> expression);

        IEnumerable<SiteStatus> FindAllTerminalStatusInfo();

    }
}
