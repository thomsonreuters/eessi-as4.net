using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Negative_Send_Scenarios._8._2._6_Configure_PMode_nonexisting_encryption_certificate
{
    /// <summary>
    /// Testin the Application with a configured PMode which reference a non-existing encryption certificate
    /// </summary>
    public class SendMessageWithNonExistingEncryptionCertificateIntegrationTest : IntegrationTestTemplate
    {        
        [Fact]
        public void ThenSendingSubmitMessageFails()
        {
            // Before
            AS4Component.Start();

            // Act            
            AS4Component.PutMessage("8.2.6-sample.xml");

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
            Assert.Single(files);
            AssertNotifyException();
        }

        private static void AssertNotifyException()
        {
            FileInfo notifyException = new DirectoryInfo(AS4ExceptionsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(notifyException);
        }
    }
}
