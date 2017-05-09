using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._7_Receive_Multiple_Payloads_Compressed_Encrypted
{
    /// <summary>
    /// Testing the Applicaiton with multiple payloads compressed and encrypted
    /// </summary>
    public class ReceiveMultiplePayloadsCompressedEncryptedIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.3.7-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveMultiplePayloadsCompressedEncryptedIntegrationTest()
        {
            this._holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            this._destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            this._holodeck = new Holodeck();
        }

        [Retry(maxRetries: 2)]
        public void ThenReceiveMultiplePayloadsSignedSucceeds()
        {
            // Before
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            this.AS4Component.Start();
            base.CleanUpFiles(AS4FullInputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            base.CopyPModeToHolodeckA("8.3.7-pmode.xml");

            // Act
            File.Copy(this._holodeckMessagesPath, this._destFileName);

            // Assert
            bool areFilesFound = base.PollingAt(Properties.Resources.holodeck_A_input_path);
            if (areFilesFound)
            {
                Console.WriteLine(@"Receive Multiple Payloads Compressed and Encrypted Integration Test succeeded!");
            }
            
            Assert.True(areFilesFound, "Receive Multiple Payloads Compressed Encrypted failed!");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayloads(new DirectoryInfo(AS4FullInputPath).GetFiles("*.jpg"));
            AssertXmlFiles(new DirectoryInfo(AS4FullInputPath).GetFiles("*.xml"));

            _holodeck.AssertReceiptOnHolodeckA();
        }

        private static void AssertPayloads(IEnumerable<FileInfo> files)
        {
            var sendPayload = new FileInfo(Properties.Resources.holodeck_payload_path);

            foreach (FileInfo receivedPayload in files)
            {
                if (receivedPayload != null)
                {
                    Assert.Equal(sendPayload.Length, receivedPayload.Length);
                }
            }
        }

        private static void AssertXmlFiles(IEnumerable<FileInfo> files)
        {
            int count = files.Count();
            if (count == 0)
            {
                return;
            }

            Assert.NotEmpty(files);
            Console.WriteLine($@"There're {count} incoming Xml Documents found");
        }
    }
}
