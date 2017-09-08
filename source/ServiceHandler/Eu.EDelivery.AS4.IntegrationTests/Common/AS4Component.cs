using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Xunit;

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
            if (!_settingsOverriden)
            {
                TryMoveConfigFile("settings.xml", "settings-original.xml", true);
                TryCopyConfigFile(@"integrationtest-settings\settings.xml", @"settings.xml", true);
            }

            _as4ComponentProcess = Process.Start("Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe");

            if (_as4ComponentProcess != null)
            {
                Console.WriteLine($@"Application Started with Process Id: {_as4ComponentProcess.Id}");
            }
        }

        private bool _settingsOverriden = false;

        /// <summary>
        /// Override the default 'settings.xml' with a new file with the given <paramref name="newSettingsName"/>.
        /// </summary>
        /// <param name="newSettingsName">Name of the new 'settings' used in the Integration Test.</param>
        public void OverrideSettings(string newSettingsName)
        {
            if (_as4ComponentProcess != null)
            {
                throw new InvalidOperationException("Cannot override config settings when AS4 MSH is already running.");
            }

            _settingsOverriden = true;

            TryMoveConfigFile("settings.xml", "settings-original.xml", true);
            TryMoveConfigFile(newSettingsName, "settings.xml", true);
        }

        private static void TryMoveConfigFile(string sourceFile, string destFile, bool overwriteExisting = false)
        {
            if (overwriteExisting)
            {
                TryDeleteConfigFile(destFile);
            }
            TryFileOperation(() => File.Move, sourceFile, destFile);
        }

        /// <summary>
        /// Puts the message.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        public void PutMessage(string messageName)
        {
            string sourceFile = $"{IntegrationTestTemplate.AS4IntegrationMessagesPath}\\{messageName}";
            string destinationFile = $"{IntegrationTestTemplate.AS4FullOutputPath}\\{messageName}";

            Console.WriteLine($@"Putting {destinationFile}");

            File.Copy(
                sourceFileName: sourceFile,
                destFileName: destinationFile,
                overwrite: true);
        }

        /// <summary>
        ///  Assert on a received Receipt on the AS4 Component.
        /// </summary>
        public void AssertReceipt()
        {
            string receiptPath = Path.GetFullPath($@".\{Properties.Resources.as4_component_receipts_path}");
            FileInfo receipt = new DirectoryInfo(receiptPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }

        /// <summary>
        /// Assert if the given <paramref name="receivedPayload" /> matches the 'Earth' payload.
        /// </summary>
        /// <param name="receivedPayload"></param>
        public void AssertEarthPayload(FileInfo receivedPayload)
        {
            var sendPayload = new FileInfo(Path.GetFullPath($".\\{Properties.Resources.submitmessage_single_payload_path}"));

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (!_as4ComponentProcess.HasExited)
            {
                _as4ComponentProcess.Kill();
            }

            TryMoveConfigFile("settings-original.xml", "settings.xml", true);
        }

        private static void TryDeleteConfigFile(string fileName)
        {
            TryFileOperation(() => (source, dest) => File.Delete(source), fileName);
        }

        private static void TryCopyConfigFile(string sourceFile, string destFile, bool overwriteExisting)
        {
            if (overwriteExisting)
            {
                TryDeleteConfigFile(destFile);
            }
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
