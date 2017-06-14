using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._2_Receive_Multiple_Payloads_with_FILE_Deliver
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveMultiplePayloadIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenReceiveMultiplePayloadsSucceeds()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);

            // Arrange
            CopyPModeToHolodeckA("8.3.2-pmode.xml");

            // Act
            CopyMessageToHolodeckA("8.3.2-sample.mmd");

            // Assert
            Assert.True(PollingAt(AS4FullInputPath));
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayloads(new DirectoryInfo(AS4FullInputPath).GetFiles("*.jpg"));
            AssertXmlFiles(new DirectoryInfo(AS4FullInputPath).GetFiles("*.xml"));

            new Holodeck().AssertReceiptOnHolodeckA();
        }

        private static void AssertPayloads(IEnumerable<FileInfo> files)
        {
            var sendPayload = new FileInfo(Properties.Resources.holodeck_payload_path);

            Assert.All(files, f => Assert.Equal(sendPayload.Length, f.Length));
        }

        private static void AssertXmlFiles(IEnumerable<FileInfo> files)
        {
            Assert.Equal(2, files.Count());
            Console.WriteLine($@"There're {files.Count()} incoming Xml Documents found");
        }
    }
}