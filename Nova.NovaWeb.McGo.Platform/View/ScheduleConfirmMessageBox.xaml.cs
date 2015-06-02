using Nova.NovaWeb.McGo.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nova.NovaWeb.McGo.Platform.View
{
    /// <summary>
    /// ScheduleConfirmMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class ScheduleConfirmMessageBox : NebulaWindow
    {
        public ScheduleConfirmMessageBox()
        {
            InitializeComponent();
        }

        public bool? IsAsyn { get; set; }

        private void PlymButton_Click(object sender, RoutedEventArgs e)
        {
            IsAsyn = false;
            this.DialogResult = false;
        }

        private void CplymButton_Click(object sender, RoutedEventArgs e)
        {
            IsAsyn = true;
            this.DialogResult = true;
        }
    }
}
