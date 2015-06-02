using GalaSoft.MvvmLight.Messaging;
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
    /// SearchBox.xaml 的交互逻辑
    /// </summary>
    public partial class SearchBox : UserControl
    {
        ImageSource bs1;
        ImageSource bs2;

        public SearchBox()
        {
            InitializeComponent();
            //this.DataContext = this;
            this.reset.MouseLeftButtonDown += reset_Click;
            this.tb.TextChanged += tb_TextChanged;
            bs1 = this.reset.Source;
            bs2 = new BitmapImage(new Uri("pack://application:,,,/MC-go;component/Resources/Images/reset.png"));
        }



        public string KeyWord
        {
            get { return (string)GetValue(KeyWordProperty); }
            set { SetValue(KeyWordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyWord.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyWordProperty =
            DependencyProperty.Register("KeyWord", typeof(string), typeof(SearchBox), new PropertyMetadata(string.Empty, new PropertyChangedCallback(KeyWordPropertyChanged)));

        private static void KeyWordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {

            }
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

            Messenger.Default.Send<NotificationMessage>(new NotificationMessage(tb.Text), "KeyWordChanged");

            //ISearchable viewModel = this.DataContext as ISearchable;
            //if (viewModel != null)
            //{
            //    viewModel.Search(tb.Text);
            //}

        }

        void reset_Click(object sender, RoutedEventArgs e)
        {
            tb.Text = string.Empty;
        }
    }
}
