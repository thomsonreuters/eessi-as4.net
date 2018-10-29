using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._9_Submit_invalid_serialized_message_result_in_Bad_Request
{
    /// <summary>
    /// Testing the 'HTTP Receiver' on the 'Subimt Agent' with invalid serialized messages.
    /// </summary>
    public class SubmitInvalidSerializedMessageTest : IntegrationTestTemplate
    {
        [Fact]
        public async Task SubmitResultInBadRequest_IfInvalidSerializedMessageIsSend()
        {
            // Arrange
            AS4Component.OverrideSettings("8.2.9-settings.xml");
            AS4Component.Start();

            // Act
            HttpWebResponse response = await HttpClient.SendPdfAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
