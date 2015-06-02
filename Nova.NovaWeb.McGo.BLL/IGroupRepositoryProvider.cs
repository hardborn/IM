using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface IGroupRepositoryProvider
    {
        IEnumerable<Group> FindAllGroup();

        IEnumerable<Group> GroupFilterBy(System.Linq.Expressions.Expression<Func<Group, object>> expression);
    }
}
