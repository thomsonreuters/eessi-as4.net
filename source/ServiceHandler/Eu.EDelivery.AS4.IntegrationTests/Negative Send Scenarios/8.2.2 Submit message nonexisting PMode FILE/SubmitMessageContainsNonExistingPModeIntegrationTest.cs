using System;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._2_Submit_message_nonexisting_PMode_FILE
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SubmitMessageContainsNonExistingPModeIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.2.2-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public SubmitMessageContainsNonExistingPModeIntegrationTest()
        {
            this._as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendingSubmitMessageFails()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartAS4Component();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);

            // Act
            File.Copy( this._as4MessagesPath, this._as4OutputPath);

            // Assert
            bool areFilesFound = AreExceptionFilesFound();
            if (areFilesFound) Console.WriteLine(@"Submit Message with non-existing PMode Integration Test succeeded!");
            Assert.True(areFilesFound);
        }

        private bool AreExceptionFilesFound()
        {
            return 
                base.PollingAt(AS4FullOutputPath, "*.exception") && 
                base.PollingAt(AS4FullOutputPath, "*.exception.details");
        }
    }
}