using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._5._Send_Multiple_Payloads_Encrypted
{
    public class MultiplePayloadsEncryptedIntegrationTest : IntegrationTestTemplate
    {
        [Fact]
        public void Test()
        {
            // Before
            AS4Component.Start();

            // Arrange
            Holodeck.CopyPModeToHolodeckB("8.1.5-pmode.xml");

            // Act
            AS4Component.PutMessage("8.1.5-sample.xml");

            // Assert
            bool areFilesFound = PollingAt(Holodeck.HolodeckBLocations.InputPath);
            if (areFilesFound)
            {
                Console.WriteLine(@"Multiple Payloads Encrypted Integration Test succeeded!");
            }

            Assert.True(areFilesFound, "Multiple Payloads Encryption Failed: no files are found during polling.");
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            AssertPayloads();
        }

        private static void AssertPayloads()
        {
            FileInfo[] receivedPayloads = new DirectoryInfo(Holodeck.HolodeckBLocations.InputPath).GetFiles();

            var sentEarth = new FileInfo($".{Properties.Resources.submitmessage_single_payload_path}");
            var sentXml = new FileInfo($".{Properties.Resources.submitmessage_second_payload_path}");

            // Earth attachment
            FileInfo receivedEarth = receivedPayloads.SingleOrDefault(x => x.Extension == ".jpg");
            FileInfo receivedXml = receivedPayloads.SingleOrDefault(x => x.Name.Contains("sample"));

            Assert.NotNull(receivedEarth);
            Assert.NotNull(receivedXml);

            Assert.Equal(sentEarth.Length, receivedEarth.Length);
            Assert.Equal(sentXml.Length, receivedXml.Length);
        }
    }
}
