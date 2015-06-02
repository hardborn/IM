using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Nova.NovaWeb.McGo.Platform
{
    public class Wfp32Window : IWin32Window
    {
        public IntPtr Handle
        {
            get;
            private set;
        }
        public Wfp32Window(Window window)
        {
            Handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        }
    }
}
