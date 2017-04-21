using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    /// <summary>
    /// Responsible to perform the operations within the AS4 Component.
    /// </summary>
    public class AS4Component : IDisposable
    {
        private Process _as4ComponentProcess;

        /// <summary>
        /// Payload file send as primary payload.
        /// </summary>
        public static FileInfo SubmitSinglePayloadImage => new FileInfo(Path.GetFullPath(@".\" + Properties.Resources.submitmessage_single_payload_path));

        /// <summary>
        /// Payload file send as secondary payload.
        /// </summary>
        public static FileInfo SubmitSecondPayloadXml => new FileInfo(Path.GetFullPath($".{Properties.Resources.submitmessage_second_payload_path}"));

        /// <summary>
        /// Gets the host address on which the AS4 Component will be run.
        /// </summary>
        public static string HostAddress
        {
            get
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                throw new Exception("Local IP Address Not Found!");
            }
        }

        /// <summary>
        /// Start AS4 Component
        /// </summary>
        public void Start()
        {
            _as4ComponentProcess = Process.Start("Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe");

            if (_as4ComponentProcess != null)
            {
                Console.WriteLine($@"Application Started with Process Id: {_as4ComponentProcess.Id}");
            }
        }

        /// <summary>
        /// Override the default 'settings.xml' with a new file with the given <paramref name="newSettingsName"/>.
        /// </summary>
        /// <param name="newSettingsName">Name of the new 'settings' used in the Integration Test.</param>
        public void OverrideSettings(string newSettingsName)
        {
            TryDeleteConfigFile("settings-temp.xml");

            TryMoveConfigFile("settings.xml", "settings-temp.xml");
            TryMoveConfigFile(newSettingsName, "settings.xml");
        }

        private static void TryMoveConfigFile(string sourceFile, string destFile)
        {
           TryFileOperation(() => File.Move, sourceFile, destFile);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_as4ComponentProcess.HasExited)
            {
                _as4ComponentProcess.Kill();
            }

            TryDeleteConfigFile("settings-temp.xml");
            TryDeleteConfigFile("settings.xml");
            TryCopyConfigFile(@"integrationtest-settings\settings.xml", @"settings.xml");
        }

        private static void TryDeleteConfigFile(string fileName)
        {
            TryFileOperation(() => (source, dest) => File.Delete(source), fileName);
        }

        private static void TryCopyConfigFile(string sourceFile, string destFile)
        {
            TryFileOperation(() => File.Copy, sourceFile, destFile);
        }

        private static void TryFileOperation(Func<Action<string, string>> operation, string source, string dest = null)
        {
            try
            {
                operation()(Path.GetFullPath($@".\config\{source}"), Path.GetFullPath($@".\config\{dest}"));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
