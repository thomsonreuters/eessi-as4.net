using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._16_Send_Message_via_Pulling
{
    public class SendMessageViaPullingTest : IntegrationTestTemplate
    {
        [Fact]
        public void HolodeckGetsReciptForPullRequest_IfRequestMatchesMpc()
        {
            // Arrange
            AS4Component.OverrideSettings("8.1.16-settings.xml");
            AS4Component.Start();

            AS4Component.PlaceMessage("8.1.16-sample.xml");

            // Act
            Holodeck.CopyPModeToHolodeckB("8.1.16-receive-pmode.xml");
            Holodeck.CopyPModeToHolodeckB("8.1.16-pmode.xml");

            // Assert
            Assert.True(PollingAt(AS4ReceiptsPath));
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            Holodeck.AssertDeliverMessageOnHolodeckB();
            AS4Component.AssertReceipt();
        }
    }
}
