using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Eu.EDelivery.AS4.IntegrationTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Send_Scenarios._8._1._13_Receive_Async_Error
{
    /// <summary>
    /// Testing the Application with a Single Payload
    /// </summary>
    public class ReceiveAsyncSignedErrorIntegrationTest : IntegrationTestTemplate
    {
        private const string SubmitMessageFilename = "\\8.1.13-sample.xml";
        private readonly string _as4MessagesPath;
        private readonly string _as4OutputPath;

        private readonly StubSender _sender;

        public ReceiveAsyncSignedErrorIntegrationTest()
        {
            _sender = new StubSender {Url = "http://localhost:9090/msh", HandleResponse = response => null};
            _as4MessagesPath = $"{AS4MessagesRootPath}{SubmitMessageFilename}";
            _as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";
        }

        [Fact]
        public async void ThenSendAsyncErrorSucceedsAsync()
        {
            // Before
            string sharedMessageId = UpdateSubmitMessageId();
            CleanUpFiles(HolodeckBInputPath);
            CleanUpFiles(AS4FullOutputPath);
            CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            CleanUpFiles(AS4ErrorsPath);

            AS4Component.Start();

            // Arrange
            CopyPModeToHolodeckB("8.1.13-pmode.xml");
            File.Copy(_as4MessagesPath, _as4OutputPath);

            // Act
            string messageWrongSigned = ReplaceSubmitMessageIdWith(sharedMessageId);
            await _sender.SendMessage(messageWrongSigned, Constants.ContentTypes.Soap);

            // Assert
            bool areFilesFound = PollingAt(AS4ErrorsPath, "*.xml");
            if (areFilesFound)
            {
                Console.WriteLine(@"Receive Async Error Integration Test succeeded!");
            }

            Assert.True(areFilesFound, "Send Async Error failed");
        }

        private string UpdateSubmitMessageId()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.GetFullPath(_as4MessagesPath));

            XmlNode messageIdNode = xmlDocument.SelectSingleNode("//*[local-name()='MessageId']");
            Assert.NotNull(messageIdNode);

            string sharedMessageId = Guid.NewGuid().ToString();
            messageIdNode.InnerText = sharedMessageId;

            xmlDocument.Save(_as4MessagesPath);

            return sharedMessageId;
        }

        private static string ReplaceSubmitMessageIdWith(string sharedMessageId)
        {
            string messageWrongSigned = Properties.Resources.as4_soap_wrong_signed_callback_message;
            messageWrongSigned = messageWrongSigned.Replace("2e0a5701-790a-4a53-a8b7-e7f528fc1b53@10.124.29.131", sharedMessageId);

            return messageWrongSigned;
        }

        /// <summary>
        /// Perform extra validation for the output files of Holodeck
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
            Assert.NotEmpty(files);
            Console.WriteLine(@"Notify Error Files are found");
        }
    }
}