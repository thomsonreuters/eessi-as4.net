using System;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._1_Submit_message_override_not_allowed_FILE
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SubmitMessageOverrideNotAllowedIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.2.1-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public SubmitMessageOverrideNotAllowedIntegrationTest()
        {
            this._as4MessagesPath = $"{IntegrationTestTemplate.AS4MessagesPath}{SubmitMessageFilename}";
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public void ThenSendingSubmitMessageFails()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartApplication();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);

            // Act
            File.Copy( this._as4MessagesPath, this._as4OutputPath);

            // Assert
            bool areFilesFound = AreExceptionFilesFound();
            if (areFilesFound) Console.WriteLine(@"Submit Message override not allowed Integration Test succeeded!");
            Assert.True(areFilesFound);
        }

        private bool AreExceptionFilesFound()
        {
            return 
                base.PollTo(AS4FullOutputPath, "*.exception") && 
                base.PollTo(AS4FullOutputPath, "*.exception.details");
        }
    }
}