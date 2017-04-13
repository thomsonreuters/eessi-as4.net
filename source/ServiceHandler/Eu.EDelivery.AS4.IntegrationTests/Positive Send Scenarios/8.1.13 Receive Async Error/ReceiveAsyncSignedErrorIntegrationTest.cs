using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        private static string _sharedMessageId;

        public ReceiveAsyncSignedErrorIntegrationTest()
        {
            this._sender = new StubSender();
            this._sender.Url = "http://localhost:9090/msh";
            this._sender.HandleResponse = response => null;

            this._as4MessagesPath = $"{AS4MessagesPath}{SubmitMessageFilename}";
            this._as4OutputPath = $"{AS4FullOutputPath}{SubmitMessageFilename}";

            UpdateSubmitMessageId();
        }

        private void UpdateSubmitMessageId()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(this._as4MessagesPath);

            XmlNode messageIdNode = xmlDocument.SelectSingleNode("//*[local-name()='MessageId']");
            _sharedMessageId = Guid.NewGuid().ToString();
            messageIdNode.InnerText = _sharedMessageId;

            xmlDocument.Save(this._as4MessagesPath);
        }

        [Fact]
        public async void ThenSendAsyncErrorSucceedsAsync()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartAS4Component();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4ErrorsPath);

            // Arrange
            base.CopyPModeToHolodeckB("8.1.13-pmode.xml");
            File.Copy(this._as4MessagesPath, this._as4OutputPath);
            Thread.Sleep(5000);

            // Act
            string messageWrongSigned = Properties.Resources.as4_soap_wrong_signed_callback_message;
            messageWrongSigned = messageWrongSigned
                .Replace("2e0a5701-790a-4a53-a8b7-e7f528fc1b53@10.124.29.131", _sharedMessageId);

            await this._sender.SendAsync(messageWrongSigned, Constants.ContentTypes.Soap);

            // Assert
            bool areFilesFound = AreFilesFound();
            if (areFilesFound) Console.WriteLine(@"Receive Async Error Integration Test succeeded!");  
        }

        private bool AreFilesFound()
        {
            const int retryCount = 2000;
            return base.PollingAt(AS4ErrorsPath, "*.xml", retryCount);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
           Assert.NotEmpty(files);
           Console.WriteLine(@"Notify Error Files are found");
        }
    }
}
