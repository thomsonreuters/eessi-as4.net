using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios
{
    // TODO: move to Component Tests : SubmitAgentFacts
    public class SendSubmitMessageResultInExceptions : IntegrationTestTemplate
    {
        public SendSubmitMessageResultInExceptions()
        {
            AS4Component.Start();
        }

        [Fact]
        public async Task Test_8_2_1_SubmitMessage_That_Tries_To_Override_SendingPMode_Values_Result_In_Submit_Exceptions_On_Disk()
        {
            // Act
            AS4Component.PutSubmitMessage("8.2.1-pmode", AS4Component.SubmitPayloadImage);

            // Assert
            await PollingService.PollUntilPresentAsync(
                AS4Component.FullOutputPath,
                fs => fs.Any(f => f.Name.EndsWith(".exception.details"))
                      && fs.Any(f => f.Name.EndsWith(".exception.details")));
        }

        [Fact]
        public async Task Test_8_2_2_SubmitMessage_With_NonExisting_SendingPMode_Reference_Result_In_Submit_Exceptions_On_Disk()
        {
            // Act
            AS4Component.PutSubmitMessage("8.2.2-pmode", AS4Component.SubmitPayloadImage);

            // Assert
            await AssertOnSubmitExceptionsAsync();
        }

        [Fact]
        public async Task Test_8_2_3_SubmitMessage_With_SendingPMode_Id_Of_Invalid_PMode_Result_In_Submit_Exceptions_On_Disk()
        {
            // Act
            AS4Component.PutSubmitMessage("8.2.3-pmode", AS4Component.SubmitPayloadImage);

            // Assert
            await AssertOnSubmitExceptionsAsync();
        }

        [Fact]
        public async Task Test_8_2_4_SubmitMessage_With_Invalid_File_Payload_Reference_Result_In_Submit_Exceptions_On_Disk()
        {
            // Act            
            AS4Component.PutSubmitMessage("8.2.4-pmode", AS4Component.SubmitPayloadImage);

            // Assert
            await AssertOnSubmitExceptionsAsync();
        }

        private static async Task AssertOnSubmitExceptionsAsync()
        {
            await PollingService.PollUntilPresentAsync(
                AS4Component.FullOutputPath,
                fs => fs.Any(f => f.Name.EndsWith(".exception.details"))
                      && fs.Any(f => f.Name.EndsWith(".exception.details")));
        }

        [Fact]
        public async Task Test_8_2_5_SubmitMessage_With_SendingPMode_Referencing_Non_Exsising_Signing_Certificate_Result_In_Send_Exception_On_Disk()
        {
            // Act
            AS4Component.PutSubmitMessage("8.2.5-pmode", AS4Component.SubmitPayloadImage);

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ExceptionsPath);
        }

        [Fact]
        public async Task Test_8_2_6_SubmitMessage_With_SendingPMode_Referencing_Non_Existing_Encryption_Certificate_Result_In_Send_Exception_On_Disk()
        {
            // Act
            AS4Component.PutSubmitMessage("8.2.6-pmode", AS4Component.SubmitPayloadImage);

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ExceptionsPath);
        }

        [Fact]
        public async Task Test_8_2_7_SubmitMessage_With_SendingPMode_Specifying_Non_Existing_Url_Result_In_Send_Exception_On_Disk()
        {
            // Act
            AS4Component.PutSubmitMessage("8.2.7-pmode", AS4Component.SubmitPayloadImage);

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ExceptionsPath);
        }
    }
}
