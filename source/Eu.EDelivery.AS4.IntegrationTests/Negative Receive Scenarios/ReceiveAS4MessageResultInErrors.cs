using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Receive_Scenarios
{
    public class ReceiveAS4MessageResultInErrors : IntegrationTestTemplate
    {
        public ReceiveAS4MessageResultInErrors()
        {
            AS4Component.Start();
        }

        [Fact]
        public async Task Test_8_4_1_Received_AS4Message_That_Cant_Be_Linked_To_ReceivingPMode_Result_In_Processing_Error()
        {
            // Arrange
            Holodeck.CopyPModeToHolodeckA("8.4.1-pmode.xml");

            // Act
            Holodeck.CopyMessageToHolodeckA("8.4.1-sample.mmd");

            // Assert
            await PollingService.PollUntilPresentAsync(
                Holodeck.HolodeckALocations.InputPath,
                fs => fs.Any(f => f.Extension == ".xml"));

            Holodeck.AssertErrorOnHolodeckA(ErrorCode.Ebms0001);
        }

        [Fact]
        public async Task Test_8_4_2_Received_AS4Message_With_External_Payload_References_Result_In_Processing_Error()
        {
            // Arrange
            const string contentType = "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

            // Act
            AS4Message response = await HttpClient.SendMessageAsync(as4message_external_payloads, contentType);

            // Assert
            var error = Assert.IsType<Error>(response.PrimaryMessageUnit);
            Assert.NotEmpty(error.ErrorLines);
            Assert.Equal(ErrorCode.Ebms0011, error.ErrorLines.First().ErrorCode);
        }

        [Fact]
        public async Task Test_8_4_3_Received_AS4Message_With_Compressed_Attachments_Without_MimeType_Result_In_Decompression_Error()
        {
            // Arrange
            const string contentType = "multipart/related; boundary=\"MIMEBoundary_58227ff3e3fc7f2a7373840dd22c75172d4362e9ce55d295\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

            // Act
            AS4Message response = await HttpClient.SendMessageAsync(as4message_missing_mime_property, contentType);

            // Assert
            var error = Assert.IsType<Error>(response.PrimaryMessageUnit);
            Assert.NotEmpty(error.ErrorLines);
            Assert.Equal(ErrorCode.Ebms0303, error.ErrorLines.First().ErrorCode);
        }

        [Fact]
        public async Task Test_8_4_4_Received_AS4Message_With_Incorrectly_Compressed_Attachments_Result_In_Decompression_Error()
        {
            // Arrange
            const string contentType = "multipart/related; boundary=\"MIMEBoundary_58227ff3e3fc7f2a7373840dd22c75172d4362e9ce55d295\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

            // Act
            AS4Message response = await HttpClient.SendMessageAsync(as4message_incorect_compressed, contentType);

            // Assert
            var error = Assert.IsType<Error>(response.PrimaryMessageUnit);
            Assert.NotEmpty(error.ErrorLines);
            Assert.Equal(ErrorCode.Ebms0303, error.ErrorLines.First().ErrorCode);
        }

        [Fact]
        public async Task Test_8_4_5_Received_AS4Message_With_Incorrectly_Signed_MessagingHeader_Result_In_Signature_Verification_Error()
        {
            // Act
            AS4Message response = await HttpClient.SendMessageAsync(as4_soap_wrong_signed_message, Constants.ContentTypes.Soap);

            // Assert
            var error = Assert.IsType<Error>(response.PrimaryMessageUnit);
            Assert.NotEmpty(error.ErrorLines);
            Assert.Equal(ErrorCode.Ebms0101, error.ErrorLines.First().ErrorCode);
        }

        [Fact]
        public async Task Test_8_4_6_Received_AS4Message_With_Incorrectly_Encrypted_Attachments_Result_In_Decryption_Error()
        {
            // Arrange
            const string contentType = "multipart/related; boundary=\"=-WoWSZIFF06iwFV8PHCZ0dg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

            // Act
            AS4Message response = await HttpClient.SendMessageAsync(as4_soap_wrong_encrypted_message, contentType);

            // Assert
            var error = Assert.IsType<Error>(response.PrimaryMessageUnit);
            Assert.NotEmpty(error.ErrorLines);
            Assert.Equal(ErrorCode.Ebms0102, error.ErrorLines.First().ErrorCode);
        }
    }
}