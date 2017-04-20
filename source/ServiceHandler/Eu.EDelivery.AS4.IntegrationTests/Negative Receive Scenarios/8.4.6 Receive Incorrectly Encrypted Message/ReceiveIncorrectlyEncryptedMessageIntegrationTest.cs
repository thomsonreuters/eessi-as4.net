using System;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Receive_Scenarios._8._4._6_Receive_Incorrectly_Encrypted_Message
{
    /// <summary>
    /// Testing the Application with a incorrectly encrypted message
    /// </summary>
    public class ReceiveIncorrectlyEncryptedMessageIntegrationTest : IntegrationTestTemplate
    {
        private readonly StubSender _sender;

        public ReceiveIncorrectlyEncryptedMessageIntegrationTest()
        {
            var soapSerializer = new SoapEnvelopeSerializer();
            var mimeSerializer = new MimeMessageSerializer(soapSerializer);
            this._sender = new StubSender(mimeSerializer);
        }

        [Fact]
        public async void ReceivingIncorrectlyEncryptedMessageFails()
        {
            // Before
            this.AS4Component.Start();
            base.CleanUpFiles(AS4FullInputPath);

            // Act
            var contentType = "multipart/related; boundary=\"=-WoWSZIFF06iwFV8PHCZ0dg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";
            string messageWrongEncrypted = Properties.Resources.as4_soap_wrong_encrypted_message;
            AS4Message as4Message = await this._sender.SendMessage(messageWrongEncrypted, contentType);

            // Assert
            AssertErrorMessage(as4Message);

            // After
            Console.WriteLine(@"Receive Compressed Message Incorrectly Encrypted Integration Test succeeded!");
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
            Assert.Equal($"EBMS:{(int)ErrorCode.Ebms0102:0000}", errorCode);
        }
    }
}
