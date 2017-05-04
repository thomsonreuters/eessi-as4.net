using System;
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
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public ReceiveAsyncSignedNRRReceiptIntegrationTest()
        {
            const string submitMessage = "\\8.1.12-sample.xml";
            _as4MessagesPath = $"{AS4MessagesRootPath}{submitMessage}";
            _as4OutputPath = $"{AS4FullOutputPath}{submitMessage}";
        }

        [Retry(MaxRetries = 3)]
        public void ThenSendAsyncSignedNRRReceiptSucceeds()
        {
            // Before
            CleanUpFiles(HolodeckBInputPath);
            AS4Component.Start();
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            CopyPModeToHolodeckB("8.1.12-pmode.xml");

            // Act
            File.Copy(_as4MessagesPath, _as4OutputPath);

            // Assert
            Assert.True(PollingAt(AS4ReceiptsPath, "*.xml"), "Send Async Signed NRR Receipt failed");
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