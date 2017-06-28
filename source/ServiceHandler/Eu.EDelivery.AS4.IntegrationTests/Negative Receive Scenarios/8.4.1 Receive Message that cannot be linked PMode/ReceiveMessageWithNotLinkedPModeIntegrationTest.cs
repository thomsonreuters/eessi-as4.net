using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Receive_Scenarios._8._4._1_Receive_Message_that_cannot_be_linked_PMode
{
    /// <summary>
    /// Testing the Application with a received message
    /// that doesn't link an existing Receiving PMode
    /// </summary>
    public class ReceiveMessageWithNotLinkedPModeIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void ThenSendingSinglePayloadSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.4.1-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.4.1-sample.mmd");

            // Assert
            Assert.True(PollingAt(Properties.Resources.holodeck_A_input_path, "*.xml"), "Receive Message with not linked PMode Integration Test failed");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            Holodeck.AssertErrorOnHolodeckA(ErrorCode.Ebms0001);
        }
    }
}
