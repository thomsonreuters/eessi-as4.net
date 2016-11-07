using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._15_Receive_Duplicate_Message
{
    /// <summary>
    /// Testing the Application with a Test Message
    /// </summary>
    public class ReceiveDuplicateMessageIntegrationTest : IntegrationTestTemplate
    {
        private const string ContentType = "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        private readonly Holodeck _holodeck;
        private readonly StubSender _sender;

        public ReceiveDuplicateMessageIntegrationTest()
        {
            this._holodeck = new Holodeck();
            this._sender = new StubSender();
        }

        [Fact]
        public void ThenSendingSinglePayloadSucceeds()
        {
            // Before
            base.StartApplication();
            base.CleanUpFiles(AS4FullInputPath);

            // Act
            this._sender.SendAsync(Properties.Resources.duplicated_as4message, ContentType);

            // Assert
            AssertMessageIsNotDelivered();
            this._holodeck.AssertReceiptOnHolodeckA();

            // After
            base.StopApplication();
        }

        private void AssertMessageIsNotDelivered()
        {
            var deliverDirectory = new DirectoryInfo(AS4FullInputPath);
            FileInfo[] files = deliverDirectory.GetFiles("*.xml");

            Assert.Empty(files);
        }
    }
}
