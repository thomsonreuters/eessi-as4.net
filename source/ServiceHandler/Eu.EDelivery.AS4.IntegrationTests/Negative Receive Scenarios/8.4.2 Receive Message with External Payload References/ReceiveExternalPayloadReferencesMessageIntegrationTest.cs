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

        private readonly StubSender _sender;

        public ReceiveExternalPayloadReferencesMessageIntegrationTest()
        {
            this._sender = new StubSender();
        }

        [Fact]
        public async void ThenSendingMessageFailsAsync()
        {
            // Before
            base.StartApplication();
            base.CleanUpFiles(AS4FullInputPath);

            // Act
            string messageMissingMimeProperty = Properties.Resources.as4message_external_payloads;
            AS4Message as4Message = await this._sender
                .SendAsync(messageMissingMimeProperty, ContentType);

            // Assert
            AssertErrorMessage(as4Message);

            // After
            Console.WriteLine(@"Receive Compressed Message with External Payload References Integration Test succeeded!");
            base.StopApplication();
        }

        private void AssertErrorMessage(AS4Message as4Message)
        {
            var error = as4Message.PrimarySignalMessage as Error;
            Assert.NotNull(error);

            Assert.NotEmpty(error.Errors);
            AssertErrorCode(error);
        }

        private void AssertErrorCode(Error error)
        {
            string errorCode = error.Errors.FirstOrDefault().ErrorCode;
            Assert.Equal($"EBMS:{(int)ErrorCode.Ebms0011:0000}", errorCode);
        }
    }
}
