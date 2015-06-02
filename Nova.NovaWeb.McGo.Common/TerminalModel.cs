
using Nova.NovaWeb.Common;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace Nova.NovaWeb.McGo.Common
{
    /// <summary>
    /// Description of Site.
    /// </summary>
    public class TerminalModel
    {
        private bool _isScreenShot;
        private bool? _enableIPCam;
        private List<StatusData> _statusList = new List<StatusData>();
        private string _timeZoneId;
        private int _groupId;
        private string _groupName;
        private string _placeAddress;
        private int _lEDWidth;
        private int _lEDHeight;
        private bool _isAlarm = false;
        private bool _isAlarmEnable = false;
        private bool _isOnline = false;
        private string _mac;
        private string _name;
        private Group _ownerGroup;
        public TerminalModel(Group owner, Site site, SiteStatus terminalStatus)
        {
            if (site == null)
                return;
            _terminal = site;
            _ownerGroup = owner;
            _terminalStatus = terminalStatus;
            DtoToModel();
        }


        /// <summary>
        /// 传输对象转换为模型对象
        /// </summary>
        private void DtoToModel()
        {
            if (_ownerGroup != null)
            {
                this._groupId = _ownerGroup.GroupID;
                this._groupName = _ownerGroup.GroupName;
            }
            if(_terminal != null)
            {
                this._mac = _terminal.Mac;
                this._name = _terminal.Sitename;
                this._lEDHeight = _terminal.LedHeight;
                this._lEDWidth = _terminal.LedWidth;
                this._timeZoneId = _terminal.TimezoneID;
                this._placeAddress = _terminal.SiteAddr;
            }
            

            if (_terminalStatus != null)
                _statusList = _terminalStatus.StatusDataList;

            if (_statusList != null)
            {
                this._isOnline = GetTerminalOnlineState();
                this._isAlarmEnable = GetTerminalAlarmEnable();
                this._isAlarm = GetTerminalAlarmState();
                this._isScreenShot = GetScreenShotStatus();
                this._enableIPCam = GetIPCamEnable();
            }
        }
        private Site _terminal;
        public Site CurrentTerminal
        {
            get
            {
                return _terminal;
            }
            private set
            {
                if (_terminal == value)
                {
                    return;
                }
                _terminal = value;
            }
        }

        private SiteStatus _terminalStatus;

        public SiteStatus TerminalStatus
        {
            get
            {
                return _terminalStatus;
            }
            private set
            {
                if (_terminalStatus == value)
                    return;
                _terminalStatus = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }

        public string Mac
        {
            get
            {
                return _mac;
            }
            private set
            {
                _mac = value;
            }
        }

        public bool IsOnline
        {
            get
            {
                return _isOnline;
            }
            private set
            {
                _isOnline = value;
            }
        }
        public bool IsEnableAlarm
        {
            get
            {
                return _isAlarmEnable;
            }
            private set
            {
                _isAlarmEnable = value;
            }
        }
        public bool IsAlarm
        {
            get
            {
                return _isAlarm;
            }
            private set
            {
                _isAlarm = value;
            }
        }

        public int LEDHeight
        {
            get
            {
                return _lEDHeight;
            }
            private set
            {
                _lEDHeight = value;
            }
        }

        public int LEDWidth
        {
            get
            {
                return _lEDWidth;
            }
            private set
            {
                _lEDWidth = value;
            }
        }

        public string PlaceAddress
        {
            get
            {
                return _placeAddress;
            }
           private set
            {
                _placeAddress = value;
            }
        }

        public string GroupName
        {
            get
            {
                return _groupName;
            }
           private set
            {
                _groupName = value;
            }
        }

        public int GroupId
        {
            get
            {
                return _groupId;
            }
            private set
            {
                _groupId = value;
            }
        }

        public string TimeZoneId
        {
            get
            {
                return _timeZoneId;
            }
            private set
            {
                _timeZoneId = value;
            }
        }

        public List<StatusData> StatusList
        {
            get
            {
                return _statusList;
            }
            private set
            {
                _statusList = value;
            }
        }

        public bool? EnableIPCam
        {
            get
            {
                return _enableIPCam;
            }
            private set
            {
                _enableIPCam = value;
            }
        }
        public bool IsScreenShot
        {
            get
            {
                return _isScreenShot;
            }
           private set
            {
                _isScreenShot = value;
            }
        }

        /// <summary>
        /// 获取终端快照状态
        /// </summary>
        /// <param name="terminalStatus"></param>
        /// <returns></returns>
        public bool GetScreenShotStatus()
        {
            if (_terminalStatus == null || _terminalStatus.StatusDataList == null)
                return false;
            StatusData statusType = _terminalStatus.StatusDataList.FirstOrDefault(T => T.StatusType == InquireStatusTypes.screenShot);
            if (statusType == null)
                return false;
            return GetScreenShotStatus(statusType.Status);
        }
       

        /// <summary>
        /// 获取终端告警是否启用
        /// </summary>
        /// <param name="terminalStatus"></param>
        /// <returns></returns>
        public bool GetTerminalAlarmEnable()
        {
            if (_terminalStatus == null || _terminalStatus.StatusDataList == null)
                return false;
            StatusData statusData = _terminalStatus.StatusDataList.FirstOrDefault(T => T.StatusType == InquireStatusTypes.monitorStatus);

            if (statusData == null)
                return  false;

            return  statusData.Status == "1" ? true : false;
        }

        /// <summary>
        /// 获取终端在线状态
        /// </summary>
        /// <param name="terminalStatus"></param>
        /// <returns></returns>
        public bool GetTerminalOnlineState()
        {
            if (_terminalStatus == null || _terminalStatus.StatusDataList == null)
                return false;

            StatusData statusData = _terminalStatus.StatusDataList.FirstOrDefault(T => T.StatusType == InquireStatusTypes.online);

            if (statusData == null)
                return false;

            return statusData.Status.Equals("0") ? false : true;
        }

        /// <summary>
        /// 获取终端设备告警状态
        /// </summary>
        /// <param name="terminalStatus"></param>
        /// <returns></returns>
        public bool GetTerminalAlarmState()
        {
            if (_terminalStatus == null || _terminalStatus.StatusDataList == null)
                return true;
            StatusData statusData = _terminalStatus.StatusDataList.FirstOrDefault(T => T.StatusType == InquireStatusTypes.MonitorAlarmStatus);
            if (statusData == null)
                return true;

            return statusData.Status == "1" ? true : false;
        }

        /// <summary>
        /// 获取终端IPCam是否启用
        /// </summary>
        /// <param name="TerminalStatus"></param>
        /// <returns></returns>
        public bool? GetIPCamEnable()
        {
            if (_terminal.IPCam == null)
                return null;
            string ipcam = _terminal.IPCam.Replace("+", string.Empty);

            if (string.IsNullOrEmpty(ipcam))
            {
                return null;
            }
            else
            {
                StatusData statusData = _terminalStatus.StatusDataList.FirstOrDefault(T => T.StatusType == InquireStatusTypes.screenShot);
                if (statusData == null)
                    return null;
                return GetIPCamStatus(statusData.Status);
            }
        }

        /// <summary>
        /// 获取该终端类型
        /// </summary>
        /// <param name="Site"></param>
        /// <returns></returns>
        public TerminalType GetTerminalType()
        {
            return _terminal.EquipCode == EquipCodeTypes.Pluto ? TerminalType.Embedded : TerminalType.PC;
        }
        /// <summary>
        /// 获取终端摄像头启用状态
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool GetIPCamStatus(string value)
        {
            string[] strSplits = value.Split(new char[] { '+' });
            if (strSplits.Length != 3)
            {
                return false;
            }
            int cycleValue;
            int.TryParse(strSplits[0].Trim(), out cycleValue);
            if (cycleValue == 0)
            {
                return false;
            }
            int result;
            int.TryParse(strSplits[2].Trim(), out result);
            if (result == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取桌面快照启用状态
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool GetScreenShotStatus(string value)
        {
            string[] strSplits = value.Split(new char[] { '+' });
            if (strSplits.Length != 3)
            {
                return false;
            }
            int result1;
            int result2;
            int.TryParse(strSplits[0].Trim(), out result1);
            if (result1 != 0)
            {
                int.TryParse(strSplits[1].Trim(), out result2);
                if (result2 == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        public TerminalModel Clone()
        {
            return new TerminalModel(_ownerGroup,_terminal.Clone(),_terminalStatus);
        }
    }
}