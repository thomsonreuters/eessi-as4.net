using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._18_Send_Message_via_Pulling_result_in_Error
{
    public class SendMessageViaPullingResultInErrorTest : IntegrationTestTemplate
    {
        [Fact(Skip = "Waiting for the composing of the Pull Agent (must update 'settings.xml')")]
        public void HolodeckGetsErrorFromPullRequest_IfMpcForPullRequestMatchesWrongCertificate()
        {
            // Arrange
            // Override 'settings.xml'...

            // Act
            CopyMessageToHolodeckB("8.1.18-pmode.xml");

            // Assert
            Assert.True(PollingAt(holodeck_B_input_path));
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            FileInfo error = files.First();
            string xml = File.ReadAllText(error.FullName);

            Assert.Contains("Warning", xml);
            Assert.Contains("EBMS:0006", xml);
            Assert.Contains("EmptyMessagePartitionChannel", xml);
        }
    }
}
