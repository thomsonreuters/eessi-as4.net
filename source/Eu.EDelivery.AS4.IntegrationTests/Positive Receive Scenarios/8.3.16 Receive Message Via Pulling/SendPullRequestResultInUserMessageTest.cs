using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._16_Receive_Message_Via_Pulling
{
    /// <summary>
    /// Testing if the AS4 Component correctly sends a Pull Request to another party.
    /// </summary>
    public class SendPullRequestResultInUserMessageTest : IntegrationTestTemplate
    {
        [Retry(MaxRetries = 2)]
        public void ThenSendingPullRequestSucceeds()
        {
            // Setup
            AS4Component.OverrideSettings("8.3.16-settings.xml");
            AS4Component.Start();

            Holodeck.CopyPModeToHolodeckB("8.3.16-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckB("8.3.16-sample.mmd");

            // Assert
            bool areFilesFound = PollingAt(AS4FullInputPath);
            Assert.True(areFilesFound, "Pull Request > User Message failed");
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayload(files.FirstOrDefault(f => f.Extension.Equals(".jpg")));
            AssertReceipt(files.FirstOrDefault(f => f.Extension.Equals(".xml")));
        }

        private static void AssertPayload(FileInfo receivedPayload)
        {
            FileInfo sendPayload = AS4Component.SubmitSinglePayloadImage;

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        private static void AssertReceipt(FileInfo receipt)
        {
            Assert.NotNull(receipt);
        }
    }
}