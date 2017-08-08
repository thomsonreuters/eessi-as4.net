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
       [Fact]
        public void ThenSendingSubmitMessageFails()
        {
            // Before
            AS4Component.Start();

            // Act            
            AS4Component.PutMessage("8.2.4-sample.xml");

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