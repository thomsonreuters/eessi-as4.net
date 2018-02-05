using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.3.8-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.3.8-sample.mmd");

            // Assert
            Assert.True(PollingAt(AS4FullInputPath, fileCount: 3, validation: ValidateDelivery), "No DeliverMessage and Payloads found at AS4.NET Component");
            Assert.True(PollingAt(Holodeck.HolodeckALocations.InputPath), "No Receipt found at Holodeck A");
        }

        private void ValidateDelivery(IEnumerable<FileInfo> files)
        {
            Assert.All(files.Where(f => f.Extension == ".jpg"), f => Assert.Equal(f.Length, Holodeck.HolodeckAJpegPayload.Length));
            Assert.Equal(files.Count(f => f.Extension == ".xml"), 2);
        }
    }
}
