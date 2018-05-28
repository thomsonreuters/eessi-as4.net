using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Strategies.Method;
using Xunit;
using MessageInfo = Eu.EDelivery.AS4.Model.Common.MessageInfo;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    public class GivenFileSenderFacts : IDisposable
    {
        private static readonly string ExpectedDirectoryPath = Directory.GetCurrentDirectory();
        private static readonly string ExpectedFileName = Path.Combine(ExpectedDirectoryPath, AnonymousDeliverMessage().MessageInfo.MessageId + ".xml");

        [Fact]
        public async Task StoresFileOnFileSystem_IfDeliverMessage()
        {
            // Arrange
            var sut = new FileSender();
            sut.Configure(new LocationMethod(ExpectedDirectoryPath));

            // Act
            SendResult r = await sut.SendAsync(AnonymousDeliverMessage());

            // Assert
            Assert.Equal(SendResult.Success, r);
            Assert.True(File.Exists(ExpectedFileName));
        }

        [Fact]
        public async Task Deliver_Returns_Retryable_If_File_Is_In_Use()
        {
            // Arrange
            var sut = new FileSender();
            sut.Configure(new LocationMethod(ExpectedDirectoryPath));

            using (new FileStream(ExpectedFileName, FileMode.Create))
            {
                // Act
                SendResult r = await sut.SendAsync(AnonymousDeliverMessage());

                // Assert
                Assert.Equal(SendResult.RetryableFail, r);
            }
        }

        private static DeliverMessageEnvelope AnonymousDeliverMessage()
        {
            return new DeliverMessageEnvelope(
                messageInfo: new MessageInfo("message-id", "mpc"), 
                deliverMessage: new byte[0], 
                contentType: "text/plain");
        }

        [Fact]
        public async Task StoresFileOnFileSystem_IfNotifyMessage()
        {
            // Arrange
            var sut = new FileSender();
            sut.Configure(new LocationMethod(ExpectedDirectoryPath));

            // Act
            SendResult r = await sut.SendAsync(AnonymousNotifyMessage());

            // Assert
            Assert.Equal(SendResult.Success, r);
            Assert.True(File.Exists(ExpectedFileName));
        }

        [Fact]
        public async Task Notify_Returns_Retryable_If_File_Is_In_Use()
        {
            // Arrange
            var sut = new FileSender();
            sut.Configure(new LocationMethod(ExpectedDirectoryPath));

            using (new FileStream(ExpectedFileName, FileMode.Create))
            {
                // Act
                SendResult r = await sut.SendAsync(AnonymousNotifyMessage());

                // Assert
                Assert.Equal(SendResult.RetryableFail, r);
            }
        }

        private static NotifyMessageEnvelope AnonymousNotifyMessage()
        {
            return new NotifyMessageEnvelope(
                messageInfo: new AS4.Model.Notify.MessageInfo { MessageId = "message-id" }, 
                statusCode: default(Status), 
                notifyMessage: new byte[0], 
                contentType: "text/plain", 
                entityType: default(Type));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            File.Delete(ExpectedFileName);
        }
    }
}