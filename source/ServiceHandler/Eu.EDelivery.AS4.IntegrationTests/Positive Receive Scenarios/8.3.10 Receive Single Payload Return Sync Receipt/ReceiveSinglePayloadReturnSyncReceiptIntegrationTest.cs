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
            _holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            _destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            _holodeck = new Holodeck();
        }

        [Fact]
        public void ThenReceiveSignalPayloadReturnSyncReceiptSucceeds()
        {
            // Before
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            CopyPModeToHolodeckA("8.3.10-pmode.xml");

            // Act
            File.Copy(_holodeckMessagesPath, _destFileName);

            // Assert
            bool areFilesFound = PollingAt(Properties.Resources.holodeck_A_input_path);
            if (areFilesFound)
            {
                Console.WriteLine(@"Receive Single Payload Return Sync Integration Test succeeded!");
            }
            Assert.True(areFilesFound);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            _holodeck.AssertReceiptOnHolodeckA();
        }
    }
}