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
        private string _sharedMessageId;

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
            this._sharedMessageId = Guid.NewGuid().ToString();
            messageIdNode.InnerText = this._sharedMessageId;

            xmlDocument.Save(this._as4MessagesPath);
        }

        [Fact]
        public async void ThenSendAsyncErrorSucceedsAsync()
        {
            // Before
            base.CleanUpFiles(base.HolodeckBInputPath);
            base.StartApplication();
            base.CleanUpFiles(AS4FullOutputPath);
            base.CleanUpFiles(Properties.Resources.holodeck_B_pmodes);
            base.CleanUpFiles(AS4ErrorsPath);

            // Arrange
            File.Copy(this._as4MessagesPath, this._as4OutputPath);
            base.CopyPModeToHolodeckB("8.1.13-pmode.xml");

            // Act
            string messageWrongSigned = Properties.Resources.as4_soap_wrong_signed_callback_message;
            messageWrongSigned = messageWrongSigned
                .Replace("2e0a5701-790a-4a53-a8b7-e7f528fc1b53@10.124.29.131", this._sharedMessageId);

            await this._sender.SendAsync(messageWrongSigned, Constants.ContentTypes.Soap);

            // Assert
            bool areFilesFound = AreFilesFound();
            if (areFilesFound) Console.WriteLine(@"Receive Async Error Integration Test succeeded!");
            else Retry();
        }

        private void Retry()
        {
            var startDir = new DirectoryInfo(AS4FullInputPath);
            FileInfo[] files = startDir.GetFiles("*.jpg", SearchOption.AllDirectories);
            Console.WriteLine($@"Polling failed, retry to check for the files. {files.Length} Files are found");

            ValidatePolledFiles(files);
        }

        private bool AreFilesFound()
        {
            const int retryCount = 2000;
            return base.PollTo(AS4ErrorsPath, "*.xml", retryCount);
        }

        protected override void ValidatePolledFiles(IEnumerable<FileInfo> files)
        {
           Assert.NotEmpty(files);
           Console.WriteLine(@"Notify Error Files are found");
        }
    }
}
