using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface ILog
    {
        void Log(Exception ex);
        void Log(string text);
    }
}
