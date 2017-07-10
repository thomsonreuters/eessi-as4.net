using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;

using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._18_Receive_Single_Payload_with_HTTP_Deliver
{
    public class TestHttpDeliverSinglePayloadViaHttp : IntegrationTestTemplate
    {
        [Fact]
        public void RunIntegrationTest()
        {
            // Before
            const string location = "http://localhost:4001/";
            using (SpyHttpDeliverTarget deliverTarget = SpyHttpDeliverTarget.AtLocation(location))
            {
                // Arrange
                Holodeck.CopyPModeToHolodeckA("8.3.18-pmode.xml");
                Holodeck.CopyMessageToHolodeckA("8.3.18-sample.mmd");

                // Act
                AS4Component.Start();

                // Assert
                Assert.True(deliverTarget.IsCalled, "Receive Single Payload Deliver HTTP Integration Test failed");
                AssertOnDeliverContent(deliverTarget.DeliveredMessage);
            }
        }

        private static void AssertOnDeliverContent(string deliverContent)
        {
            Assert.False(string.IsNullOrEmpty(deliverContent), "Delivered Message was empty");

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(deliverContent);
            XmlNode deliverMessageNode = xmlDocument.SelectSingleNode("//*[local-name()='DeliverMessage']");

            Assert.NotNull(deliverMessageNode);
        }
    }
}