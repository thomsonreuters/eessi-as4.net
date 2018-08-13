using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._2._Send_Multiple_Payloads_with_FILE_Submit
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class MultiplePayloadIntegrationTest : IntegrationTestTemplate
    {        
        [Fact]
        public void ThenSendingMultiplePayloadSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.2-pmode.xml");

            // Act
            AS4Component.PutMessage("8.1.2-sample.xml");            

            // Assert
            bool areFilesFound = PollingAt(AS4ReceiptsPath);
            if (areFilesFound) Console.WriteLine(@"Multiple Payloads Integration test succeeded!");
            Assert.True(areFilesFound, "Multiple Payloads failed");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayloads();
            AssertReceipt();
        }

        private void AssertPayloads()
        {
            FileInfo[] receivedPayloads = new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath).GetFiles();

            FileInfo sentEarth = AS4Component.SubmitSinglePayloadImage;
            FileInfo sentXml = AS4Component.SubmitSecondPayloadXml;

            // Earth attachment
            FileInfo receivedEarth = receivedPayloads.FirstOrDefault(x => x.Extension == ".jpg");
            FileInfo receivedXml = receivedPayloads.FirstOrDefault(x => x.Name.Contains("sample"));

            Assert.NotNull(receivedEarth);
            Assert.NotNull(receivedXml);

            Assert.Equal(sentEarth.Length, receivedEarth.Length);
            Assert.Equal(sentXml.Length, receivedXml.Length);
        }

        private void AssertReceipt()
        {
            FileInfo receipt = new DirectoryInfo(AS4ReceiptsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }
    }
}