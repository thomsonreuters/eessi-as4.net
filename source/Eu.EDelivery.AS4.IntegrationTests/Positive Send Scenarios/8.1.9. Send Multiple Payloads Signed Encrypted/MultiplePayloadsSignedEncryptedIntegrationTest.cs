using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._9._Send_Multiple_Payloads_Signed_Encrypted
{
    /// <summary>
    /// Testing the Application with multiple payloads signed and encrypted
    /// </summary>
    public class MultiplePayloadsSignedEncryptedIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "8.1.9-sample.xml";
        
        [Fact]
        public void ThenSendingMultiplePayloadCompressedEncryptedSucceeds()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.9-pmode.xml");

            // Act
            AS4Component.PutMessage(SubmitMessageFilename);            

            // Assert
            bool areFilesFound = PollingAt(AS4ReceiptsPath);
            if (areFilesFound)
            {
                Console.WriteLine(@"Multiple Payloads Signed and Encrypted Integration Test succeeded!");
            }

            Assert.True(areFilesFound, "Multiple Payloads Signed and Encrypted failed");
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            // Assert
            AssertPayloads();
            AssertReceipt();
        }

        private static void AssertPayloads()
        {
            FileInfo[] receivedPayloads = new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath).GetFiles();

            var sentEarth = new FileInfo($".{Properties.Resources.submitmessage_single_payload_path}");
            var sentXml = new FileInfo($".{Properties.Resources.submitmessage_second_payload_path}");

            // Earth attachment
            FileInfo receivedEarth = receivedPayloads.FirstOrDefault(x => x.Extension == ".jpg");
            FileInfo receivedXml = receivedPayloads.FirstOrDefault(x => x.Name.Contains("sample") &&  x.Extension == ".xml");            

            Assert.NotNull(receivedEarth);
            Assert.NotNull(receivedXml);

            Assert.Equal(sentEarth.Length, receivedEarth.Length);
            Assert.Equal(sentXml.Length, receivedXml.Length);            
        }

        private static void AssertReceipt()
        {
            FileInfo receipt = new DirectoryInfo(AS4ReceiptsPath).GetFiles("*.xml").FirstOrDefault();

            Assert.NotNull(receipt);
        }
    }
}
