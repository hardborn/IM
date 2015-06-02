using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nova.NovaWeb.Protocol;
using Nova.NovaWeb.Common;

namespace Nova.NovaWeb.McGo.DAL
{
    public class TempTransmissionInfoService:ITransmissionInfoService
    {
        public TransmissionInfo GetPlatformDataTransInfo()
        {
            return new TransmissionInfo() { Account = "d_Deploy", DestinationAddress = "ftp://192.168.0.187", Password = "123456" };
        }

        public TransmissionInfo GetTerminalDataTransInfo(string groupId)
        {
            return new TransmissionInfo() { Account = "m_Deploy", DestinationAddress = "ftp://192.168.0.187", Password = "123456" };
        }
    }
}
