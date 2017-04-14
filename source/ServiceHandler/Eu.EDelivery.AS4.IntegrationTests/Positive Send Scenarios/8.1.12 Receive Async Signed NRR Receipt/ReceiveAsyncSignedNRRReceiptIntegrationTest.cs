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
        private const string SubmitMessageFilename = "\\8.1.12-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public ReceiveAsyncSignedNRRReceiptIntegrationTest()
        {
            _as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
            _as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendAsyncSignedNRRReceiptSucceeds()
        {
            // Before
            CleanUpFiles(HolodeckBInputPath);
            StartAS4Component();
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            CopyPModeToHolodeckB("8.1.12-pmode.xml");

            // Act
            File.Copy(_as4MessagesPath, _as4OutputPath);

            // Assert
            bool areFilesFound = AreFilesFound();
            if (areFilesFound)
            {
                Console.WriteLine(@"Receive Async Signed NRR Receipt Integration Test succeeded!");
            }
            else
            {
                Retry();
            }

            Assert.True(areFilesFound, "Send Async Signed NRR Receipt failed");
        }

        private void Retry()
        {
            var startDir = new DirectoryInfo(AS4FullInputPath);
            FileInfo[] files = startDir.GetFiles("*.jpg", SearchOption.AllDirectories);
            Console.WriteLine($@"Polling failed, retry to check for the files. {files.Length} Files are found");

            ValidatePolledFiles(files);
        }

        private bool AreFilesFound()
        {
            const int retryCount = 2000;
            return PollingAt(AS4ReceiptsPath, "*.xml", retryCount);
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