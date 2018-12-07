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
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios
{
    public class ReceiveAS4MessageResultInDeliverable : IntegrationTestTemplate
    {
        [Fact]
        public async Task Test_8_3_14_AS4Message_Test_Message_Gets_Responded_But_Not_Delivered()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckA("8.3.14-pmode.xml");

            // Act
            Holodeck.PutMessageSinglePayloadToHolodeckA("8.3.14-pmode");

            // Assert
            await PollingService.PollUntilPresentAsync(Holodeck.HolodeckALocations.InputPath);

            Assert.Empty(Directory.GetFiles(AS4Component.FullInputPath, "*.xml"));
        }

        [Fact]
        public async Task Test_8_3_15_AS4Message_Duplicate_Doesnt_Get_Delivered()
        {
            // Arrange
            AS4Component.Start();
            const string contentType = "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

            await HttpClient.SendMessageAsync(duplicated_as4message, contentType);
            await PollingService.PollUntilPresentAsync(AS4Component.FullInputPath);
            CleanUpFiles(AS4Component.FullInputPath);

            // Act
            await HttpClient.SendMessageAsync(duplicated_as4message, contentType);

            // Assert
            Assert.Empty(Directory.GetFiles(AS4Component.FullInputPath, "*.xml"));
        }

        [Fact]
        public async Task Test_8_3_16_Pulled_AS4Message_Gets_Delivered()
        {
            // Arrange
            AS4Component.OverrideSettings("8.3.16-settings.xml");
            AS4Component.Start();

            Holodeck.CopyPModeToHolodeckB("8.3.16-pmode.xml");

            // Act
            Holodeck.PutMessageSinglePayloadToHolodeckB("ex-pm-pull-ut");

            // Assert
            await PollingService.PollUntilPresentAsync(
                AS4Component.FullInputPath,
                fs => fs.Count() == 2,
                timeout: TimeSpan.FromSeconds(50));

            var deliverDir = new DirectoryInfo(AS4Component.FullInputPath);
            FileInfo[] deliverables = deliverDir.GetFiles();

            Assert.NotNull(deliverables.FirstOrDefault(f => f.Extension == ".xml"));
            Assert.NotNull(deliverables.FirstOrDefault(f => f.Extension == ".jpg"
                                                            && f.Length == Holodeck.HolodeckAJpegPayload.Length));
        }

        private static readonly HttpClient PayloadServiceClient = new HttpClient();

        [Fact]
        public async Task Test_8_3_17_AS4Message_Gets_Delivered_To_Payload_Service()
        {
            // Arrange
            AS4Component.OverrideSettings("8.3.17-settings.xml");

            Holodeck.CopyPModeToHolodeckA("8.3.17-pmode.xml");
            Holodeck.PutMessageSinglePayloadToHolodeckA("8.3.16-pmode");

            // Act
            AS4Component.Start();

            // Assert
            IEnumerable<FileInfo> deliverables =
                await PollingService.PollUntilPresentAsync(
                    AS4Component.FullInputPath,
                    fs => fs.Any(f => f.Extension == ".xml"));

            var doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(deliverables.First().FullName));
            XmlElement locationElement = doc["DeliverMessage"]?["Payloads"]?["Payload"]?["Location"];
            Assert.True(locationElement != null, "No payload location found in delivered DeliverMessage");

            HttpResponseMessage downloadResponse = await PayloadServiceClient.GetAsync(locationElement.InnerText);
            Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
        }

        [Fact]
        public void Test_8_3_18_AS4Message_Gets_Delivered_To_Http_Target()
        {
            // Before
            const string location = "http://localhost:4001/";
            using (SpyHttpDeliverTarget deliverTarget = SpyHttpDeliverTarget.AtLocation(location))
            {
                // Arrange
                Holodeck.CopyPModeToHolodeckA("8.3.18-pmode.xml");
                Holodeck.PutMessageSinglePayloadToHolodeckA("8.3.17-pmode");

                // Act
                AS4Component.Start();

                // Assert
                Assert.True(deliverTarget.IsCalled, "Receive Single Payload Deliver HTTP Integration Test failed");
                string deliverContent = deliverTarget.DeliveredMessage;
                Assert.False(String.IsNullOrEmpty(deliverContent), "Delivered Message was empty");

                var doc = new XmlDocument();
                doc.LoadXml(deliverContent);
                XmlNode deliverMessageNode = doc.SelectSingleNode("//*[local-name()='DeliverMessage']");
                Assert.NotNull(deliverMessageNode);
            }
        }
    }
}
