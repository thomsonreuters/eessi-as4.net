using System;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Transformers;
using Moq;
using Xunit;
using Callback =
    System.Func
    <Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.InternalMessage>>;

namespace Eu.EDelivery.AS4.ServiceHandler.UnitTests.Agents
{
    /// <summary>
    /// Testing <see cref="Agent" />
    /// </summary>
    public class GivenAgentFacts
    {
        private readonly Mock<ITransformer> _transformer;
        private readonly Mock<IReceiver> _receiver;
        private readonly Mock<IStep> _transmitter;

        public GivenAgentFacts()
        {
            this._transformer = new Mock<ITransformer>();
            this._receiver = new Mock<IReceiver>();
            this._transmitter = new Mock<IStep>();
        }

        /// <summary>
        /// Testing if the Channel succeeds
        /// </summary>
        ////public class GivenAgentSuccess : GivenAgentFacts
        ////{
        ////    [Fact]
        ////    public void ThenChannelTransmitsSuccessfullyMessages()
        ////    {
        ////        // Arrange
        ////        var receiverMock = new Mock<IReceiver>();
        ////        SetupReceiverMock(receiverMock);
        ////        var stepMock = new Mock<IStep>();

        ////        var sut = new StubAgent(
        ////            receiverMock.Object, stepMock.Object, new SubmitMessageXmlTransformer());

        ////        // Act
        ////        sut.Start(CancellationToken.None);
        ////        Thread.Sleep(2000);

        ////        // Assert
        ////        stepMock.Verify(x => x.ExecuteAsync(
        ////            It.IsAny<InternalMessage>(), CancellationToken.None), Times.Never());
        ////    }

        ////    private void SetupReceiverMock(Mock<IReceiver> receiverMock)
        ////    {
        ////        receiverMock
        ////            .Setup(x => x.StartReceiving(It.IsAny<Callback>(), CancellationToken.None))
        ////            .Callback<Callback, CancellationToken>((callback, cancellationToken) 
        ////                => callback(new ReceivedMessage(new MemoryStream {Position = 0}), cancellationToken));
        ////    }

        ////    [Fact]
        ////    public void ThenOpeningAChannelStartsTheReceiver()
        ////    {
        ////        // Arrange
        ////        var receiverMock = new Mock<IReceiver>();
        ////        var transmitterMock = new Mock<IStep>();

        ////        var sut = new StubAgent(
        ////            receiverMock.Object, transmitterMock.Object, new SubmitMessageXmlTransformer());

        ////        // Act
        ////        sut.Start(CancellationToken.None);
        ////        // Assert
        ////        AssertVerifyCallingStartReceiving(receiverMock);
        ////    }

        ////    private void AssertVerifyCallingStartReceiving(Mock<IReceiver> receiverMock)
        ////    {
        ////        receiverMock.Verify(x => x.StartReceiving(
        ////            It.IsAny<Callback>(), CancellationToken.None), Times.Once());
        ////    }
        ////}

        /// <summary>
        /// Testing if the Channel fails
        /// </summary>
        ////public class GivenAgentFails : GivenAgentFacts
        ////{
        ////    /// <summary>
        ////    /// Helper method to create Channel
        ////    /// </summary>
        ////    /// <param name="receiver"></param>
        ////    /// <param name="transmitter"></param>
        ////    /// <param name="transformer"></param>
        ////    /// <returns></returns>
        ////    private Agent CreateStubAgent(
        ////        IReceiver receiver,
        ////        IStep transmitter,
        ////        ITransformer transformer)
        ////    {
        ////        return new StubAgent(receiver, transmitter, transformer);
        ////    }

        ////    [Fact]
        ////    public void ThenCreateChannelFails()
        ////    {
        ////        // Act / Assert
        ////        Assert.Throws<ArgumentNullException>(() =>
        ////                CreateStubAgent(null, this._transmitter.Object, this._transformer.Object));

        ////        Assert.Throws<ArgumentNullException>(() =>
        ////                CreateStubAgent(this._receiver.Object, null, this._transformer.Object));
        ////    }
        ////}
    }
}