using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Eu.EDelivery.AS4.MessageSubmitter
{
    /// <summary>
    /// Interaction logic for BrowseFileControl.xaml
    /// </summary>
    public partial class BrowseFileControl : UserControl
    {

        public static readonly DependencyProperty SelectedFileNameProperty =
           DependencyProperty.Register("SelectedFileName", typeof(string), typeof(BrowseFileControl));

        public string SelectedFileName
        {
            get
            {
                return (string)GetValue(SelectedFileNameProperty);
            }
            set
            {
                SetValue(SelectedFileNameProperty, value);
            }
        }

        public BrowseFileControl()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            if (!String.IsNullOrEmpty(FilenameTextBox.Text))
            {
                var initialDirectory = System.IO.Path.GetDirectoryName(FilenameTextBox.Text);
                if (!String.IsNullOrEmpty(initialDirectory))
                {
                    dlg.InitialDirectory = initialDirectory;
                }
            }

            if (dlg.ShowDialog() ?? false)
            {
                SelectedFileName = dlg.FileName;
            }            
        }
    }
}
