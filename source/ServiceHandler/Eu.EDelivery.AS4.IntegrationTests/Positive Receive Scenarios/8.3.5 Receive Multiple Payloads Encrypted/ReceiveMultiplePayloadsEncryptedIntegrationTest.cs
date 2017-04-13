using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._5_Receive_Multiple_Payloads_Encrypted
{
    /// <summary>
    /// Testing the Application with multiple payloads encrypted
    /// </summary>
    public class ReceiveMultiplePayloadsEncryptedIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.3.5-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveMultiplePayloadsEncryptedIntegrationTest()
        {
            this._holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            this._destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            this._holodeck = new Holodeck();
        }

        [Fact]
        public void ThenReceiveMultiplePayloadsEncryptedSucceeds()
        {
            // Before
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.StartApplication();
            base.CleanUpFiles(AS4FullInputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            base.CopyPModeToHolodeckA("8.3.5-pmode.xml");

            // Act
            File.Copy(this._holodeckMessagesPath, this._destFileName);

            // Assert
            bool areFilesFound = base.PollTo(Properties.Resources.holodeck_A_input_path);
            if (areFilesFound) Console.WriteLine(@"Receive Multiple Payloads Encrypted Integration Test succeeded!");
            else Retry();
        }

        private void Retry()
        {
            var startDir = new DirectoryInfo(AS4FullInputPath);
            FileInfo[] files = startDir.GetFiles("*.jpg", SearchOption.AllDirectories);
            Console.WriteLine($@"Polling failed, retry to check for the files. {files.Length} Files are found");

            ValidatePolledFiles(files);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayloads(new DirectoryInfo(AS4FullInputPath).GetFiles("*.jpg"));
            AssertXmlFiles(new DirectoryInfo(AS4FullInputPath).GetFiles("*.xml"));

            this._holodeck.AssertReceiptOnHolodeckA();
        }

        private void AssertPayloads(IEnumerable<FileInfo> files)
        {
            var sendPayload = new FileInfo(Properties.Resources.holodeck_payload_path);

            foreach (FileInfo receivedPayload in files)
                if (receivedPayload != null)
                    Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        private void AssertXmlFiles(IEnumerable<FileInfo> files)
        {
            int count = files.Count();
            if (count == 0) return;

            Assert.Equal(2, count);
            Console.WriteLine($@"There're {count} incoming Xml Documents found");
        }
    }
}
