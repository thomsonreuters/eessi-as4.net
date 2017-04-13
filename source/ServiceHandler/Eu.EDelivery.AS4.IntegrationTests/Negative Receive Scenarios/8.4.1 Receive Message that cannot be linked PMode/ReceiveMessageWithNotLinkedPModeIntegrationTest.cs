using System;
using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Receive_Scenarios._8._4._1_Receive_Message_that_cannot_be_linked_PMode
{
    /// <summary>
    /// Testing the Application with a received message
    /// that doesn't link an existing Receiving PMode
    /// </summary>
    public class ReceiveMessageWithNotLinkedPModeIntegrationTest : IntegrationTestTemplate
    {
        private const string HolodeckMessageFilename = "\\8.4.1-sample.mmd";
        private readonly string _holodeckMessagesPath;
        private readonly string _destFileName;
        private readonly Holodeck _holodeck;

        public ReceiveMessageWithNotLinkedPModeIntegrationTest()
        {
            this._destFileName = $"{Properties.Resources.holodeck_A_output_path}{HolodeckMessageFilename}";
            this._holodeckMessagesPath = Path.GetFullPath($"{HolodeckMessagesPath}{HolodeckMessageFilename}");
            this._holodeck = new Holodeck();
        }

        [Fact]
        public void ThenSendingSinglePayloadSucceeds()
        {
            // Before
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.StartAS4Component();
            base.CleanUpFiles(AS4FullInputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_A_pmodes);
            base.CleanUpFiles(Properties.Resources.holodeck_A_output_path);
            base.CleanUpFiles(Properties.Resources.holodeck_A_input_path);

            // Arrange
            base.CopyPModeToHolodeckA("8.4.1-pmode.xml");

            // Act
            File.Copy(this._holodeckMessagesPath, this._destFileName);

            // Assert
            bool areFilesFound = base.PollingAt(Properties.Resources.holodeck_A_input_path, "*.xml");
            if (areFilesFound) Console.WriteLine(@"Receive Message with not linked PMode Integration Test succeeded!");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            this._holodeck.AssertErrorOnHolodeckA(ErrorCode.Ebms0001);
        }
    }
}
