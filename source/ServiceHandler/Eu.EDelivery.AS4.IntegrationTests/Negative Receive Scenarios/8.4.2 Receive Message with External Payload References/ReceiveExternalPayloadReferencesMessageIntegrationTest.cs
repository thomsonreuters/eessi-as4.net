using System;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Receive_Scenarios._8._4._2_Receive_Message_with_External_Payload_References
{
    /// <summary>
    /// Testing the Application with External Payload References
    /// </summary>
    public class ReceiveExternalPayloadReferencesMessageIntegrationTest : IntegrationTestTemplate
    {
        private const string ContentType = "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        [Fact]
        public async void ThenSendingMessageFailsAsync()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);

            // Act
            string messageMissingMimeProperty = Properties.Resources.as4message_external_payloads;
            AS4Message as4Message = await new StubSender().SendMessage(messageMissingMimeProperty, ContentType);

            // Assert
            AssertErrorMessage(as4Message);

            // After
            Console.WriteLine(@"Receive Compressed Message with External Payload References Integration Test succeeded!");
            StopApplication();
        }

        private static void AssertErrorMessage(AS4Message as4Message)
        {
            var error = as4Message.PrimarySignalMessage as Error;
            Assert.NotNull(error);

            Assert.NotEmpty(error.Errors);
            AssertErrorCode(error);
        }

        private static void AssertErrorCode(Error error)
        {
            string errorCode = error.Errors.FirstOrDefault().ErrorCode;
            Assert.Equal($"EBMS:{(int)ErrorCode.Ebms0011:0000}", errorCode);
        }
    }
}
