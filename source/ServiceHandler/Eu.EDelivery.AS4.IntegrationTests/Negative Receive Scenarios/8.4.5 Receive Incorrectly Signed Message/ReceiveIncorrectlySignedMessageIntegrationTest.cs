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
        [Fact]
        public async void ThenReceivingIncorrectlySignedMessageFails()
        {
            // Before
            AS4Component.Start();

            // Act
            string messageWrongSigned = Properties.Resources.as4_soap_wrong_signed_message;
            AS4Message as4Message = await new StubSender().SendMessage(messageWrongSigned, Constants.ContentTypes.Soap);

            // Assert
            AssertErrorMessage(as4Message);

            // After
            Console.WriteLine(@"Receive Compressed Message Incorrectly Signed Integration Test succeeded!");
            StopApplication();
        }

        private static void AssertErrorMessage(AS4Message as4Message)
        {
            var error = as4Message.PrimaryMessageUnit as Error;
            Assert.NotNull(error);

            Assert.NotEmpty(error.ErrorLines);
            AssertErrorCode(error);
        }

        private static void AssertErrorCode(Error error)
        {
            ErrorCode errorCode = error.ErrorLines.First().ErrorCode;
            Assert.Equal(ErrorCode.Ebms0101, errorCode);
        }
    }
}
