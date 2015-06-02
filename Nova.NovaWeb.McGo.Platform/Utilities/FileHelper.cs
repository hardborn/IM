using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.Utilities
{
    public class FileHelper
    {
        public static void DeleteFiles(string directoryPath)
        {
            DirectoryInfo fdir = new DirectoryInfo(directoryPath);
            if(File.Exists(directoryPath) == false)
            {
                return;
            }

            FileInfo[] file = fdir.GetFiles();
            if (file == null)
                return;
            foreach (var item in file)
            {
                File.Delete(item.FullName);
            }
        }
    }
}
