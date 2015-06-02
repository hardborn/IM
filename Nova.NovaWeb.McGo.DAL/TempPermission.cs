using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.DAL
{
    public class TempPermission:IPermission
    {

        public TempPermission()
        {
            _privilegeList = new List<PrivilegeTypes>();
        }

        private List<PrivilegeTypes> _privilegeList;

        public List<PrivilegeTypes> PrivilegeList
        {
            get
            {
                return _privilegeList;
            }
            set
            {
                if(value == _privilegeList)
                    return ;
                _privilegeList = value;
            }
        }
    }
}
