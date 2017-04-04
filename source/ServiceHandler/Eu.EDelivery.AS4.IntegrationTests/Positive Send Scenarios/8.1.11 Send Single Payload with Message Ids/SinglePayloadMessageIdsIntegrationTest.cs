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
        private const string SubmitMessageFilename = "\\8.1.11-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public SinglePayloadMessageIdsIntegrationTest()
        {
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
            this._as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendingSinglePayloadWithMessageIdsSucceeds()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartApplication();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            base.CopyPModeToHolodeckB("8.1.11-pmode.xml");

            // Act
            File.Copy(this._as4MessagesPath, this._as4OutputPath);

            // Assert
            bool areFilesFound = base.PollTo(AS4ReceiptsPath);
            if (areFilesFound) Console.WriteLine(@"Single Payload with Message Properties Integration Test succeeded!");
            Assert.True(areFilesFound);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayloads();
            AssertAS4Receipt();
        }

        private void AssertPayloads()
        {
            IEnumerable<FileInfo> files = new DirectoryInfo(base.HolodeckBInputPath).GetFiles();

            FileInfo receivedPayload = files.FirstOrDefault(f => f.Extension.Equals(".jpg"));
            var sendPayload = new FileInfo(Properties.Resources.submitmessage_single_payload_path);

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);

            FileInfo receipt = files.FirstOrDefault(f => f.Extension.Equals(".xml"));
            if (receipt != null) AssertHolodeckReceipt(receipt);
        }

        private void AssertHolodeckReceipt(FileInfo receipt)
        {
            var xmlDocument = new XmlDocument();
            if (receipt != null) xmlDocument.Load(receipt.FullName);

            AssertXmlTag("RefToMessageId", xmlDocument);
            AssertXmlTag("MessageId", xmlDocument);
            AssertXmlTag("ConversationId", xmlDocument);
        }

        private void AssertXmlTag(string localName, XmlDocument xmlDocument)
        {
            XmlNode xmlNode = xmlDocument
                .SelectSingleNode($"//*[local-name()='{localName}']");

            Assert.NotNull(xmlNode);
            Console.WriteLine($@"{localName} found in Receipt");
        }

        private void AssertAS4Receipt()
        {
            FileInfo receipt = new DirectoryInfo(AS4ReceiptsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }
    }
}