using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._1_Receive_Single_Payload_with_FILE_Deliver
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveSinglePayloadIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.3.1-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveSinglePayloadIntegrationTest()
        {
            _destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            _holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            _holodeck = new Holodeck();
        }

        [Fact]
        public void ThenReceiveSignalPayloadSucceeds()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);

            // Arrange
            CopyPModeToHolodeckA("8.3.1-pmode.xml");

            // Act
            File.Copy(_holodeckMessagesPath, _destFileName);

            // Assert
            bool areFilesFound = PollingAt(AS4FullInputPath);
            Assert.True(areFilesFound, "Receive Single Payload fails");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            _holodeck.AssertDandelionPayload();
            _holodeck.AssertReceiptOnHolodeckA();
        }
    }
}