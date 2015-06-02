using Nova.Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nova.NovaWeb.Windows.View
{
    /// <summary>
    /// CommandLogView.xaml 的交互逻辑
    /// </summary>
    public partial class CommandLogView : Window
    {
        private string _languagePath;
        private string _cultureName;

        public CommandLogView()
        {
            InitializeComponent();
            this.Loaded += CommandLogView_Loaded;
        }

        public CommandLogView(string languagePath, string cultureName)
            : this()
        {
            _languagePath = languagePath;
            _cultureName = cultureName;
        }

        void CommandLogView_Loaded(object sender, RoutedEventArgs e)
        {
            string languageFilePath = Path.Combine(_languagePath, _cultureName, "InquireCommandModule." + _cultureName + ".xaml");
            MultiLanguageUtils.LoadLanguage(languageFilePath, Application.Current.Resources);

            ICollectionView dataView = CollectionViewSource.GetDefaultView(logDataGrid.ItemsSource);
            dataView.SortDescriptions.Clear();

            dataView.SortDescriptions.Add(new SortDescription("OperationTime", ListSortDirection.Ascending));
            dataView.Refresh();
        }

        private void TitleHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
