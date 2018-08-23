using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._11_Send_Single_Payload_with_Message_Ids
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SinglePayloadMessageIdsIntegrationTest : IntegrationTestTemplate
    {               
        [Fact]
        public void ThenSendingSinglePayloadWithMessageIdsSucceeds()
        {
            // Before
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.11-pmode.xml");

            AS4Component.PutMessage("8.1.11-sample.xml");            

            // Act
            AS4Component.Start();

            // Assert
            Assert.True(PollingAt(AS4ReceiptsPath), "Send Single Payload with Message Id failed");
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="_">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> _)
        {
            IEnumerable<FileInfo> holodeckInFiles = new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath).GetFiles();
            AS4Component.AssertEarthPayload(holodeckInFiles.FirstOrDefault(f => f.Extension.Equals(".jpg")));

            XmlDocument userDoc = LoadUserMessageDocFrom(holodeckInFiles);
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

        private static void AssertXmlTag(string localName, XmlNode xmlDocument)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode($"//*[local-name()='{localName}']");

            Assert.True(xmlNode != null, $"Cannot find XML tag {localName} in Holodeck UserMessage");
            Console.WriteLine($@"{localName} found in Receipt");
        }
    }
}