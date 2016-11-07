using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._10._Send_Single_Payload_with_Message_Properties
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SinglePayloadMessagePropertiesIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.1.10-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public SinglePayloadMessagePropertiesIntegrationTest()
        {
            this._as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendingSinglePayloadWithMessagePropertiesSucceeds()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartApplication();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            base.CopyPModeToHolodeckB("8.1.10-pmode.xml");

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
            AssertHolodeckReceiptFile();
            AssertAS4Receipt();
        }

        private void AssertPayloads()
        {
            FileInfo receivedPayload = new DirectoryInfo(base.HolodeckBInputPath).GetFiles("*.jpg").FirstOrDefault();
            var sendPayload = new FileInfo($"{OutputPrefix}{Properties.Resources.submitmessage_single_payload_path}");

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        private void AssertHolodeckReceiptFile()
        {
            FileInfo receipt = new DirectoryInfo(base.HolodeckBInputPath)
                .GetFiles("*.xml").FirstOrDefault();

            var xmlDocument = new XmlDocument();
            if (receipt != null) xmlDocument.Load(receipt.FullName);

            XmlNode messagePropertyNode = xmlDocument.SelectSingleNode("//*[local-name()='MessageProperties']");
            if (messagePropertyNode == null) return;

            Assert.Equal(2, messagePropertyNode.ChildNodes.Count);
            Console.WriteLine(@"Two Message Properties found in sended Receipt");
        }

        private void AssertAS4Receipt()
        {
            FileInfo receipt = new DirectoryInfo(AS4ReceiptsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }
    }
}