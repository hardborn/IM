using Nova.NovaWeb.McGo.Platform.Model;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nova.NovaWeb.McGo.Platform.Control
{
    /// <summary>
    /// ScheduleSearchBox.xaml 的交互逻辑
    /// </summary>
    public partial class ScheduleSearchBox : UserControl
    {
        ImageSource bs1;
        ImageSource bs2;
        public ScheduleSearchBox()
        {
            InitializeComponent();
            this.reset.MouseLeftButtonDown += reset_Click;
            this.tb.TextChanged += tb_TextChanged;
            bs1 = this.reset.Source;
            bs2 = new BitmapImage(new Uri("pack://application:,,,/MC-go;component/Resources/Images/reset.png"));
        }






        void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb.Text))
            {
                reset.Source = bs1;
            }
            else
            {
                reset.Source = bs2;
            }

            ISearchable viewModel = this.DataContext as ISearchable;
            if (viewModel != null)
            {
                viewModel.Search(tb.Text);
            }

        }

        void reset_Click(object sender, RoutedEventArgs e)
        {
            tb.Text = string.Empty;
        }
    }
}
