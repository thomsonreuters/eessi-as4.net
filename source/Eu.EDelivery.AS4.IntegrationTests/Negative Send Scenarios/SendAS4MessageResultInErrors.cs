using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios
{
    public class SendAS4MessageResultInErrors : IntegrationTestTemplate
    {
        [Fact]
        public async Task Test_8_2_8_Send_AS4Message_To_Receiving_MSH_That_Cant_Find_PMode_Result_In_Notification_Error()
        {
            // Arrange
            AS4Component.Start();

            // Act
            AS4Component.PutSubmitMessage("8.2.8-pmode", AS4Component.SubmitPayloadImage);

            // Assert
            IEnumerable<FileInfo> errors = 
                await PollingService.PollUntilPresentAsync(AS4Component.ErrorsPath);

            string xml = File.ReadAllText(errors.First().FullName);
            var notification = AS4XmlSerializer.FromString<NotifyMessage>(xml);
            Assert.True(notification != null, "Found Error notification cannot be deserialized to a 'NotifyMessage'");
            Assert.True(notification.StatusInfo != null, "Found Error notification doesn't hava a <StatusInfo/> element");

            Assert.Equal(Status.Error, notification.StatusInfo.Status);
            Assert.True(
                notification.StatusInfo.Any.First().SelectSingleNode("//*[local-name()='ErrorDetail']")?.InnerText != null, 
                "Found Error notification doesn't have a <SignalMessage/> included");
        }
    }
}
