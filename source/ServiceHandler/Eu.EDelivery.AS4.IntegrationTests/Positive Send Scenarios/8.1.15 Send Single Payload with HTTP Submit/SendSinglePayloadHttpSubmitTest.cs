using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._15_Send_Single_Payload_with_HTTP_Submit
{
    public class SendSinglePayloadHttpSubmitTest : IntegrationTestTemplate
    {
        [Fact]
        public async Task RunIntegrationTest()
        {
            // Before
            CleanUpFiles(HolodeckBInputPath);
            
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            CleanUpFiles(AS4ReceiptsPath);

            AS4Component.OverrideSettings("8.1.15-settings.xml");
            AS4Component.Start();
            var stubSender = new StubSender {Url = "http://localhost:5001/"};

            // Arrange
            CopyPModeToHolodeckB("8.1.15-pmode.xml");

            // Act
            await stubSender.SendMessage(Properties.Resources.submitmessage_8_1_15_xml, "application/xml");

            // Assert
            Assert.True(PollingAt(AS4ReceiptsPath), "Send Single Payload HTTP Submit failed");
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            var holodeck = new Holodeck();

            holodeck.AssertSinglePayloadOnHolodeckB();
            AS4Component.AssertReceipt();
        }
    }
}
