using System;
using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._10_Receive_Single_Payload_Return_Sync_Receipt
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveSinglePayloadReturnSyncReceiptIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.3.10-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveSinglePayloadReturnSyncReceiptIntegrationTest()
        {
            this._holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            this._destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            this._holodeck = new Holodeck();
        }

        [Fact]
        public void ThenReceiveSignalPayloadReturnSyncReceiptSucceeds()
        {
            // Before
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.StartApplication();
            base.CleanUpFiles(AS4FullInputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            base.CopyPModeToHolodeckA("8.3.10-pmode.xml");

            // Act
            File.Copy(this._holodeckMessagesPath, this._destFileName);

            // Assert
            bool areFilesFound = base.PollTo(Properties.Resources.holodeck_A_input_path);
            if (areFilesFound) Console.WriteLine(@"Receive Single Payload Return Sync Integration Test succeeded!");
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
            this._holodeck.AssertReceiptOnHolodeckA();
        }
    }
}