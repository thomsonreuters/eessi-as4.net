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
        private const string SubmitMessageFilename = "\\8.1.5-sample.xml";
        private readonly string _as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
        private readonly string _as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";

        [Fact]
        public void Test()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartApplication();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4ReceiptsPath);

            // Arrange
            base.CopyPModeToHolodeckB("8.1.5-pmode.xml");

            // Act
            File.Copy(this._as4MessagesPath, this._as4OutputPath);

            // Assert
            bool areFilesFound = base.PollTo(HolodeckBInputPath);
            if (areFilesFound) Console.WriteLine(@"Multiple Payloads Encrypted Integration Test succeeded!");
            Assert.True(areFilesFound);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            AssertPayloads();
        }

        private void AssertPayloads()
        {
            FileInfo[] receivedPayloads = new DirectoryInfo(HolodeckBInputPath).GetFiles();

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
