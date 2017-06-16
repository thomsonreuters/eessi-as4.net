using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

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

            File.Copy($"{AS4MessagesRootPath}\\8.1.16-sample.xml", $"{AS4FullOutputPath}\\8.1.16-sample.xml", overwrite: true);

            // Act
            CopyPModeToHolodeckB("8.1.16-receive-pmode.xml");
            CopyPModeToHolodeckB("8.1.16-pmode.xml");

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
