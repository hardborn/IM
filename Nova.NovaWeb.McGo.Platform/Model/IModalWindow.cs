using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Nova.NovaWeb.McGo.Platform.Model
{
    public interface IModalWindow
    {
        bool? DialogResult { get; set; }
        event EventHandler Closed;
        void Show();
        bool? ShowDialog();
        bool ShowInTaskbar { get; set; }
        Window Owner { get; set; }
        object DataContext { get; set; }
        void Close();
    }

}
