
using Nova.NovaWeb.Common;
using Nova.Xml;
using Nova.Xml.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Common
{
    public class SchedulePublishPathInfo
    {
        public SchedulePublishPathInfo()
        {
        }

        public SchedulePublishPathInfo(string cloudFilePath, string plutoFilePath, string plutoConverterFilePath, string plutoMdcorFilePath)
        {
            CloudFilePath = cloudFilePath;
            PlutoFilePath = plutoFilePath;
            PlutoConverterFilePath = plutoConverterFilePath;
            PlutoMdcorFilePath = plutoMdcorFilePath;
        }
        private Dictionary<string, int> _dicCloudMediaFileWeights;
        public Dictionary<string, int> DicCloudMediaFileWeights
        {
            get
            {
                return _dicCloudMediaFileWeights;
            }
        }

        private Dictionary<string, int> _dicPlutoMediaFileWeights;
        public Dictionary<string, int> DicPlutoMediaFileWeights
        {
            get
            {
                return _dicPlutoMediaFileWeights;
            }
        }

        private string _cloudFilePath;
        public string CloudFilePath
        {
            get
            {
                return _cloudFilePath;
            }
            set
            {
                if (_cloudFilePath == value)
                    return;
                _cloudFilePath = value;
            }
        }

        private string _plutoFilePath;
        public string PlutoFilePath
        {
            get
            {
                return _plutoFilePath;
            }
            set
            {
                if (_plutoFilePath == value)
                    return;
                _plutoFilePath = value;
            }
        }

        public string PlutoConverterFilePath { get; set; }

        public string PlutoMdcorFilePath { get; set; }

        public bool SetCloudUploadMediaListWeights(List<string> uploadMediaFileList)
        {
            long totalLength = 0;
            FileInfo fInfo;
            List<long> fInfoList = new List<long>();
            string fileEx, filePath;
            for (int i = 0; i < uploadMediaFileList.Count; i++)
            {
                fileEx = Path.GetExtension(uploadMediaFileList[i]);
                if (fileEx == ".cplym" || fileEx == ".plym" || fileEx == ".plpym" || fileEx == ".mdcor")
                    filePath = uploadMediaFileList[i];
                else
                {
                    filePath = Path.GetDirectoryName(uploadMediaFileList[i]);
                    filePath = Path.Combine(filePath, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(uploadMediaFileList[i])) + Path.GetExtension(uploadMediaFileList[i]));
                }
                fInfo = new FileInfo(filePath);
                if (fInfo == null)
                    return false;
                totalLength += fInfo.Length;
                fInfoList.Add(fInfo.Length);
            }

            if (_dicCloudMediaFileWeights == null)
            {
                _dicCloudMediaFileWeights = new Dictionary<string, int>();
            }

            
            for (int i = 0; i < fInfoList.Count; i++)
            {
                if (!_dicCloudMediaFileWeights.Keys.Contains(uploadMediaFileList[i]))
                {
                    _dicCloudMediaFileWeights.Add(uploadMediaFileList[i], (int)(fInfoList[i] * 100 / totalLength));
                }
            }
            return true;
        }
        public bool SetPlutoUploadMediaListWeights(List<string> uploadMediaFileList)
        {
            long totalLength = 0;
            FileInfo fInfo;
            List<long> fInfoList = new List<long>();
            string fileEx, filePath;
            for (int i = 0; i < uploadMediaFileList.Count; i++)
            {
                fileEx = Path.GetExtension(uploadMediaFileList[i]);
                if (fileEx == ".plym" || fileEx == ".plpym" || fileEx == ".mdcor")
                    filePath = uploadMediaFileList[i];
                else
                {
                    filePath = Path.GetDirectoryName(uploadMediaFileList[i]);
                    filePath = Path.Combine(filePath, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(uploadMediaFileList[i])) + Path.GetExtension(uploadMediaFileList[i]));
                }
                fInfo = new FileInfo(filePath);
                if (fInfo == null)
                    return false;
                totalLength += fInfo.Length;
                fInfoList.Add(fInfo.Length);
            }

            if (_dicPlutoMediaFileWeights == null)
            {
                _dicPlutoMediaFileWeights = new Dictionary<string, int>();
            }

            for (int i = 0; i < fInfoList.Count; i++)
            {
                if (!_dicPlutoMediaFileWeights.Keys.Contains(uploadMediaFileList[i]))
                    _dicPlutoMediaFileWeights.Add(uploadMediaFileList[i], (int)(fInfoList[i] * 100 / totalLength));
            }
            return true;
        }
    }
}
