using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios
{
    public class SendAS4MessageResultInError : IntegrationTestTemplate
    {
        [Fact]
        public async Task Test_8_1_13_Send_AS4Message_Wrongly_Signed_Result_In_Async_Error()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckB("8.1.13-pmode.xml");

            string ebmsMessageId = Guid.NewGuid().ToString();
            AS4Component.PutSubmitMessage(
                "8.1.13-pmode",
                submit => submit.MessageInfo.MessageId = ebmsMessageId,
                AS4Component.SubmitPayloadImage);

            string messageWrongSigned = ReplaceSubmitMessageIdWith(ebmsMessageId);

            // Act
            await HttpClient.SendMessageAsync(
                Holodeck.HolodeckBHttpEndpoint, 
                messageWrongSigned, 
                Constants.ContentTypes.Soap);

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ErrorsPath);
        }

        private static string ReplaceSubmitMessageIdWith(string generatedMessageId)
        {
            // We need to generate and replace the message id with a new one because Holodeck only non-duplicates.

            string messageWrongSigned = as4_soap_wrong_signed_callback_message;
            messageWrongSigned = messageWrongSigned.Replace("2e0a5701-790a-4a53-a8b7-e7f528fc1b53@10.124.29.131", generatedMessageId);

            return messageWrongSigned;
        }
    }
}
