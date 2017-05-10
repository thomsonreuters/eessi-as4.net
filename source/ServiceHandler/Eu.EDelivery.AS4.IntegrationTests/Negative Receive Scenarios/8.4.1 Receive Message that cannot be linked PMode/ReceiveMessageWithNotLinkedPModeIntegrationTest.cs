using System;
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
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            CopyPModeToHolodeckA("8.4.1-pmode.xml");

            // Act
            CopyMessageToHolodeckA("8.4.1-sample.mmd");

            // Assert
            bool areFilesFound = PollingAt(Properties.Resources.holodeck_A_input_path, "*.xml");
            Assert.True(areFilesFound, "Receive Message with not linked PMode Integration Test failed");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            var holodeck = new Holodeck();
            holodeck.AssertErrorOnHolodeckA(ErrorCode.Ebms0001);
        }
    }
}
