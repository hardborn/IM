using Nova.NovaWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface IPermission
    {
        List<PrivilegeTypes> PrivilegeList { get; set; }
    }
}
