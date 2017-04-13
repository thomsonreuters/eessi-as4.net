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
            this._as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendAsyncSignedNRRReceiptSucceeds()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartAS4Component();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            base.CopyPModeToHolodeckB("8.1.12-pmode.xml");

            // Act
            File.Copy(this._as4MessagesPath, this._as4OutputPath);

            // Assert
            bool areFilesFound = AreFilesFound();
            if (areFilesFound) Console.WriteLine(@"Receive Async Signed NRR Receipt Integration Test succeeded!");
            else Retry();
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
            return base.PollingAt(AS4ReceiptsPath, "*.xml", retryCount);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            Assert.NotEmpty(files);
            FileInfo receipt = files.FirstOrDefault();

            Assert.NotNull(receipt);
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(receipt.FullName);

            XmlNode nonRepudiationInformation = xmlDocument
                .SelectSingleNode("//*[local-name()='NonRepudiationInformation']");
            Assert.NotNull(nonRepudiationInformation);
        }
    }
}
