using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Transformers;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    public class GivenReceiveMessageTransformerFacts
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Wraps_Into_VirtualStream_If_Cant_Seek(bool canSeek)
        {
            // Arrange
            var stub = new Mock<Stream>();
            stub.SetupGet(str => str.CanSeek).Returns(canSeek);
            var sut = new ReceiveMessageTransformer();

            // Act
            MessagingContext result = await sut.TransformAsync(
                new ReceivedMessage(stub.Object, Constants.ContentTypes.Soap));

            // Assert
            Assert.True(
                result.ReceivedMessage.UnderlyingStream is VirtualStream != canSeek, 
                "Incoming stream isn't wrapped in 'VirtualStream'");
        }

        [Theory]
        [InlineData("existing-id", true)]
        [InlineData("none-existing-id", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public async Task Adds_ReceivePMode_When_PMode_Setting_Is_Defined(string id, bool contains)
        {
            // Arrange
            var stub = new Mock<IConfig>();
            stub.Setup(c => c.GetReceivingPModes())
                  .Returns(new[] { new ReceivingProcessingMode {Id = "existing-id"} });

            var sut = new ReceiveMessageTransformer(stub.Object);
            sut.Configure(
                new Dictionary<string, string>
                    { [ReceiveMessageTransformer.ReceivingPModeKey] = id });

            var msg = new ReceivedMessage(Stream.Null, Constants.ContentTypes.Mime);

            // Act
            MessagingContext result = await sut.TransformAsync(msg);

            // Assert
            bool hasId = result?.ReceivingPMode?.Id != null;
            Assert.True(contains == (hasId && result.ReceivingPMode.Id == id));
        }
    }
}
