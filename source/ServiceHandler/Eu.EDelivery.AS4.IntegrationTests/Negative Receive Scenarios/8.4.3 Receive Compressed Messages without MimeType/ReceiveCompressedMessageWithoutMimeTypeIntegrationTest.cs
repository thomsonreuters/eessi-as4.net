using System;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Receive_Scenarios._8._4._3_Receive_Compressed_Messages_without_MimeType
{
    /// <summary>
    /// Testing the Application with a Test Message
    /// </summary>
    public class ReceiveCompressedMessageWithoutMimeTypeIntegrationTest : IntegrationTestTemplate
    {
        private const string ContentType = "multipart/related; boundary=\"MIMEBoundary_58227ff3e3fc7f2a7373840dd22c75172d4362e9ce55d295\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        [Fact]
        public async void ThenSendingSinglePayloadSucceedsAsync()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);

            // Act
            string messageMissingMimeProperty = Properties.Resources.as4message_missing_mime_property;
            AS4Message as4Message = await new StubSender().SendMessage(messageMissingMimeProperty, ContentType);

            // Assert
            AssertErrorMessage(as4Message);

            // After
            Console.WriteLine(@"Receive Compressed Message without Mime Type Property Integration Test succeeded!");
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
            Assert.Equal($"EBMS:{(int)ErrorCode.Ebms0303:0000}", errorCode);
        }
    }
}
