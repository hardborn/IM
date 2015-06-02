using Nova.MediaItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Common
{
    public class MediaModel
    {
        private string _filePath = string.Empty;
        private MediaType _type;

        public MediaModel(IMedia media)
        {
            if (media == null)
                return;
            _type = media.Type;

            var fileMedia = media as FileMedia;
            if(fileMedia == null )
                return ;
            _filePath = fileMedia.Path;
        }

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

        public MediaType Type
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
    }
}
