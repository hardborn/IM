using Nova.Globalization;
using Nova.NovaWeb.Common;
using Nova.NovaWeb.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Data;

namespace Nova.NovaWeb.Windows.Converters
{
    public class CommandParameterConverter : IValueConverter
    {
        private System.Globalization.CultureInfo _culture;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            _culture = Thread.CurrentThread.CurrentCulture;
            var replayCommand = value as InquireReplyCommand;
            if (replayCommand == null)
            {
                return null;
            }
            else
            {
                return GetCommandPara(replayCommand);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取命令参数的显示数据
        /// </summary>
        /// <param name="replyCmd"></param>
        /// <returns></returns>
        private string GetCommandPara(InquireReplyCommand replyCmd)
        {
            if (
                replyCmd.CommandType == CmdTypes.downloadPlaylist ||
                replyCmd.CommandType == CmdTypes.downloadSchedule ||
                replyCmd.CommandType == CmdTypes.downLoadPlayProgam ||
                replyCmd.CommandType == CmdTypes.uploadDirSet)
            {
                string tempPara = replyCmd.CommandPara.Substring(0, replyCmd.CommandPara.IndexOf('+'));
                tempPara = tempPara.Substring(tempPara.LastIndexOf('/') + 1);
                tempPara = RemoveMD5InFileName(tempPara);
                return tempPara;
            }
            else if (
                replyCmd.CommandType == CmdTypes.dviEncrypt ||
                replyCmd.CommandType == CmdTypes.modifyDviEncryptPwd ||
                replyCmd.CommandType == CmdTypes.enableDviEncrypt ||
                replyCmd.CommandType == CmdTypes.readDviEncryptStatus ||
                replyCmd.CommandType == CmdTypes.readDviEncryptConfig)
            {
                return string.Empty;
            }
            else if (replyCmd.CommandType == CmdTypes.emergencyMessage)
            {
                return GetEmergencyText(replyCmd);
            }
            //else if (replyCmd.CommandType == CmdTypes.switchLed)//参数有变化,添加类型CmdTypes.plutoPowerPlanSet,CmdTypes.displayModeSet,autoRestartTerminalSet,restartTeminal,downLoadPlayProgam,lockUnlockTC(无参数)
            //{
            //    SwitchLedType switchType;
            //    if (!IsSwitchLedParaValid(replyCmd, out switchType))
            //    {
            //        return replyCmd.CommandPara;
            //    }

            //    return replyCmd.CommandPara;
            //}
            else if (replyCmd.CommandType == CmdTypes.lockUnlockTC)
            {
                try
                {
                    string lockText = "";
                    if (_culture.TwoLetterISOLanguageName.Equals("en",StringComparison.OrdinalIgnoreCase))
                    {
                        string tempPara = replyCmd.CommandPara;
                        //if (replyCmd.CommandPara == "0")
                        //{
                        //    tempPara = replyCmd.CommandPara;
                        //}
                        //else
                        //{
                        //    tempPara = replyCmd.CommandPara.Substring(0, replyCmd.CommandPara.IndexOf('+'));
                        //}
                       
                        if (tempPara == "0")
                        {
                            lockText = "lockTerminal";
                        }
                        else
                        {
                            lockText = "unLockTerminal";
                        }
                        return lockText;
                    }
                    else if (_culture.Name.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                    {
                        string tempPara = replyCmd.CommandPara;
                        //if (replyCmd.CommandPara == "0")
                        //{
                        //    tempPara = replyCmd.CommandPara;
                        //}
                        //else
                        //{
                        //    tempPara = replyCmd.CommandPara.Substring(0, replyCmd.CommandPara.IndexOf('+'));
                        //}
                        if (tempPara == "0")
                        {
                            lockText = "锁定";
                        }
                        else
                        {
                            lockText = "解锁";
                        }
                        return lockText;
                    }
                    return lockText;
                }
                catch (Exception)
                {
                    return string.Empty;
                }

            }
            else if (replyCmd.CommandType == CmdTypes.soundSchSet ||
                replyCmd.CommandType == CmdTypes.lightSchSet)
            {
                return string.Empty;
            }
            else if (replyCmd.CommandType == CmdTypes.uploadLog)
            {
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                if (paraArr.Length < 5)
                {
                    return "";
                }
                return paraArr[3] + " -> " + paraArr[4];
            }
            else if (replyCmd.CommandType == CmdTypes.terminalConfig)
            {
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                if (paraArr.Length < 4)
                {
                    return "";
                }
                return paraArr[0] +" | "+ paraArr[1];
            }
            else if (replyCmd.CommandType == CmdTypes.cleanMediasPeriodSet)
            {
                string cmdPara = string.Empty;
                if (_culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
                {
                   
                    if (replyCmd.CommandPara == "0")
                    {
                        cmdPara = "One day previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "1")
                    {
                        cmdPara = "Three day previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "2")
                    {
                        cmdPara = "One week previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "3")
                    {
                        cmdPara = "One month previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "4")
                    {
                        cmdPara = "Two months previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "5")
                    {
                        cmdPara = "Three months previously uploaded media files";
                    }
                    else
                    {
                        cmdPara = string.Empty;
                    }
                }
                else if (_culture.Name.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                {
                    
                    if (replyCmd.CommandPara == "0")
                    {
                        cmdPara = "一天以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "1")
                    {
                        cmdPara = "三天以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "2")
                    {
                        cmdPara = "一周以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "3")
                    {
                        cmdPara = "一个月以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "4")
                    {
                        cmdPara = "两个月以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "5")
                    {
                        cmdPara = "三个月以前下载的媒体文件";
                    }
                    else
                    {
                        cmdPara = string.Empty;
                    }

                  
                }
                return cmdPara;
              
            }
            else if (replyCmd.CommandType == CmdTypes.updateSoftware)
            {
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                if (paraArr.Length < 4)
                {
                    return string.Empty;
                }
                return paraArr[3];
            }
            else if (replyCmd.CommandType == CmdTypes.screenShotPeriodSet)
            {
                string cmdPara = "";
                if (_culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
                {
                    string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                   
                    if (paraArr.Length < 1)
                    {
                        return cmdPara;
                    }
                    if (paraArr[0] == "0")
                    {
                        cmdPara = "Close Snapshot";
                    }
                    else
                    {
                        cmdPara = "Open Snapshot，Set a period of:";
                        string unit = "min";
                        cmdPara = cmdPara + paraArr[0] + unit;
                    }

                   
                }
                else if (_culture.Name.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                {
                    string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                    if (paraArr.Length < 1)
                    {
                        return cmdPara;
                    }
                    if (paraArr[0] == "0")
                    {
                        cmdPara = "关闭快照";
                    }
                    else
                    {
                        cmdPara = "开启快照，并周期为：";
                        string unit = "分钟";
                        cmdPara = cmdPara + paraArr[0] + unit;
                    }
                }
                return cmdPara;
             
            }
            else if (replyCmd.CommandType == CmdTypes.emergencyPlaylist)
            {
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                string cmdPara = "", playlistNameText = "清单名称：", startTimeText = "起始播放时间：", duringText = "播放时长：";
                string itemResult1;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_playlistName", out itemResult1);
                string itemResult2;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_startTime", out itemResult2);
                string itemResult3;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_during", out itemResult3);

                if (paraArr.Length >= 8)
                {
                    cmdPara = itemResult1 + Path.GetFileNameWithoutExtension(paraArr[0]) + itemResult2 + paraArr[6] + itemResult3 + paraArr[7];
                    cmdPara = string.Format("{0}{1}  {2}{3}  {4}{5}", itemResult1, Path.GetFileNameWithoutExtension(paraArr[0]), itemResult2, paraArr[6], itemResult3, paraArr[7]);
                }
                return cmdPara;
            }
            else if (replyCmd.CommandType == CmdTypes.switchLed)
            {
                return string.Empty;
            }
            else if (replyCmd.CommandType == CmdTypes.uploadLogParaSet ||
                replyCmd.CommandType == CmdTypes.displayModeSet ||
                replyCmd.CommandType == CmdTypes.mediaSync ||
                replyCmd.CommandType == CmdTypes.updateSSL)
            {
                string cmdPara =string.Empty;
                if (replyCmd.CommandPara == "1")
                {
                    string result = GetLocalizationBussnissInfo("InquireCommandModule_Business_Enable", "Enable");
                    //MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_Enable", out result);
                    cmdPara = result;
                }
                else if (replyCmd.CommandPara == "0")
                {
                    string result = GetLocalizationBussnissInfo("InquireCommandModule_Business_Disabled", "Disabled");
                    //MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_Disabled", out result);
                    cmdPara = result;
                }
                return cmdPara;
            }
            else if (replyCmd.CommandType == CmdTypes.autoRestartTerminalSet)
            {
                string cmdPara = string.Empty;
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                if (paraArr[0] == "1")
                {
                    string result = GetLocalizationBussnissInfo("InquireCommandModule_Business_Enable", "Enable");
                    cmdPara = result;
                    cmdPara += "(" + paraArr[1] +")";
                }
                else if (paraArr[0] == "0")
                {
                    string result = GetLocalizationBussnissInfo("InquireCommandModule_Business_Disabled", "Disabled");
                    cmdPara = result;
                }
                return cmdPara;
            }
            else
            {
                return replyCmd.CommandPara;
            }
        }

        private string GetLocalizationBussnissInfo(string key, string defaultInfo)
        {
            string itemResult;
            if (MultiLanguageUtils.GetLanguageString(key, out itemResult))
            {
                return itemResult;
            }
            else
            {
                return itemResult = defaultInfo;
            }
        }

        private string RemoveMD5InFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            string[] nameSplits = fileName.Split(new Char[] { '.' });
            if (nameSplits.Length == 2)
            {
                return fileName;
            }
            else if (nameSplits.Length == 3)
            {
                return nameSplits[0] + "." + nameSplits[2];
            }
            else
            {
                return nameSplits[0] + "." + nameSplits[nameSplits.Length - 1];
            }
        }

        private string GetEmergencyText(InquireReplyCommand replyCmd)
        {
            string msgTypeStr = replyCmd.CommandPara.Substring(0, 1);
            int msgTypeInt = Int32.Parse(msgTypeStr);
            MsgTypeEnum msgType = (MsgTypeEnum)msgTypeInt;
            bool bSuccess;
            EmergencyMsgBase emergMsgClass = null;
            string InvalidCmdPara = "命令参数格式错误";


            switch (msgType)
            {
                case MsgTypeEnum.MerryGoRound:
                    emergMsgClass = new EmergencyMsgMerryGoRound(replyCmd.CommandPara, out bSuccess);
                    break;
                case MsgTypeEnum.SingleLine:
                    emergMsgClass = new EmergencyMsgSingleLine(replyCmd.CommandPara, out bSuccess);
                    break;
                case MsgTypeEnum.Static:
                    emergMsgClass = new EmergencyMsgStatic(replyCmd.CommandPara, out bSuccess);
                    break;
                default:
                    return InvalidCmdPara;
            }
            if (!bSuccess)
            {
                return InvalidCmdPara;
            }
            return emergMsgClass.MsgText;
        }

        private bool IsSwitchLedParaValid(InquireReplyCommand cmd, out SwitchLedType switchType)
        {
            switchType = SwitchLedType.normal;
            try
            {
                int intSwithcType = Int32.Parse(cmd.CommandPara.Substring(0, 1));
                switchType = (SwitchLedType)intSwithcType;
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
