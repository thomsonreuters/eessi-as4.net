using System.Collections.Generic;
using System.IO;
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
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.3.7-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.3.7-sample.mmd");

            // Assert
           Assert.True(PollingAt(Holodeck.HolodeckALocations.InputPath, fileCount: 3));
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            Holodeck.AssertPayloadsOnHolodeckA(new DirectoryInfo(AS4FullInputPath).GetFiles("*.jpg"));
            Holodeck.AssertXmlFilesOnHolodeckA(new DirectoryInfo(AS4FullInputPath).GetFiles("*.xml"));
            Holodeck.AssertReceiptOnHolodeckA();
        }
    }
}
