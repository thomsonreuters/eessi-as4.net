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

        private string _portalUrl;

        public SystemTrayForm()
        {
            InitializeComponent();

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

            _icon.ContextMenu.Popup += (sender, args) =>
            {
                ServiceControllerStatus st = GetCurrentWindowsServiceStatus();

                if (_icon.ContextMenu.MenuItems.ContainsKey("Start")
                    && st != ServiceControllerStatus.StopPending
                    && st != ServiceControllerStatus.Stopped)
                {
                    SetContextMenuForStoppingWindowsService();
                }
                else if (_icon.ContextMenu.MenuItems.ContainsKey("Stop")
                         && st != ServiceControllerStatus.StartPending
                         && st != ServiceControllerStatus.Running)
                {
                    SetContextMenuForStartingWindowsService();
                }
            };
        }

        private static ServiceControllerStatus GetCurrentWindowsServiceStatus()
        {
            using (var controller = new ServiceController("AS4Service"))
            {
                return controller.Status;
            }
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
            StartWindowsService();
            SetContextMenuForStoppingWindowsService();
        }

        private static void StartWindowsService()
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
        }

        private void SetContextMenuForStoppingWindowsService()
        {
            _icon.ContextMenu
                 .MenuItems
                 .Remove(
                     _icon.ContextMenu
                          .MenuItems
                          .OfType<MenuItem>()
                          .First(m => m.Text == @"Start"));

            _icon.ContextMenu
                 .MenuItems
                 .Add(index: 0, item: new MenuItem("Stop", OnStop) { Name = "Stop" });
        }

        private void OnStop(object sender, EventArgs e)
        {
            StopWindowsService();
            SetContextMenuForStartingWindowsService();
        }

        private static void StopWindowsService()
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
        }

        private void SetContextMenuForStartingWindowsService()
        {
            _icon.ContextMenu
                 .MenuItems
                 .Remove(
                     _icon.ContextMenu
                          .MenuItems
                          .OfType<MenuItem>()
                          .First(m => m.Text == @"Stop"));

            _icon.ContextMenu
                 .MenuItems
                 .Add(index: 0, item: new MenuItem("Start", OnStart) { Name = "Start" });
        }

        private void OnOpenPortal(object sender, EventArgs e)
        {
            Process.Start(_portalUrl);
        }

        private void OnConfigure(object sender, EventArgs e)
        {
            using (var form = new ConfigurePortalForm())
            {
                form.PortalUrl = _portalUrl;
                DialogResult dialogResult = form.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    _portalUrl = form.PortalUrl;

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
