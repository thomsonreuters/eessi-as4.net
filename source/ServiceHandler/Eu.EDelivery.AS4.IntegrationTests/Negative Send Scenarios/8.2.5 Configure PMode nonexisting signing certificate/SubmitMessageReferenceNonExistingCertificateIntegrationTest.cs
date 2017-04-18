using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._5_Configure_PMode_nonexisting_signing_certificate
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class SubmitMessageReferenceNonExistingCertificateIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.2.5-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        public SubmitMessageReferenceNonExistingCertificateIntegrationTest()
        {
            this._as4MessagesPath = $"{AS4MessagesRootPath}{SubmitMessageFilename}";
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
            base.CleanUpFiles(AS4ExceptionsPath);

            // Act
            File.Copy( this._as4MessagesPath, this._as4OutputPath);

            // Assert
            bool areFilesFound = AreExceptionFilesFound();
            if (areFilesFound) Console.WriteLine(@"Submit Message with invalid signing certificate Integration Test succeeded!");
            Assert.True(areFilesFound);
        }

        private bool AreExceptionFilesFound()
        {
            return base.PollingAt(AS4ExceptionsPath);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            Assert.Equal(1, files.Count());
            AssertNotifyException();
        }

        private void AssertNotifyException()
        {
            FileInfo notifyException = new DirectoryInfo(AS4ExceptionsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(notifyException);
        }
    }
}