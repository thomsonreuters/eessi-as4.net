using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Streaming;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    public class GivenReceiveMessageTransformerFacts
    {
        [Fact]
        public async Task Throws_InvalidMessage_When_Incoming_Stream_Isnt_AS4Message()
        {
            // Arrange
            Stream str = new MemoryStream(
                Encoding.UTF8.GetBytes(
                    "<root>This is definitly not an AS4Message!</root>"));

            var incoming = new ReceivedMessage(str, Constants.ContentTypes.Soap);
            var sut = new ReceiveMessageTransformer(StubConfig.Default);

            // Act / Assert
            await Assert.ThrowsAsync<InvalidMessageException>(
                () => sut.TransformAsync(incoming));
        }

        [CustomProperty]
        public void Throws_InvalidMessage_When_Receiving_SignalMessage_While_Having_A_ReceivingPMode_Configured(SignalMessage s)
        {
            // Arrange
            AS4Message receipt = AS4Message.Create(s);
            var incoming = new ReceivedMessage(receipt.ToStream(), Constants.ContentTypes.Soap);

            var sut = new ReceiveMessageTransformer(StubConfig.Default);
            sut.Configure(
                new Dictionary<string, string>
                    { [ReceiveMessageTransformer.ReceivingPModeKey] = "pmode-id" });

            // Act / Assert
            Assert.Throws<InvalidMessageException>(
                () => sut.TransformAsync(incoming).GetAwaiter().GetResult());

        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Wraps_Into_VirtualStream_If_Cant_Seek(bool canSeek)
        {
            // Arrange
            var stub = new StubStream(canSeek, AS4Message.Empty.ToStream());
            var sut = new ReceiveMessageTransformer(StubConfig.Default);

            // Act
            MessagingContext result = await sut.TransformAsync(
                new ReceivedMessage(stub, Constants.ContentTypes.Soap));

            // Assert
            Assert.True(
                result.ReceivedMessage.UnderlyingStream is VirtualStream != canSeek, 
                "Incoming stream isn't wrapped in 'VirtualStream'");
        }


        [Theory]
        [InlineData("none-existing-id")]
        [InlineData("")]
        public async Task Fails_When_ReceivePMode_Is_Not_Defined(string id)
        {
            // Arrange
            var stub = new Mock<IConfig>();
            stub.Setup(c => c.GetReceivingPModes())
                .Returns(new[] { new ReceivingProcessingMode { Id = "existing-id" } });

            var sut = new ReceiveMessageTransformer(stub.Object);
            sut.Configure(
                new Dictionary<string, string>
                    { [ReceiveMessageTransformer.ReceivingPModeKey] = id });

            var msg = new ReceivedMessage(
                AS4Message.Empty.ToStream(), 
                Constants.ContentTypes.Soap);

            // Act / Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.TransformAsync(msg));
        }

        [Theory]
        [InlineData("existing-id")]
        [InlineData(null)]
        public async Task Adds_ReceivePMode_When_PMode_Setting_Is_Defined(string id)
        {
            // Arrange
            var stub = new Mock<IConfig>();
            stub.Setup(c => c.GetReceivingPModes())
                  .Returns(new[] { new ReceivingProcessingMode {Id = "existing-id" } });

            var sut = new ReceiveMessageTransformer(stub.Object);
            sut.Configure(
                new Dictionary<string, string>
                    { [ReceiveMessageTransformer.ReceivingPModeKey] = null });

            var msg = new ReceivedMessage(
                AS4Message.Empty.ToStream(), 
                Constants.ContentTypes.Soap);

            // Act
            MessagingContext result = await sut.TransformAsync(msg);

            // Assert
            bool expectedNotConfiguredPMode = result.ReceivingPMode == null;
            bool expectedConfiguredPMode = result.ReceivingPMode?.Id == id;
            Assert.True(expectedNotConfiguredPMode || expectedConfiguredPMode);
        }
    }
}
