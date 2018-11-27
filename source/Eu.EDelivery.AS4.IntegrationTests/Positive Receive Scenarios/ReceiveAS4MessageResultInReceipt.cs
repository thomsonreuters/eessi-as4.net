using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._18_Receive_Single_Payload_with_HTTP_Deliver;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios
{
    public class ReceiveAS4MessageResultInReceipt : IntegrationTestTemplate
    {
        [Fact]
        public async Task Test_8_3_1_AS4Message_With_Single_Payload_Result_In_File_Deliver()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckA("8.3.1-pmode.xml");

            // Act
            Holodeck.PutMessageSinglePayloadToHolodeckA("8.3.1-pmode");

            // Assert
            await PollingService.PollUntilPresentAsync(
                AS4Component.FullInputPath,
                fs => fs.Any(f => f.Extension == ".xml"));

            Holodeck.AssertSinglePayloadOnHolodeckA();
        }

        [Fact]
        public async Task Test_8_3_2_AS4Message_With_Multiple_Payloads_Result_In_File_Deliver()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.3.2-pmode");
        }

        [Fact]
        public async Task Test_8_3_3_AS4Message_With_Multiple_Compressed_Payloads_Result_In_File_Deliver()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.3.3-pmode");
        }

        [Fact]
        public async Task Test_8_3_4_AS4Message_With_Multiple_Payloads_Signed_MessagingHeader_Result_In_File_Deliver()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.3.4-pmode");
        }

        [Fact]
        public async Task Test_8_3_5_AS4Message_With_Multiple_Encrypted_Payloads_Result_In_File_Deliver()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.3.5-pmode");
        }

        [Fact]
        public async Task Test_8_3_6_AS4Message_With_Multiple_Compressed_Payloads_Signed_MessagingHeader_Result_In_File_Deliver()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.3.6-pmode");
        }

        [Fact]
        public async Task Test_8_3_7_AS4Message_With_Multiple_Compressed_Encrypted_Payloads_Result_In_File_Deliver()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.3.7-pmode");
        }

        [Fact]
        public async Task Test_8_3_8_AS4Message_With_Multiple_Compressed_Encrypted_Payloads_Signed_MessagingHeader_Result_In_File_Deliver()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.3.8-pmode");
        }

        [Fact]
        public async Task Test_8_3_9_AS4Message_With_Multiple_Encrypted_Payloads_Signed_MessagingHeader_Result_In_File_Deliver()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.3.9-pmode");
        }

        private async Task TestAS4MessageWithMultiplePayloadsUsingPModeAsync(string pmodeId)
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckA($"{pmodeId}.xml");

            // Act
            Holodeck.PutMessageTwoPayloadsToHolodeckA(pmodeId);

            // Assert
            await PollingService.PollUntilPresentAsync(
                AS4Component.FullInputPath,
                fs => fs.Count() == 3
                      && fs.Count(f => f.Extension == ".jpg") == 1
                      && fs.Count(f => f.Extension == ".xml") == 2);

            Holodeck.AssertSinglePayloadOnHolodeckA();
        }

        [Fact]
        public async Task Test_8_3_10_AS4Message_Gets_Responded_With_Sync_Receipt()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckA("8.3.10-pmode.xml");

            // Act
            Holodeck.PutMessageSinglePayloadToHolodeckA("8.3.10-pmode");

            // Assert
            await PollingService.PollUntilPresentAsync(Holodeck.HolodeckALocations.InputPath);
        }

        [Fact]
        public async Task Test_8_3_11_AS4Message_Gets_Responded_With_Non_Repudiation_Receipt()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckA("8.3.11-pmode.xml");

            // Act
            Holodeck.PutMessageSinglePayloadToHolodeckA("8.3.11-pmode");

            // Assert
            await PollingService.PollUntilPresentAsync(Holodeck.HolodeckALocations.InputPath);
        }

        [Fact]
        public async Task Test_8_3_12_AS4Message_With_Single_Payload_Gets_Responded_With_Async_Receipt()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckA("8.3.12-pmode.xml");

            // Act
            Holodeck.PutMessageSinglePayloadToHolodeckA("8.3.12-pmode");

            // Assert
            await PollingService.PollUntilPresentAsync(
                Holodeck.HolodeckALocations.InputPath,
                fs => fs.Any(f => f.Extension == ".xml"),
                timeout: TimeSpan.FromSeconds(50));
        }

        [Fact]
        public async Task Test_8_3_13_AS4Message_With_Single_Payload_Gets_Responded_With_Signed_NonRepudiation_Receipt()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckA("8.3.13-pmode.xml");

            // Act
            Holodeck.PutMessageSinglePayloadToHolodeckA("8.3.13-pmode");

            // Assert
            await PollingService.PollUntilPresentAsync(
                Holodeck.HolodeckALocations.InputPath,
                fs => fs.Any(f => f.Extension == ".xml"),
                timeout: TimeSpan.FromSeconds(50));
        }
    }
}
