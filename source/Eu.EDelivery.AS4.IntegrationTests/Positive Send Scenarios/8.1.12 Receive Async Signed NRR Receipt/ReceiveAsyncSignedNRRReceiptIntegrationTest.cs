using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._12_Receive_Async_Signed_NRR_Receipt
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveAsyncSignedNRRReceiptIntegrationTest : IntegrationTestTemplate
    {

        [Retry(MaxRetries = 3)]
        public void ThenSendAsyncSignedNRRReceiptSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.12-pmode.xml");

            // Act
            AS4Component.PutMessage("8.1.12-sample.xml");

            // Assert
            Assert.True(PollingAt(AS4ReceiptsPath, "*.xml", retryCount: 100000), "Send Async Signed NRR Receipt failed");
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            Assert.NotEmpty(files);
            FileInfo receipt = files.FirstOrDefault();

            Assert.NotNull(receipt);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText(receipt.FullName));

            XmlNode nonRepudiationInformation = xmlDocument.SelectSingleNode("//*[local-name()='NonRepudiationInformation']");
            Assert.NotNull(nonRepudiationInformation);
        }
    }
}