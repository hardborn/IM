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
using Nova.NovaWeb.Windows.ViewModels;
using Nova.Globalization;
using System.Collections.ObjectModel;
using Nova.NovaWeb.Common;
using System.Collections;

namespace Nova.NovaWeb.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InquireCommandWindow : Window
    {
        public InquireCommandWindow()
        {
            InitializeComponent();
            this.Loaded += InquireCommandWindow_Loaded;
        }

        void InquireCommandWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InquiryWindowViewModel viewModel = this.DataContext as InquiryWindowViewModel;
            MultiLanguageUtils.LoadLanguage(viewModel.GetLanguageResourcePath(), Application.Current.Resources);
        }


        private void TitleHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            if (e.ClickCount == 2)
            {
                this.WindowState = this.WindowState == System.Windows.WindowState.Maximized ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;
            }
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

    }
}
