using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._14_Receive_Test_Message
{
    /// <summary>
    /// Testing the Application with a Test Message
    /// </summary>
    public class ReceiveTestMessageIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.3.14-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveTestMessageIntegrationTest()
        {
            _holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            _destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            _holodeck = new Holodeck();
        }

        [Retry(MaxRetries = 3)]
        public void ThenReceiveTestMessageSucceeds()
        {
            // Before
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            AS4Component.Start();
            CleanUpFiles(AS4FullInputPath);
            CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            CopyPModeToHolodeckA("8.3.14-pmode.xml");

            // Act
            File.Copy(_holodeckMessagesPath, _destFileName);

            // Assert
            Assert.True(
                PollingAt(Properties.Resources.holodeck_A_input_path, "*.xml", 5000),
                "Receive Test Message Integration Test failed");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            this._holodeck.AssertReceiptOnHolodeckA();
            AssertMessageIsNotDelivered();
        }

        private void AssertMessageIsNotDelivered()
        {
            string fullDeliverPath = Path.GetFullPath(AS4FullInputPath);
            var deliverDirectory = new DirectoryInfo(fullDeliverPath);
            FileInfo[] files = deliverDirectory.GetFiles("*.xml");

            Assert.Empty(files);
        }
    }
}
