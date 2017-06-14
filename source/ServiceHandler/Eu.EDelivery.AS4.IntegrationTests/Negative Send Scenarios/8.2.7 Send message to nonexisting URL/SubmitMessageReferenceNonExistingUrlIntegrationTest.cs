using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._7_Send_message_to_nonexisting_URL
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SubmitMessageReferenceNonExistingUrlIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.2.7-sample.xml";
        private readonly string _as4MessagesPath = $"{IntegrationTestTemplate.AS4MessagesRootPath}{SubmitMessageFilename}";
        private readonly string _as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        
        [Fact]
        public void ThenSendingSubmitMessageFails()
        {
            // Before
            AS4Component.Start();
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(AS4ExceptionsPath);

            // Act
            File.Copy(_as4MessagesPath, _as4OutputPath);

            // Assert
            Assert.True(AreExceptionFilesFound());
        }

        private bool AreExceptionFilesFound()
        {
            return PollingAt(AS4ExceptionsPath);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            Assert.Equal(1, files.Count());
            AssertNotifyException();
        }

        private static void AssertNotifyException()
        {
            FileInfo notifyException = new DirectoryInfo(AS4ExceptionsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(notifyException);
        }
    }
}