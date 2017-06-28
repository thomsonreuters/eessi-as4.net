using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._18_Send_Message_via_Pulling_result_in_Error
{
    public class SendMessageViaPullingResultInErrorTest : IntegrationTestTemplate
    {
        [Fact]
        public void HolodeckGetsErrorFromPullRequest_IfMpcForPullRequestMatchesWrongCertificate()
        {
            // Arrange
            AS4Component.OverrideSettings("8.1.18-settings.xml");
            AS4Component.Start();   

            // Act
            Holodeck.CopyPModeToHolodeckB("8.1.18-receive-pmode.xml");
            Holodeck.CopyPModeToHolodeckB("8.1.18-pmode.xml");

            // Assert
            Assert.True(PollingAt(HolodeckBInputPath));
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            FileInfo error = files.First();
            Assert.NotNull(error);

            // Holodeck locks file, so we can't read it?
            // ------------------------------------------------------------
            // string xml = File.ReadAllText(error.FullName);
            // Assert.Contains("Warning", xml);
            // Assert.Contains("EBMS:0006", xml);
            // Assert.Contains("EmptyMessagePartitionChannel", xml);
        }
    }
}
