using System;
using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._13_Receive_Single_Payload_Return_Async_Signed_NRR_Receipt
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveSinglePayloadReturnAsyncSignedIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.3.13-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveSinglePayloadReturnAsyncSignedIntegrationTest()
        {
            this._holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            this._destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            this._holodeck = new Holodeck();
        }

        [Fact(Skip = "Datastore is locked?")]
        public void ThenSendingSinglePayloadSucceeds()
        {
            // Before
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            CopyPModeToHolodeckA("8.3.13-pmode.xml");

            // Act
            File.Copy(_holodeckMessagesPath, _destFileName);

            // Assert
            bool areFilesFound = PollingAt(Properties.Resources.holodeck_A_input_path);
            if (areFilesFound)
            {
                Console.WriteLine(@"Receive Single Payload Return Async Signed NRR Integration Test succeeded!");
            }

            Assert.True(areFilesFound, "Receive Single Payload Return Async Signed NRR Integration Test failed!");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            _holodeck.AssertReceiptOnHolodeckA();
        }
    }
}
