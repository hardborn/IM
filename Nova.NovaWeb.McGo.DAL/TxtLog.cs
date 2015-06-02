using Nova.NovaWeb.McGo.BLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.DAL
{
    public class TxtLogger : ILog, IDisposable
    {
        private TextWriter writer;

        public TxtLogger()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Log";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            this.writer = TextWriter.Synchronized(File.CreateText(path + @"\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".log"));
        }

        public void Dispose()
        {
            this.writer.Close();
            this.writer.Dispose();
        }

        public void Log(Exception ex)
        {
            Exception innerException = ex;
            while (innerException.InnerException != null)
            {
                innerException = innerException.InnerException;
            }
            this.writer.WriteLine(string.Format("[{0}]错误：{1}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"), innerException.Message));
            this.writer.WriteLine(innerException.StackTrace);
            this.writer.Flush();
        }

        public void Log(string text)
        {
            this.writer.WriteLine(string.Format("[{0}]消息：{1}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"), text));
            this.writer.Flush();
        }
    }
}
