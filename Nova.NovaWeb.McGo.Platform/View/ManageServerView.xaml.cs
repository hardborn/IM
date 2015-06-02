using Nova.NovaWeb.McGo.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nova.NovaWeb.McGo.Platform.View
{
    /// <summary>
    /// ManageServerView.xaml 的交互逻辑
    /// </summary>
    public partial class ManageServerView : NebulaWindow
    {
        private string _serverURL;
        private int _isRefresh = 0;

        public ManageServerView()
        {
            InitializeComponent();
        }


        public ManageServerView(string serverURL)
            :this()
        {
            _serverURL = serverURL;
            Environment.SetEnvironmentVariable("WEBKIT_IGNORE_SSL_ERRORS", "1");
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

           // webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.Url = new Uri( _serverURL);           
            
        }


    public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

        //private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        //{
        //    ((System.Windows.Forms.WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        //}

        //private void Window_Error(object sender, HtmlElementErrorEventArgs e)
        //{
        //    // Ignore the error and suppress the error dialog box. 
        //    e.Handled = true;
        //}


  
        //    //对错误进行处理
        //void Window_Error(object sender, System.Windows.Forms.HtmlElementErrorEventArgs e)
        //{
        //    // 自己的处理代码
        //    e.Handled = true;
        //}

        //void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    if (_isRefresh < 1)
        //    {
        //        webBrowser.Refresh();
        //        _isRefresh++;
        //    }
        //}

    }
}
