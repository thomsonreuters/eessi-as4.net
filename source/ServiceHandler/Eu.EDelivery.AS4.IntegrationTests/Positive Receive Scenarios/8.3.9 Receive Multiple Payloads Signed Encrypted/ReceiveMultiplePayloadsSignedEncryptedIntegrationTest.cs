using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._9_Receive_Multiple_Payloads_Signed_Encrypted
{
    /// <summary>
    /// Testing the Application with multiple payloads signed and encrypted
    /// </summary>
    public class ReceiveMultiplePayloadsSignedEncryptedIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenReceiveMultiplePayloadsSignedSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.3.9-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.3.9-sample.mmd");

            // Assert
            Assert.True(PollingAt(AS4FullInputPath, fileCount: 3), "Receive Multiple Payloads Encrypted Integration Test failed");
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
