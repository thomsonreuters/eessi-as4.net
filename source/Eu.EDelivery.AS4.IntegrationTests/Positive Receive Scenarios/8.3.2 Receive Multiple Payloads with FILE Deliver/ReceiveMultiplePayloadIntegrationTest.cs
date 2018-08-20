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

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.3.2-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.3.2-sample.mmd");

            // Assert
            Assert.True(PollingAt(AS4FullInputPath, fileCount: 3, validation: ValidateDelivery), "No DeliverMessage and Payloads found at AS4.NET Component");
            Assert.True(PollingAt(Holodeck.HolodeckALocations.InputPath), "No Receipt found at Holodeck A");
        }

        private void ValidateDelivery(IEnumerable<FileInfo> files)
        {
            Assert.All(files.Where(f => f.Extension == ".jpg"), f => Assert.Equal(f.Length, Holodeck.HolodeckAJpegPayload.Length));
            Assert.Equal(2, files.Count(f => f.Extension == ".xml"));
        }
    }
}