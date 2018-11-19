using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;

namespace Eu.EDelivery.AS4.WindowsService.SystemTray
{
    [RunInstaller(runInstaller: true)]
    public partial class SystemTrayInstaller : Installer
    {
        /// <summary>
        /// Raises the <see cref="E:System.Configuration.Install.Installer.AfterInstall" /> event.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property have completed their installations. </param>
        protected override void OnAfterInstall(IDictionary savedState)
        {
            Process.Start(Path.GetDirectoryName(this.Context.Parameters["AssemblyPath"]) + @"\Eu.EDelivery.AS4.WindowsService.SystemTray.exe");
        }

        /// <summary>
        /// When overridden in a derived class, removes an installation.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after the installation was complete. </param>
        /// <exception cref="T:System.ArgumentException">The saved-state <see cref="T:System.Collections.IDictionary" /> might have been corrupted. </exception>
        /// <exception cref="T:System.Configuration.Install.InstallException">An exception occurred while uninstalling. This exception is ignored and the uninstall continues. However, the application might not be fully uninstalled after the uninstallation completes. </exception>
        public override void Uninstall(IDictionary savedState)
        {
            foreach (Process p in Process.GetProcessesByName("Eu.EDelivery.AS4.WindowsService.SystemTray"))
            {
                if (!p.HasExited)
                {
                    p.Kill();
                }
            }
        }
    }
}