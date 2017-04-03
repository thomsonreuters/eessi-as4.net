using System;
using System.IO;
using System.Windows.Forms;
using PayloadService.Connector;

namespace UploadTool
{
    public partial class MainForm : Form
    {
        private PayloadConnector _connector;

        public MainForm()
        {
            InitializeComponent();
            ServiceLocationCombobox.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _connector = PayloadConnector.Connect(ServiceLocationCombobox.Text);

            ContainerPanel.Enabled = true;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dlg.FileName;
                }
            }
        }

        private async void UploadButton_Click(object sender, EventArgs e)
        {
            var result = await _connector.UploadFile(textBox1.Text);

            if (result.Success)
            {
                HistoryListbox.Items.Add(result.Location);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (HistoryListbox.SelectedIndex == -1)
            {
                return;
            }

            var id = HistoryListbox.SelectedItem as string;

            using (var dlg = new FolderBrowserDialog())
            {
                string rootFolder = @"c:\temp\downloadtest";

                if (Directory.Exists(rootFolder))
                {
                    dlg.SelectedPath = rootFolder;                      
                }
                else
                {
                    dlg.RootFolder = Environment.SpecialFolder.Desktop;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var result = await _connector.DownloadFile(id);

                    if (result.Success)
                    {
                        var targetPath = Path.Combine(dlg.SelectedPath, result.OriginalFilename);

                        using (var sr = new BinaryReader(result.Content))
                        {
                            File.WriteAllBytes(targetPath, sr.ReadBytes((int)result.Content.Length));
                        }

                        string argument = "/select, \"" + targetPath + "\"";

                        System.Diagnostics.Process.Start("explorer.exe", argument);

                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage);
                    }
                }
            }
        }
    }
}
