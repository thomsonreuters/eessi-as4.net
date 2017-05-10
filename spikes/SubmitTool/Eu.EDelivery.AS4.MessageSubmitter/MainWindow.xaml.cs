using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Watchers;

namespace Eu.EDelivery.AS4.MessageSubmitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SubmitMessageViewModel _viewModel = new SubmitMessageViewModel();

        private PModeWatcher<SendingProcessingMode> _sendPModeWatcher;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = _viewModel;

            PayloadListView.ItemsSource = _viewModel.PayloadInformation;
        }

        private void AddPayloadButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.PayloadInformation.Add(new PayloadInfoViewModel());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SubmitMessageCreator.CreateSubmitMessages(_viewModel);
            MessageBox.Show($"{_viewModel.NumberOfSubmitMessages} submitmessages created in {_viewModel.SubmitLocation}");
        }

        private void PopulateSendingPModeCombobox()
        {
            if (_sendPModeWatcher != null)
            {
                _sendPModeWatcher.Stop();
                PModeCombobox.ItemsSource = new string[] { };
            }

            if (String.IsNullOrWhiteSpace(SendingPModeLocationTextBox.Text))
            {
                return;
            }

            _sendPModeWatcher = new PModeWatcher<SendingProcessingMode>(SendingPModeLocationTextBox.Text);
            _sendPModeWatcher.Start();

            PModeCombobox.ItemsSource = _sendPModeWatcher.GetPModes().Select(p => p.Id).OrderBy(id => id).ToArray();
        }

        private void BrowsePModeLocationButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SendingPModeLocationTextBox.Text = dlg.SelectedPath;
                    PopulateSendingPModeCombobox();
                }
            }
        }
    }
}
