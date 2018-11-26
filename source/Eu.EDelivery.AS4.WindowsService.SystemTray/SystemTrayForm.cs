using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;
using Eu.EDelivery.AS4.WindowsService.SystemTray.Properties;

namespace Eu.EDelivery.AS4.WindowsService.SystemTray
{
    public partial class SystemTrayForm : Form
    {
        private readonly NotifyIcon _icon;

        private Uri _portalUrl;

        public SystemTrayForm()
        {
            InitializeComponent();

            // TODO: maybe we should determine the current status of the windows service before assuming we haven't started it manually.
            _icon = new NotifyIcon
            {
                Visible = true,
                Icon = Resources.favicon,
                ContextMenu = new ContextMenu(
                    new []
                    {
                        new MenuItem("Start", OnStart)
                        {
                            Name = "Start"
                        },
                        new MenuItem("Open Portal", OnOpenPortal)
                        {
                            Name = "Open Portal",
                            Enabled = false
                        },
                        new MenuItem("Configure", OnConfigure)
                        {
                            Name = "Configure"
                        }
                    })
            };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void OnStart(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                using (var controller = new ServiceController("AS4Service"))
                {
                    if (controller.Status != ServiceControllerStatus.Running)
                    {
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running);
                    }
                }
            });

            _icon.ContextMenu
                 .MenuItems
                 .Remove(
                     _icon.ContextMenu
                          .MenuItems
                          .OfType<MenuItem>()
                          .First(m => m.Text == "Start"));

            _icon.ContextMenu
                 .MenuItems
                 .Add(index: 0, item: new MenuItem("Stop", OnStop));
        }

        private void OnStop(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                using (var controller = new ServiceController("AS4Service"))
                {
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                }
            });

            _icon.ContextMenu
                 .MenuItems
                 .Remove(
                     _icon.ContextMenu
                          .MenuItems
                          .OfType<MenuItem>()
                          .First(m => m.Text == "Stop"));

            _icon.ContextMenu
                 .MenuItems
                 .Add(index: 0, item: new MenuItem("Start", OnStart));
        }

        private void OnOpenPortal(object sender, EventArgs e)
        {
            Task.Run(() => Process.Start(_portalUrl.OriginalString));
        }

        private void OnConfigure(object sender, EventArgs e)
        {
            var form = new ConfigurePortalForm();
            DialogResult dialogResult = form.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                if (String.IsNullOrWhiteSpace(form.PortalUrl))
                {
                    MessageBox.Show(
                        @"Cannot configure AS4.NET portal with a empty url",
                        @"Configuration failure",
                        MessageBoxButtons.OK);
                }
                else if (!Uri.TryCreate(form.PortalUrl, UriKind.RelativeOrAbsolute, out Uri portalUrl))
                {
                    MessageBox.Show(
                        @"Cannot configure AS4.NET portal with an input that's not a valid URL",
                        @"Configuration failure",
                        MessageBoxButtons.OK);
                }
                else
                {
                    _portalUrl = portalUrl;

                    foreach (MenuItem menuItem in
                        _icon.ContextMenu
                             .MenuItems
                             .Find("Open Portal", searchAllChildren: false))
                    {
                        menuItem.Enabled = true;
                    }
                }
            }
        }
    }
}
