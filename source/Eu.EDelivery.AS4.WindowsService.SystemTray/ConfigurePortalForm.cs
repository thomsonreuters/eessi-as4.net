using System;
using System.Windows.Forms;

namespace Eu.EDelivery.AS4.WindowsService.SystemTray
{
    public partial class ConfigurePortalForm : Form
    {
        public ConfigurePortalForm()
        {
            InitializeComponent();

            txtPortal.Focus();
            btnOK.Click += BtnOK_Click;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(PortalUrl))
            {
                MessageBox.Show(
                    @"Cannot configure AS4.NET portal with a empty url",
                    @"Configuration failure",
                    MessageBoxButtons.OK);

                DialogResult = DialogResult.None;
            }
            else if (!PortalUrl.StartsWith("http"))
            {
                MessageBox.Show(
                    @"Cannot configure AS4.NET portal with an input that's not a valid HTTP URL",
                    @"Configuration failure",
                    MessageBoxButtons.OK);

                DialogResult = DialogResult.None;
            }
        }

        public string PortalUrl => txtPortal.Text;
    }
}
