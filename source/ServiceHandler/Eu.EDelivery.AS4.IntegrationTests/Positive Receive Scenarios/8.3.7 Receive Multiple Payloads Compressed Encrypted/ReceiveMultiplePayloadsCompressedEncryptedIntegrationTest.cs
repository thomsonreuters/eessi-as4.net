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
        [Retry(maxRetries: 2)]
        public void ThenReceiveMultiplePayloadsSignedSucceeds()
        {
            // Before
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            CopyPModeToHolodeckA("8.3.7-pmode.xml");

            // Act
            CopyMessageToHolodeckA("8.3.7-sample.mmd");

            // Assert
            bool areFilesFound = PollingAt(Properties.Resources.holodeck_A_input_path);
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

            var holodeck = new Holodeck();
            holodeck.AssertReceiptOnHolodeckA();
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
