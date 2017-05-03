using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._17_Receive_Single_Payload_with_HTTP_Deliver
{
    public class TestSubmitDeliverSinglePayloadViaHttp : IntegrationTestTemplate
    {
        [Fact]
        public void RunIntegrationTest()
        {
            // Before
            const string location = "http://location:5000";
            using (StubHttpDeliverTarget deliverTarget = StubHttpDeliverTarget.AtLocation(location))
            {
                // Arrange
                AS4Component.OverrideSettings("8.3.17-settings.xml");
                AS4Component.Start();
                var stubSender = new StubSender();

                // Act
                stubSender.SendMessage(Properties.Resources.submitmessage_8_3_17_xml, "application/xml");

                // Assert
                Assert.True(deliverTarget.IsCalled);
                Assert.False(string.IsNullOrEmpty(deliverTarget.DeliverMessageContent));
            }
        }
    }
}
