using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._4_Submit_message_invalid_payload_reference_FILE
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SubmitMessageReferenceNonExistingPayloadIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.2.4-sample.xml";
        private readonly string _as4MessagesPath = $"{AS4MessagesRootPath}{SubmitMessageFilename}";
        private readonly string _as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        
       [Fact]
        public void ThenSendingSubmitMessageFails()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullOutputPath);

            // Act
            File.Copy(_as4MessagesPath, _as4OutputPath);

            // Assert
            Assert.True(AreExceptionFilesFound());
        }

        private bool AreExceptionFilesFound()
        {
            return
                PollingAt(AS4FullOutputPath, "*.exception") &&
                PollingAt(AS4FullOutputPath, "*.exception.details");
        }
    }
}