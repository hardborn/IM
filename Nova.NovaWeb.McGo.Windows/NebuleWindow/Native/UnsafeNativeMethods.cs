using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Nova.NovaWeb.McGo.Windows.Native
{
    /// <devdoc>http://msdn.microsoft.com/en-us/library/ms182161.aspx</devdoc>
    [SuppressUnmanagedCodeSecurity]
    public static class UnsafeNativeMethods
    {
        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/aa969518%28v=vs.85%29.aspx</devdoc>
        [DllImport("dwmapi", PreserveSig = false, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DwmIsCompositionEnabled();

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/aa969512%28v=vs.85%29.aspx</devdoc>
        [DllImport("dwmapi", PreserveSig = true, CallingConvention = CallingConvention.Winapi, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Error)]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, [In] ref MARGINS pMarInset);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/aa969524%28v=vs.85%29.aspx</devdoc>
        [DllImport("dwmapi", PreserveSig = true, CallingConvention = CallingConvention.Winapi, ExactSpelling = true)]
        public static extern int DwmSetWindowAttribute([In] IntPtr hwnd, [In] int attr, [In] ref int attrValue, [In] int attrSize);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms633572%28v=vs.85%29.aspx</devdoc>
        [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr DefWindowProc([In] IntPtr hwnd, [In] int msg, [In] IntPtr wParam, [In] IntPtr lParam);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/dd144901%28v=VS.85%29.aspx</devdoc>
        [DllImport("user32", EntryPoint = "GetMonitorInfoW", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo([In] IntPtr hMonitor, [Out] MONITORINFO lpmi);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/dd145064%28v=VS.85%29.aspx</devdoc>
        [DllImport("user32")]
        public static extern IntPtr MonitorFromWindow([In] IntPtr handle, [In] int flags);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms633545(v=vs.85).aspx</devdoc>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms647486%28v=vs.85%29.aspx</devdoc>
        [DllImport("user32", CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadStringW", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern int LoadString([In] [Optional] IntPtr hInstance, [In] uint uID, [Out] StringBuilder lpBuffer, [In] int nBufferMax);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms633528(v=vs.85).aspx</devdoc>
        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool IsWindow([In] [Optional] IntPtr hWnd);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms647985(v=vs.85).aspx</devdoc>
        [DllImport("user32")]
        public static extern IntPtr GetSystemMenu([In] IntPtr hWnd, [In] bool bRevert);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms648003(v=vs.85).aspx</devdoc>
        [DllImport("user32")]
        public static extern uint TrackPopupMenuEx([In] IntPtr hmenu, [In] uint fuFlags, [In] int x, [In] int y, [In] IntPtr hwnd, [In] [Optional] IntPtr lptpm);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms644944(v=vs.85).aspx</devdoc>
        [DllImport("user32", EntryPoint = "PostMessage", SetLastError = true)]
        private static extern bool _PostMessage([In] [Optional] IntPtr hWnd, [In] uint Msg, [In] IntPtr wParam, [In] IntPtr lParam);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms648390(v=vs.85).aspx</devdoc>
        [DllImport("user32")]
        public static extern bool GetCursorPos([Out] out Win32Point pt);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms646258(v=vs.85).aspx</devdoc>
        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetDoubleClickTime();

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms684175%28v=vs.85%29.aspx</devdoc>
        [DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadLibraryW", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr LoadLibrary([In] [MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms683152%28v=vs.85%29.aspx</devdoc>
        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary([In] IntPtr hModule);

        [DllImport("user32.dll", EntryPoint = "SetClassLong")]
        public static extern uint SetClassLongPtr32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
        public static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(int crColor);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        /// <devdoc>http://msdn.microsoft.com/en-us/library/windows/desktop/ms647636(v=vs.85).aspx</devdoc>
        [DllImport("user32.dll")]
        public static extern uint EnableMenuItem(IntPtr hMenu, uint itemId, uint uEnable);

        public static void PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam)
        {
            if (!_PostMessage(hWnd, Msg, wParam, lParam))
            {
                throw new Win32Exception();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public readonly Int32 X;
            public readonly Int32 Y;
        };

        public static int GET_X_LPARAM(IntPtr lParam)
        {
            return LOWORD(lParam.ToInt32());
        }

        public static int GET_Y_LPARAM(IntPtr lParam)
        {
            return HIWORD(lParam.ToInt32());
        }

        private static int HIWORD(int i)
        {
            return (short)(i >> 16);
        }

        private static int LOWORD(int i)
        {
            return (short)(i & 0xFFFF);
        }
    }
}
