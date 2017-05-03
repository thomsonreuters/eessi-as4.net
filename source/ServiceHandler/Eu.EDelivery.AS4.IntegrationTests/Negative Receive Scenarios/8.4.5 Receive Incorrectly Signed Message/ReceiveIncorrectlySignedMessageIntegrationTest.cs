using System;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Receive_Scenarios._8._4._5_Receive_Incorrectly_Signed_Message
{
    /// <summary>
    /// Testing the Application with a incorrectly signed message
    /// </summary>
    public class ReceiveIncorrectlySignedMessageIntegrationTest : IntegrationTestTemplate
    {
        private readonly StubSender _sender;

        public ReceiveIncorrectlySignedMessageIntegrationTest()
        {
            _sender = new StubSender();
        }

        [Fact]
        public async void ThenReceivingIncorrectlySignedMessageFails()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);

            // Act
            string messageWrongSigned = Properties.Resources.as4_soap_wrong_signed_message;
            AS4Message as4Message = await _sender.SendMessage(messageWrongSigned, Constants.ContentTypes.Soap);

            // Assert
            AssertErrorMessage(as4Message);

            // After
            Console.WriteLine(@"Receive Compressed Message Incorrectly Signed Integration Test succeeded!");
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
            string errorCode = error.Errors.First().ErrorCode;
            Assert.Equal($"EBMS:{(int)ErrorCode.Ebms0101:0000}", errorCode);
        }
    }
}
