using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._17_Send_Message_via_Pulling_without_MPC
{
    public class SendMessageViaPullingWithoutMpcTest : IntegrationTestTemplate
    {
        [Fact(Skip = "Waiting for the composing of the Pull Agent (must update 'settings.xml')")]
        public void HolodeckGetsReceiptForPullRequest_IfNoMpcIsNotDefinedInPullRequest()
        {
            // Arrange
            // Override 'settings.xml'...

            // Act
            CopyPModeToHolodeckB("8.1.17-pmode.xml");
            
            // Assert
            Assert.True(PollingAt(holodeck_B_input_path));
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            Holodeck.AssertReceiptOnHolodeckB(files);
        }
    }
}
