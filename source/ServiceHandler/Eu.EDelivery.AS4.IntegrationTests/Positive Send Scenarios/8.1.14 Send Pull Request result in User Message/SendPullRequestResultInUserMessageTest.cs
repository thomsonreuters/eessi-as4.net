using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._14_Send_Pull_Request_result_in_User_Message
{
    /// <summary>
    /// Testing if the AS4 Component correctly sends a Pull Request to another party.
    /// </summary>
    public class SendPullRequestResultInUserMessageTest : IntegrationTestTemplate
    {
        private readonly Holodeck _holodeck;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendPullRequestResultInUserMessageTest"/> class.
        /// </summary>
        public SendPullRequestResultInUserMessageTest()
        {
            _holodeck = new Holodeck();
        }

        [Fact]
        public void ThenSendingPullRequestSucceeds()
        {
            // Setup
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(AS4FullInputPath);

            // Act
            base.StartApplication();

            // Assert
            bool areFilesFound = base.PollTo(AS4FullInputPath);
            Assert.True(areFilesFound);
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            _holodeck.AssertImagePayload();   
        }
    }
}
