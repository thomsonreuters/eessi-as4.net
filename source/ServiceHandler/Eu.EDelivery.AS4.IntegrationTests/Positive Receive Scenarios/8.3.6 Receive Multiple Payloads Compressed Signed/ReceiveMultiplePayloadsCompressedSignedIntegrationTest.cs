using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._6_Receive_Multiple_Payloads_Compressed_Signed
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveMultiplePayloadsCompressedSignedIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenReceiveMultiplePayloadsCompressedSignedSucceeds()
        {
            // Before
            CleanUpFiles(AS4FullInputPath);

            AS4Component.Start();

            // Arrange
            CopyPModeToHolodeckA("8.3.6-pmode.xml");

            // Act
            CopyMessageToHolodeckA("8.3.6-sample.mmd");

            // Assert
            Assert.True(PollingAt(AS4FullInputPath));
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            Holodeck.AssertPayloadsOnHolodeckA(new DirectoryInfo(AS4FullInputPath).GetFiles("*.jpg"));
            Holodeck.AssertXmlFilesOnHolodeckA(new DirectoryInfo(AS4FullInputPath).GetFiles("*.xml"));
            Holodeck.AssertReceiptOnHolodeckA();
        }
    }
}