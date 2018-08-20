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
        [Fact]
        public void ThenSendingSinglePayloadWithMessagePropertiesSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.10-pmode.xml");

            // Act
            AS4Component.PutMessage("8.1.10-sample.xml");            

            // Assert
            bool areFilesFound = PollingAt(AS4ReceiptsPath);
            if (areFilesFound)
            {
                Console.WriteLine(@"Single Payload with Message Properties Integration Test succeeded!");
            }
            Assert.True(areFilesFound, "Single Payload with Message Properties failed");
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
            FileInfo receivedPayload = new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath).GetFiles("*.jpg").FirstOrDefault();
            var sendPayload = new FileInfo(Path.GetFullPath($".\\{Properties.Resources.submitmessage_single_payload_path}"));

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        private static void AssertHolodeckReceiptFile()
        {
            FileInfo receipt = new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath)
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