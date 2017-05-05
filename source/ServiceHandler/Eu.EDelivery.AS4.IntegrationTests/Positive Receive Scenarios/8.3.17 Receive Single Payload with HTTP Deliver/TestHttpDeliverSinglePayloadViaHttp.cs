using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._17_Receive_Single_Payload_with_HTTP_Deliver
{
    public class TestHttpDeliverSinglePayloadViaHttp : IntegrationTestTemplate
    {
        [Fact]
        public void RunIntegrationTest()
        {
            // Before
            CleanupFiles();

            const string location = "http://localhost:4001/";
            using (SpyHttpDeliverTarget deliverTarget = SpyHttpDeliverTarget.AtLocation(location))
            {
                // Arrange
                CopyPModeToHolodeckA("8.3.17-pmode.xml");
                CopyMessageToHolodeckA("8.3.17-sample.mmd");

                // Act
                AS4Component.Start();

                // Assert
                Assert.True(deliverTarget.IsCalled, "Receive Single Payload Deliver HTTP Integration Test failed");
                AssertOnDeliverContent(deliverTarget.DeliveredMessage);
            }
        }

        private void CleanupFiles()
        {
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(Properties.Resources.holodeck_A_input_path);
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