using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._1._Send_Single_Payload_with_FILE_Submit
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SinglePayloadIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.1.1-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public SinglePayloadIntegrationTest()
        {
            this._as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendingSinglePayloadSucceeds()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartApplication();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            base.CopyPModeToHolodeckB("8.1.1-pmode.xml");

            // Act
            File.Copy(this._as4MessagesPath, this._as4OutputPath);

            // Assert
            bool areFilesFound = base.PollTo(AS4ReceiptsPath);
            if (areFilesFound) Console.WriteLine(@"Single Payload Integration Test succeeded!");
            Assert.True(areFilesFound);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayload();
            AssertReceipt();
        }

        private void AssertPayload()
        {
            FileInfo receivedPayload = new DirectoryInfo(base.HolodeckBInputPath).GetFiles("*.jpg").FirstOrDefault();
            var sendPayload = new FileInfo(Path.GetFullPath(@".\" + Properties.Resources.submitmessage_single_payload_path));

            Assert.NotNull(receivedPayload);
            Assert.Equal(sendPayload.Length, receivedPayload.Length);
        }

        private void AssertReceipt()
        {
            FileInfo receipt = new DirectoryInfo(AS4ReceiptsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }
    }
}