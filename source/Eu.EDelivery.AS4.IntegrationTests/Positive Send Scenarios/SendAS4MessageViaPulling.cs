using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios
{
    public class SendAS4MessageViaPulling : IntegrationTestTemplate
    {
        [Fact]
        public async Task Test_8_1_16_Respond_With_UserMessage_When_Receive_PullRequest_Result_In_Notified_Receipt()
        {
            // Arrange
            AS4Component.OverrideSettings("8.1.16-settings.xml");
            AS4Component.Start();

            AS4Component.PutSubmitMessage(
                "8.1.16-pmode",
                submit => submit.MessageInfo = new MessageInfo { Mpc = "http://example.holodeckb2b.org/mpc/1" },
                AS4Component.SubmitPayloadImage);

            // Act
            Holodeck.CopyPModeToHolodeckB("8.1.16-receive-pmode.xml");
            Holodeck.CopyPModeToHolodeckB("8.1.16-pmode.xml");

            // Assert
            await PollingService.PollUntilPresentAsync(
                AS4Component.ReceiptsPath,
                fs => fs.Any(f => f.Extension == ".xml"),
                timeout: TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(30)));

            Holodeck.AssertDeliverMessageOnHolodeckB();
            AS4Component.AssertReceipt();
        }

        [Fact]
        public async Task Test_8_1_18_Respond_With_Error_When_Receive_PullRequest_Is_Not_Authorized()
        {
            // Arrange
            AS4Component.OverrideSettings("8.1.18-settings.xml");
            AS4Component.Start();

            // Act
            Holodeck.CopyPModeToHolodeckB("8.1.18-receive-pmode.xml");
            Holodeck.CopyPModeToHolodeckB("8.1.18-pmode.xml");

            // Assert
            await PollingService.PollUntilPresentAsync(Holodeck.HolodeckBLocations.InputPath);
        }
    }
}
