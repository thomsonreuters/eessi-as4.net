using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    /// <summary>
    /// Responsible to perform the operations within the AS4 Component.
    /// </summary>
    public class AS4Component : IDisposable
    {
        private Process _as4ComponentProcess;

        public static readonly string AS4MessagesRootPath = Path.GetFullPath($@".\{submit_messages_path}");
        public static readonly string FullOutputPath = Path.GetFullPath($@".\{submit_output_path}");
        public static readonly string FullInputPath = Path.GetFullPath($@".\{submit_input_path}");
        public static readonly string AS4IntegrationMessagesPath = Path.GetFullPath($@".\{submit_messages_path}\integrationtest-messages");

        public static readonly string ReceiptsPath = Path.GetFullPath($@".\{as4_component_receipts_path}");
        public static readonly string ErrorsPath = Path.GetFullPath($@".\{as4_component_errors_path}");
        public static readonly string ExceptionsPath = Path.GetFullPath($@".\{as4_component_exceptions_path}");

        /// <summary>
        /// Payload file send as primary payload.
        /// </summary>
        public static FileInfo SubmitSinglePayloadImage = new FileInfo(Path.GetFullPath(@".\" + submitmessage_single_payload_path));

        /// <summary>
        /// Submit payload representation of the image.
        /// </summary>
        public static Payload SubmitPayloadImage =
            new Payload("earth", @"file:///.\messages\attachments\earth.jpg", "image/jpeg")
            {
                PayloadProperties = new[] { new PayloadProperty("Test", "Test") }
            };

        /// <summary>
        /// Payload file send as secondary payload.
        /// </summary>
        public static FileInfo SubmitSecondPayloadXml = new FileInfo(Path.GetFullPath($".{submitmessage_second_payload_path}"));

        /// <summary>
        /// Submit payload representation of the xml file.
        /// </summary>
        public static Payload SubmitPayloadXml =
            new Payload("xml-sample", @"file:///.\messages\attachments\sample.xml", "application/xml")
            {
                PayloadProperties = new[] { new PayloadProperty("Important", "Yes") }
            };

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
        /// Gets the <see cref="IConfig"/> implementation for the given <see cref="AS4Component"/>.
        /// </summary>
        /// <returns></returns>
        public IConfig GetConfiguration()
        {
            Config config = Config.Instance;

            if (config.IsInitialized == false)
            {
                config.Initialize(@"config\settings.xml");
            }

            return config;
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

            var psi = new ProcessStartInfo
            {
                FileName = "Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe",
                Arguments = "",
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Minimized
            };

            _as4ComponentProcess = Process.Start(psi);

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

        /// <summary>
        /// Puts the message.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        public void PutMessage(string messageName)
        {
            string sourceFile = $"{AS4IntegrationMessagesPath}\\{messageName}";
            string destinationFile = $"{FullOutputPath}\\{messageName}";

            Console.WriteLine($@"Putting {destinationFile}");

            File.Copy(
                sourceFileName: sourceFile,
                destFileName: destinationFile,
                overwrite: true);
        }

        /// <summary>
        /// Puts a <see cref="SubmitMessage"/> with a given reference to a <see cref="SendingProcessingMode"/>
        /// and with optional payloads references on disk so they get picked-up by the AS4 component.
        /// </summary>
        /// <param name="pmodeId">The identifier to reference a <see cref="SendingProcessingMode"/>.</param>
        public void PutSubmitMessageSinglePayload(string pmodeId)
        {
            PutSubmitMessage(pmodeId, SubmitPayloadImage);
        }

        /// <summary>
        /// Puts a <see cref="SubmitMessage"/> with a given reference to a <see cref="SendingProcessingMode"/>
        /// and with optional payloads references on disk so they get picked-up by the AS4 component.
        /// </summary>
        /// <param name="pmodeId">The identifier to reference a <see cref="SendingProcessingMode"/>.</param>
        public void PutSubmitMessageMultiplePayloads(string pmodeId)
        {
            PutSubmitMessage(pmodeId, SubmitPayloadImage, SubmitPayloadXml);
        }

        /// <summary>
        /// Puts a <see cref="SubmitMessage"/> with a given reference to a <see cref="SendingProcessingMode"/>
        /// and with optional payloads references on disk so they get picked-up by the AS4 component.
        /// </summary>
        /// <param name="pmodeId">The identifier to reference a <see cref="SendingProcessingMode"/>.</param>
        /// <param name="payloads">The sequence of submit payloads to include in the message.</param>
        public void PutSubmitMessage(
            string pmodeId,
            params Payload[] payloads)
        {
            PutSubmitMessage(pmodeId, submit => { }, payloads);
        }

        /// <summary>
        /// Puts a <see cref="SubmitMessage"/> with a given reference to a <see cref="SendingProcessingMode"/>
        /// and with optional payloads references on disk so they get picked-up by the AS4 component.
        /// </summary>
        /// <param name="pmodeId">The identifier to reference a <see cref="SendingProcessingMode"/>.</param>
        /// <param name="payloads">The sequence of submit payloads to include in the message.</param>
        /// <param name="setCustomFixtures">The function to place custom settings to the created message.</param>
        public void PutSubmitMessage(
            string pmodeId,
            Action<SubmitMessage> setCustomFixtures,
            params Payload[] payloads)
        {
            var submitMessage = new SubmitMessage
            {
                Collaboration =
                {
                    AgreementRef =
                    {
                        PModeId = pmodeId,
                        Value = "http://agreements.holodeckb2b.org/examples/agreement0"
                    },
                    ConversationId = "eu:edelivery:as4:sampleconversation",
                    Action = Constants.Namespaces.TestAction,
                    Service =
                    {
                        Type = Constants.Namespaces.TestService,
                        Value = Constants.Namespaces.TestService
                    }
                },
                Payloads = payloads
            };

            setCustomFixtures(submitMessage);

            string xml = AS4XmlSerializer.ToString(submitMessage);
            string fileName = Path.Combine(FullOutputPath, $"submit-{pmodeId}.xml");

            Console.WriteLine($@"Putting {fileName}");
            File.WriteAllText(fileName, xml);
        }

        /// <summary>
        ///  Assert on a received Receipt on the AS4 Component.
        /// </summary>
        public void AssertReceipt()
        {
            string receiptPath = Path.GetFullPath($@".\{as4_component_receipts_path}");
            FileInfo receiptFile = 
                new DirectoryInfo(receiptPath)
                    .GetFiles("*.xml")
                    .FirstOrDefault();

            Assert.True(receiptFile != null, "No Receipt found at AS4 Component");
        }

        /// <summary>
        /// Assert if the given <paramref name="receivedPayload" /> matches the 'Earth' payload.
        /// </summary>
        /// <param name="receivedPayload"></param>
        public void AssertEarthPayload(FileInfo receivedPayload)
        {
            var sendPayload = new FileInfo(Path.GetFullPath($".\\{submitmessage_single_payload_path}"));

            Assert.True(receivedPayload != null, "No submit payload found at Holodeck B");
            Assert.True(
                sendPayload.Length == receivedPayload.Length, 
                $"Send submit payload doesn't have the same length as the received payload {sendPayload.Length} != {receivedPayload.Length}");
        }

        /// <summary>
        /// Assert if the delivered payloads at Holodeck B are the same one like the submited payloads.
        /// </summary>
        public void AssertMultiplePayloadsOnHolodeckB()
        {
            FileInfo[] receivedPayloads = new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath).GetFiles();

            FileInfo sentEarth = SubmitSinglePayloadImage;
            FileInfo sentXml = SubmitSecondPayloadXml;

            FileInfo receivedEarth = receivedPayloads.FirstOrDefault(x => x.Extension == ".jpg");
            FileInfo receivedXml = receivedPayloads.FirstOrDefault(x => x.Name.Contains("sample"));

            Assert.NotNull(receivedEarth);
            Assert.NotNull(receivedXml);

            Assert.Equal(sentEarth.Length, receivedEarth.Length);
            Assert.Equal(sentXml.Length, receivedXml.Length);
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

        private static void TryMoveConfigFile(string sourceFile, string destFile, bool overwriteExisting = false)
        {
            if (overwriteExisting)
            {
                TryDeleteConfigFile(destFile);
            }

            TryFileOperation(() => File.Move, sourceFile, destFile);
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
