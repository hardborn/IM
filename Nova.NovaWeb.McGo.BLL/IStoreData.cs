using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface IStoreData
    {
        void ClearData();

        object this[string key] { get; set; }
    }
}
