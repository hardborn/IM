using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Nova.NovaWeb.Common;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface ITransmissionInfoService
    {
        TransmissionInfo GetPlatformDataTransInfo();
        TransmissionInfo GetTerminalDataTransInfo(string groupId);
    }
}
