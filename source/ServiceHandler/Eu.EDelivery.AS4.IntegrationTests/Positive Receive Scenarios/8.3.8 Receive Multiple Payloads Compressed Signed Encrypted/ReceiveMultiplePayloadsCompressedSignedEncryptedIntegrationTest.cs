using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._8_Receive_Multiple_Payloads_Compressed_Signed_Encrypted
{
    /// <summary>
    /// Testing the Application with multiple payloads compressed, signed and encypted
    /// </summary>
    public class ReceiveMultiplePayloadsCompressedSignedEncryptedIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenReceiveMultiplePayloadsSignedSucceeds()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);

            // Arrange
            CopyPModeToHolodeckA("8.3.8-pmode.xml");

            // Act
            CopyMessageToHolodeckA("8.3.8-sample.mmd");

            // Assert
            Assert.True(PollingAt(AS4FullInputPath), "Receive Multiple Payloads Compressed, Signed and Encrypted Integration Test failed");
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
