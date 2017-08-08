using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._2_Submit_message_nonexisting_PMode_FILE
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SubmitMessageContainsNonExistingPModeIntegrationTest : IntegrationTestTemplate
    {        
        [Fact]
        public void ThenSendingSubmitMessageFails()
        {
            // Before
            AS4Component.Start();

            // Act
            AS4Component.PutMessage("8.2.2-sample.xml");            

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