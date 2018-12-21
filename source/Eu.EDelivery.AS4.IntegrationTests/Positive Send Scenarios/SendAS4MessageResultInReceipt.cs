using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;
using Xunit;
using static Eu.EDelivery.AS4.IntegrationTests.Properties.Resources;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios
{
    public class SendAS4MessageResultInReceipt : IntegrationTestTemplate
    {
        [Fact]
        public async Task Test_8_1_1_AS4Message_With_Single_Payload_Result_In_Notified_Receipt()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckB("8.1.1-pmode.xml");

            // Act
            AS4Component.PutSubmitMessageSinglePayload("8.1.1-pmode");

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ReceiptsPath);

            Holodeck.AssertSinglePayloadOnHolodeckB();
            AS4Component.AssertReceipt();
        }

        [Fact]
        public async Task Test_8_1_2_AS4Message_With_Multiple_Payloads_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.1.2-pmode");
        }

        [Fact]
        public async Task Test_8_1_3_AS4Message_With_Multiple_Compressed_Payloads_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.1.3-pmode");
        }

        [Fact]
        public async Task Test_8_1_4_AS4Message_With_Multiple_Signed_Payloads_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.1.4-pmode");
        }

        [Fact]
        public async Task Test_8_1_5_AS4Message_With_Multiple_Encrypted_Payloads_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.1.5-pmode");
        }

        [Fact]
        public async Task Test_8_1_6_AS4Message_With_Multiple_Payloads_Compressed_Signed_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.1.6-pmode");
        }

        [Fact]
        public async Task Test_8_1_7_AS4Message_With_Multiple_Payloads_Compressed_Encrypted_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.1.7-pmode");
        }

        [Fact]
        public async Task Test_8_1_8_AS4Message_With_Multiple_Payloads_Compressed_Signed_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.1.8-pmode");
        }

        [Fact]
        public async Task Test_8_1_9_AS4Message_With_Multiple_Payloads_Signed_Encrypted_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("8.1.9-pmode");
        }

        [Fact]
        public async Task AS4Message_With_Multiple_Payloads_KeyIdentifier_Signed_Encrypted_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("integration-encrypted-keyidentifer-signed");
        }

        [Fact]
        public async Task AS4Message_With_Multiple_Payloads_IssuerSerial_Signed_Encrypted_Result_In_Notified_Receipt()
        {
            await TestAS4MessageWithMultiplePayloadsUsingPModeAsync("integration-encrypted-issuerserial-signed");
        }

        private async Task TestAS4MessageWithMultiplePayloadsUsingPModeAsync(string pmodeId)
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckB($"{pmodeId}.xml");

            // Act
            AS4Component.PutSubmitMessageMultiplePayloads(pmodeId);

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ReceiptsPath);

            AS4Component.AssertMultiplePayloadsOnHolodeckB();
            AS4Component.AssertReceipt();
        }

        [Fact]
        public async Task Test_8_1_10_AS4Message_With_Single_Payload_And_MessageProperties_Result_In_Delivered_UserMessage_With_Same_MessageProperties()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckB("8.1.10-pmode.xml");

            // Act
            AS4Component.PutSubmitMessage(
                "8.1.10-pmode",
                submit => submit.MessageProperties = new []
                {
                    new MessageProperty("Important", "Yes"),
                    new MessageProperty("OriginalSender", "AS4.NET") 
                },
                AS4Component.SubmitPayloadImage);

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ReceiptsPath);

            Holodeck.AssertSinglePayloadOnHolodeckB();
            AS4Component.AssertReceipt();

            FileInfo userMessageFile =
                new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath)
                    .GetFiles("*.xml")
                    .FirstOrDefault();
            Assert.True(userMessageFile != null, "No UserMessage found at Holodeck B");

            var userMessageXml = new XmlDocument();
            userMessageXml.Load(userMessageFile.FullName);
            XmlNode messagePropertyNode = userMessageXml.SelectSingleNode("//*[local-name()='MessageProperties']");

            Assert.True(messagePropertyNode != null, "UserMessage at Holodeck B doesn't contain <MessageProperties/> element");
            Assert.True(
                messagePropertyNode.ChildNodes?.Count == 2,
                $"UserMessage at Holodeck B doesn't contain 2 MessageProperties but {messagePropertyNode.ChildNodes?.Count}");
        }

        [Fact]
        public async Task Test_8_1_11_AS4Message_With_Single_Payload_And_MessageIds_Result_In_Notified_Receipt_With_Same_MessageIds()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckB("8.1.11-pmode.xml");

            // Act
            AS4Component.PutSubmitMessage(
                "8.1.11-pmode",
                submit =>
                {
                    submit.MessageInfo.MessageId = Guid.NewGuid().ToString();
                    submit.MessageInfo.RefToMessageId = Guid.NewGuid().ToString();
                },
                AS4Component.SubmitPayloadImage);

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ReceiptsPath);

            IEnumerable<FileInfo> deliveredFiles = new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath).GetFiles();
            AS4Component.AssertEarthPayload(deliveredFiles.FirstOrDefault(f => f.Extension == ".jpg"));

            XmlDocument userDoc = LoadUserMessageDocFrom(deliveredFiles);
            XmlDocument receiptDoc = LoadReceiptDocFrom(AS4ReceiptsPath);

            XmlNode refToMessageIdNode = receiptDoc.SelectSingleNode("//*[local-name()='RefToMessageId']");
            Assert.True(refToMessageIdNode != null, "Cannot find 'RefToMessageId' node in Receipt at AS4.NET component");

            XmlNode messageIdNode = userDoc.SelectSingleNode("//*[local-name()='MessageId']");
            Assert.True(messageIdNode != null, "Cannot find 'MessageId' node in UserMessage at Holodeck B");

            Assert.Equal(messageIdNode.InnerText, refToMessageIdNode.InnerText);
        }

        private static XmlDocument LoadUserMessageDocFrom(IEnumerable<FileInfo> holodeckInFiles)
        {
            FileInfo userMessage = holodeckInFiles.FirstOrDefault(f => f.Extension.Equals(".xml"));
            Assert.NotNull(userMessage);

            var userDoc = new XmlDocument();
            userDoc.Load(userMessage.FullName);

            void AssertXmlTag(string localName, XmlNode xmlDocument)
            {
                XmlNode xmlNode = xmlDocument.SelectSingleNode($"//*[local-name()='{localName}']");

                Assert.True(xmlNode != null, $"Cannot find XML tag {localName} in Holodeck UserMessage");
                Console.WriteLine($@"{localName} found in Receipt");
            }

            AssertXmlTag("MessageId", userDoc);
            AssertXmlTag("ConversationId", userDoc);
            return userDoc;
        }

        private static XmlDocument LoadReceiptDocFrom(string inPath)
        {
            FileInfo receipt = new DirectoryInfo(inPath).GetFiles("*.xml").FirstOrDefault();
            Assert.NotNull(receipt);

            var receiptDoc = new XmlDocument();
            receiptDoc.Load(receipt.FullName);
            return receiptDoc;
        }

        [Fact]
        public async Task Test_8_1_12_AS4Message_With_Single_Payload_Result_In_Notified_Signed_Non_Repudiation_Receipt()
        {
            // Arrange
            AS4Component.Start();
            Holodeck.CopyPModeToHolodeckB("8.1.12-pmode.xml");

            // Act
            AS4Component.PutSubmitMessageSinglePayload("8.1.12-pmode");

            IEnumerable<FileInfo> receipts = 
                await PollingService.PollUntilPresentAsync(AS4Component.ReceiptsPath);

            string xml = File.ReadAllText(receipts.First().FullName);
            var notification = AS4XmlSerializer.FromString<NotifyMessage>(xml);

            Assert.True(notification != null, "Found Receipt notification cannot be deserialized to a NotifyMessage");
            Assert.True(notification.StatusInfo != null, "Found Receipt notification doesn't have a <StatusInfo/> element");
            Assert.Equal(Status.Delivered, notification.StatusInfo.Status);
            Assert.True(notification.StatusInfo.Any?.Any(), "Found Receipt notification doesn't have a <SignalMessage/> included");
            Assert.True(
                notification.StatusInfo.Any?.First()?.SelectSingleNode("//*[local-name()='NonRepudiationInformation']") != null,
                "Found Receipt notification doesn't have a <NonRepudiationInformation/> element");
        }

        [Fact]
        public async Task Test_8_1_15_SubmitMessage_Via_Http_Result_In_Notified_Receipt()
        {
            // Arrange
            AS4Component.OverrideSettings("8.1.15-settings.xml");
            AS4Component.Start();
            const string submitAgentHttpEndpoint = "http://localhost:5001/";

            Holodeck.CopyPModeToHolodeckB("8.1.15-pmode.xml");

            // Act
            await HttpClient.SendMessageAsync(
                submitAgentHttpEndpoint,
                submitmessage_8_1_15_xml,
                "application/xml");

            // Assert
            await PollingService.PollUntilPresentAsync(AS4Component.ReceiptsPath);

            Holodeck.AssertSinglePayloadOnHolodeckB();
            AS4Component.AssertReceipt();
        }
    }
}
