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

        private readonly StubSender _sender;

        public ReceiveDuplicateMessageIntegrationTest()
        {
            _sender = new StubSender();
        }

        [Fact]
        public void ThenSendingSinglePayloadSucceeds()
        {
            // Before
            StartAS4Component();
            CleanUpFiles(AS4FullInputPath);

            // Act
            _sender.SendAsync(Properties.Resources.duplicated_as4message, ContentType);
            CleanUpFiles(AS4FullInputPath);
            _sender.SendAsync(Properties.Resources.duplicated_as4message, ContentType);

            // Assert
            AssertMessageIsNotDelivered();

            // After
            StopApplication();
        }

        private static void AssertMessageIsNotDelivered()
        {
            var deliverDirectory = new DirectoryInfo(AS4FullInputPath);
            FileInfo[] files = deliverDirectory.GetFiles("*.xml");

            Assert.Empty(files);
        }
    }
}
