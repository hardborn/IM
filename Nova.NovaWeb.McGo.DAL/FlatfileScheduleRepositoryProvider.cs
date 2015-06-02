using Nova.NovaWeb.Common;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.Utilities;
using Nova.Xml;
using Nova.Xml.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.DAL
{
    public class FlatfileScheduleRepositoryProvider : IScheduleRepositoryProvider
    {
        public IEnumerable<Schedule> FindAll()
        {
            //List<Schedule> allSchedules = new List<Schedule>();

            //CodeTimer.Time("获取播放列表", 1, new Action(() =>
            //{
            //    allSchedules.AddRange(GetPcSchedule());
            //    allSchedules.AddRange(GetEmbeddedSchedule());
                
            //}));
            List<Schedule> allSchedules = new List<Schedule>();
            allSchedules.AddRange(GetPcSchedule());
            allSchedules.AddRange(GetEmbeddedSchedule());
            return allSchedules;
            //return allSchedules;
        }

        /// <summary>
        /// 获取本地同步播放方案列表
        /// </summary>
        /// <returns></returns>
        private List<Schedule> GetPcSchedule()
        {
            return GetSchedules(ScheduleType.PC,".cplym");
        }

        /// <summary>
        /// 获取本地异步播放方案列表
        /// </summary>
        /// <returns></returns>
        private List<Schedule> GetEmbeddedSchedule()
        {
            return GetSchedules(ScheduleType.Common,".plym");           
        }

        private List<Schedule> GetSchedules(ScheduleType scheduleType,string fileExtension)
        {
            List<Schedule> schedules = new List<Schedule>();

            List<FileInfo> playFiles = GetFiles(PlatformConfig.LocalScheduleDirectoryPath, fileExtension);

            foreach (var item in playFiles)
            {
                bool bSuccess;
                PlayProgramXml playProgramXml = new PlayProgramXml(item.FullName, XmlFile.XmlFileFlag.XmlExisting, out bSuccess);
                if (bSuccess)
                {
                    Schedule schedule = new Schedule();
                    schedule.FilePath = item.FullName;
                    schedule.Name = Path.GetFileName(item.FullName);
                    schedule.ScreenHeight = playProgramXml.ScreenHeight;
                    schedule.ScreenWidth = playProgramXml.ScreenWidth;
                    schedule.ScreenX = playProgramXml.ScreenX;
                    schedule.ScreenY = playProgramXml.ScreenY;
                    schedule.Type = scheduleType;
                    schedule.LastEditTime = File.GetLastWriteTime(item.FullName);
                    schedules.Add(schedule);
                }
            }
            return schedules;
        }

        public void Update(Schedule schedule)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Schedule> FilterBy(System.Linq.Expressions.Expression<Func<Schedule, object>> expression)
        {
            throw new NotImplementedException();
        }

        private List<FileInfo> GetFiles(string path, string extName)
        {
            return GetDir(path, extName);
        }


        private List<FileInfo> GetDir(string path, string extName)
        {
            List<FileInfo> lst = new List<FileInfo>();
            try
            {
                string[] dir = Directory.GetDirectories(path);
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                if (file.Length != 0 || dir.Length != 0)
                {
                    foreach (FileInfo f in file)
                    {
                        if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0)
                        {
                            lst.Add(f);
                        }
                    }
                    foreach (string d in dir)
                    {
                        GetDir(d, extName);
                    }
                }
            }
            catch
            {
            }
            ;
            return lst;
        }
    }
}
