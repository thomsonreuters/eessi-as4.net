using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send.Response;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model.Core;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send.Response
{
    /// <summary>
    /// Testing <see cref="TailResponseHandler"/>
    /// </summary>
    public class GivenResponseHandlerFacts
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenResponseHandlerFacts"/> class.
        /// </summary>
        public GivenResponseHandlerFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenTailResponseHandlerFacts
        {
            [Fact]
            public async Task ThenHandlerReturnsFixedValue()
            {
                // Arrange
                AS4Response expectedResponse = CreateAnonymousAS4Response();

                var handler = new TailResponseHandler();

                // Act
                StepResult actualResult = await handler.HandleResponse(
                    expectedResponse,
                    CreateAnonymousNextHandler());

                // Assert
                Assert.Equal(expectedResponse.ResultedMessage, actualResult.InternalMessage);
            }
        }

        public class GivenEmptyResponseHandlerFacts : IAS4ResponseHandler
        {
            [Fact]
            public async Task ThenHandlerReturnsSameResultedMessage_IfStatusIsAccepted()
            {
                // Arrange
                IAS4Response as4Response = CreateAS4ResponseWithStatus(HttpStatusCode.Accepted);
                var handler = new EmptyBodyResponseHandler();

                // Act
                StepResult actualResult = await handler.HandleResponse(as4Response, CreateAnonymousNextHandler());

                // Assert
                InternalMessage expectedMessage = as4Response.ResultedMessage;
                InternalMessage actualMessage = actualResult.InternalMessage;

                Assert.Equal(expectedMessage, actualMessage);
                Assert.Empty(actualMessage.AS4Message.SignalMessages);
            }

            private static IAS4Response CreateAS4ResponseWithStatus(HttpStatusCode statusCode)
            {
                var stubAS4Response = new Mock<IAS4Response>();
                stubAS4Response.Setup(r => r.StatusCode).Returns(statusCode);
                stubAS4Response.Setup(r => r.ResultedMessage).Returns(new InternalMessage());

                return stubAS4Response.Object;
            }

            [Fact]
            public async Task ThenNextHandlerGetsTheResponse_IfStatusIsNotAccepted()
            {
                // Arrange
                IAS4Response as4Response = CreateAnonymousAS4Response();
                var handler = new EmptyBodyResponseHandler();

                // Act
                await handler.HandleResponse(as4Response, this);
            }

            /// <summary>
            /// Handle the given <paramref name="response"/>, but delegate to the <paramref name="nextHandler"/> if you can't.
            /// </summary>
            /// <param name="response"></param>
            /// <param name="nextHandler"></param>
            /// <returns></returns>
            public Task<StepResult> HandleResponse(IAS4Response response, IAS4ResponseHandler nextHandler)
            {
                Assert.IsType<TailResponseHandler>(nextHandler);

                return StepResult.SuccessAsync(response.ResultedMessage);
            }
        }

        public class GivenPullRequestResponseHandlerFacts : IAS4ResponseHandler
        {
            [Fact]
            public async Task ThenNextHandlerGetsResponse_IfNotOriginatedFromPullRequest()
            {
                // Arrange
                IAS4Response as4Response = CreateAnonymousAS4Response();
                ISerializer stubSerializer = CreateStubSerializerThatReturns(new Receipt());
                var handler = new PullRequestResponseHandler(CreatedStubProviderFor(stubSerializer));
               

                // Act
                await handler.HandleResponse(as4Response, this);
            }

            public Task<StepResult> HandleResponse(IAS4Response response, IAS4ResponseHandler nextHandler)
            {
                Assert.Null(nextHandler);

                return StepResult.SuccessAsync(response.ResultedMessage);
            }

            [Fact]
            public async Task ThenHandlerReturnsStoppedExecutionStepResult()
            {
                // Arrange
                IAS4Response stubAS4Response = CreateResponseWith(new PullRequest());
                ISerializer stubSerializer = CreateStubSerializerThatReturns(new PullRequestError());
                var handler = new PullRequestResponseHandler(CreatedStubProviderFor(stubSerializer));

                // Act
                StepResult actualResult = await handler.HandleResponse(stubAS4Response, CreateAnonymousNextHandler());

                // Assert
                Assert.False(actualResult.CanProceed);
            }

            private static ISerializerProvider CreatedStubProviderFor(ISerializer stubbedSerializer)
            {
                var stubProvider = new Mock<ISerializerProvider>();
                stubProvider.Setup(p => p.Get(It.IsAny<string>())).Returns(stubbedSerializer);

                return stubProvider.Object;
            }

            private static ISerializer CreateStubSerializerThatReturns(SignalMessage signalMessage)
            {
                AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(signalMessage).Build();
                var stubbedSerializer = new Mock<ISerializer>();

                stubbedSerializer.Setup(
                    s =>
                        s.DeserializeAsync(
                            It.IsAny<Stream>(),
                            It.IsAny<string>(),
                            It.IsAny<CancellationToken>())).ReturnsAsync(as4Message);

                return stubbedSerializer.Object;
            }

            private static IAS4Response CreateResponseWith(SignalMessage signalMessage)
            {
                InternalMessage pullRequestMessage = new InternalMessageBuilder().WithSignalMessage(signalMessage).Build();
                var stubAS4Response = new Mock<IAS4Response>();
                stubAS4Response.Setup(r => r.ResultedMessage).Returns(pullRequestMessage);

                return stubAS4Response.Object;
            }
        }

        private static IAS4ResponseHandler CreateAnonymousNextHandler()
        {
            return new Mock<IAS4ResponseHandler>().Object;
        }

        private static AS4Response CreateAnonymousAS4Response()
        {
            return new AS4Response(new Mock<HttpWebResponse>().Object, new InternalMessage(), CancellationToken.None);
        }
    }
}
