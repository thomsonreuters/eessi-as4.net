using System;
using System.IO;
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
        public void StoresFileOnFileSystem_IfDeliverMessage()
        {
            // Arrange
            var sut = new FileSender();
            sut.Configure(new LocationMethod(ExpectedDirectoryPath));

            // Act
            sut.Send(AnonymousDeliverMessage());

            // Assert
            Assert.True(File.Exists(ExpectedFileName));
        }

        private static DeliverMessageEnvelope AnonymousDeliverMessage()
        {
            return new DeliverMessageEnvelope(new MessageInfo("message-id", "mpc"), new byte[0], "text/plain");
        }

        [Fact]
        public void StoresFileOnFileSystem_IfNotifyMessage()
        {
            // Arrange
            var sut = new FileSender();
            sut.Configure(new LocationMethod(ExpectedDirectoryPath));

            // Act
            sut.Send(AnonymousNotifyMessage());

            Assert.True(File.Exists(ExpectedFileName));
        }

        private static NotifyMessageEnvelope AnonymousNotifyMessage()
        {
            return new NotifyMessageEnvelope(new AS4.Model.Notify.MessageInfo { MessageId = "message-id" }, default(Status), new byte[0], "text/plain");
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