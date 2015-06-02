using GalaSoft.MvvmLight;
using Nova.NovaWeb.McGo.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.ViewModel
{
    public class MediaViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _filePath = string.Empty;

        public MediaViewModel(MediaModel mediaModel)
        {
            if (mediaModel == null)
                return;
            _filePath = mediaModel.FilePath;
        }


        public string Id
        {
            get { return _id; }
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_filePath))
                    return string.Empty;
                else
                    return Path.GetFileName(_filePath);
            }
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath == value)
                {
                    return;
                }
                _filePath = value;
                RaisePropertyChanged("FilePath");
                RaisePropertyChanged("Name");
            }
        }
    }
}
