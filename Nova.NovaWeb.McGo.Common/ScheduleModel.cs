using System;
using System.IO;
using System.Collections.Generic;
using Nova.Xml.Files;
using Nova.NovaWeb.Common;
using Nova.Xml;
using Nova.MediaItem;

namespace Nova.NovaWeb.McGo.Common
{
    public class ScheduleModel
    {
        private string _downloadPathInServer;
        private bool _supportReadback = true;
        private List<MediaModel> _mediaList = new List<MediaModel>();
        private string _name;
        private DateTime _lastEditTime;
        private ScheduleType _type;
        private int _screenY;
        private int _screenX;
        private int _screenWidth;
        private int _screenHeight;
        private string _convertedFilePath;
        private string _mdcorFilePath;
        private ScheduleSourceType _sourceType;
        private Schedule _schedule;

        //public ScheduleModel()
        //{
        //}

        //public ScheduleModel(Schedule schedule)
        //    :this(schedule,ScheduleSourceType.LocalSource)
        //{
        //    _schedule = schedule;
        //    if (_schedule == null)
        //        return;

        //    DtoToModel();
        //}

        public ScheduleModel(Schedule schedule, ScheduleSourceType sourceType = ScheduleSourceType.LocalSource)
        {
            if (schedule == null)
                return;

            _schedule = schedule;
            _sourceType = sourceType;
            DtoToModel();
        }

        public ScheduleModel(
            string name,
            string filePath,
            ScheduleType type,
            DateTime lastEditTime,
            int screenHeight,
            int screenwidth,
            int screenX,
            int screenY)
        {
            _schedule = new Schedule() {
                Name = _schedule.Name,
                FilePath = _schedule.FilePath,
                Type = _schedule.Type,
                LastEditTime = _schedule.LastEditTime,
                ScreenHeight = _schedule.ScreenHeight,
                ScreenWidth = _schedule.ScreenWidth,
                ScreenX = _schedule.ScreenX,
                ScreenY = _schedule.ScreenY };

            DtoToModel();
        }




        private void DtoToModel()
        {
            this.Name = _schedule.Name;
            this.FilePath = _schedule.FilePath;
            this.Type = _schedule.Type;
            this.LastEditTime = _schedule.LastEditTime;
            this.ScreenHeight = _schedule.ScreenHeight;
            this.ScreenWidth = _schedule.ScreenWidth;
            this.ScreenX = _schedule.ScreenX;
            this.ScreenY = _schedule.ScreenY;
        }

        public List<MediaModel> GetMediaList()
        {
            if (string.IsNullOrEmpty(_filePath))
                return null;
            if (!File.Exists(_filePath))
                return null;

            var mediaList = new List<MediaModel>();

            List<IMedia> iMedias;

            if (!PlayProgramParser.GetAllMedia(_filePath, out iMedias))
                return null;

            foreach (var mediaItem in iMedias)
            {
                mediaList.Add(new MediaModel(mediaItem));
            }
            return mediaList;
        }

        public long GetAllMediaSize()
        {
            if (string.IsNullOrEmpty(_filePath))
                return 0L;
            if (!File.Exists(_filePath))
                return 0L;

            Dictionary<string, long> mediaSizeDictionary;
            long allMediaSize = 0L;
            try
            {
                PlayProgramParser.GetAllMediaSize(_filePath, out mediaSizeDictionary, out allMediaSize);
            }
            catch (Exception e)
            {
                string info = e.Message;
            }
            

            FileInfo fileinfo = new FileInfo(_filePath);
            long fileSize = fileinfo.Length;

            return allMediaSize + fileSize;
        }

        public ScheduleModel Clone()
        {
            return new ScheduleModel(_schedule.Name,
                       _schedule.FilePath,
                       _schedule.Type,
                       _schedule.LastEditTime,
                       _schedule.ScreenHeight,
                       _schedule.ScreenWidth,
                       _schedule.ScreenX,
                       _schedule.ScreenY);
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _filePath;
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }

        public string ConvertedFilePath
        {
            get
            {
                return _convertedFilePath;
            }
            set
            {
                _convertedFilePath = value;
            }
        }

        public string MdcorFilePath
        {
            get
            {
                return _mdcorFilePath;
            }
            set
            {
                _mdcorFilePath = value;
            }
        }

        public int ScreenHeight
        {
            get
            {
                return _screenHeight;
            }
            set
            {
                _screenHeight = value;
            }
        }

        public int ScreenWidth
        {
            get
            {
                return _screenWidth;
            }
            set
            {
                _screenWidth = value;
            }
        }

        public int ScreenX
        {
            get
            {
                return _screenX;
            }
            set
            {
                _screenX = value;
            }
        }

        public int ScreenY
        {
            get
            {
                return _screenY;
            }
            set
            {
                _screenY = value;
            }
        }

        public ScheduleType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public ScheduleSourceType SourceType
        {
            get
            {
                return _sourceType;
            }
            set
            {
                _sourceType = value;
            }
        }

        public DateTime LastEditTime
        {
            get
            {
                return _lastEditTime;
            }
            set
            {
                _lastEditTime = value;
            }
        }

        public bool SupportReadback
        {
            get
            {
                return _supportReadback;
            }
            set
            {
                _supportReadback = value;
            }
        }

        public string DownloadPathInServer
        {
            get
            {
                return _downloadPathInServer;
            }
            set
            {
                _downloadPathInServer = value;
            }
        }
    }
}
