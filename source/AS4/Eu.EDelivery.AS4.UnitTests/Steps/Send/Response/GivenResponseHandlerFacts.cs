using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
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
                StepResult actualResult = await handler.HandleResponse(expectedResponse);

                // Assert
                Assert.Equal(expectedResponse.ResultedMessage, actualResult.InternalMessage);
            }
        }

        public class GivenEmptyResponseHandlerFacts
        {
            [Fact]
            public async Task ThenHandlerReturnsSameResultedMessage_IfStatusIsAccepted()
            {
                // Arrange
                IAS4Response as4Response = CreateAS4ResponseWithStatus(HttpStatusCode.Accepted);
                var handler = new EmptyBodyResponseHandler(CreateAnonymousNextHandler());

                // Act
                StepResult actualResult = await handler.HandleResponse(as4Response);

                // Assert
                InternalMessage expectedMessage = as4Response.OriginalRequest;
                InternalMessage actualMessage = actualResult.InternalMessage;

                Assert.Equal(expectedMessage, actualMessage);
                Assert.Empty(actualMessage.AS4Message.SignalMessages);
            }

            private static IAS4Response CreateAS4ResponseWithStatus(HttpStatusCode statusCode)
            {
                var stubAS4Response = new Mock<IAS4Response>();
                stubAS4Response.Setup(r => r.StatusCode).Returns(statusCode);
                stubAS4Response.Setup(r => r.OriginalRequest).Returns(new InternalMessage());

                return stubAS4Response.Object;
            }

            [Fact]
            public async Task ThenNextHandlerGetsTheResponse_IfStatusIsNotAccepted()
            {
                // Arrange
                IAS4Response as4Response = CreateAnonymousAS4Response();
                var spyHandler = new SpyAS4ResponseHandler();
                var handler = new EmptyBodyResponseHandler(spyHandler);

                // Act
                await handler.HandleResponse(as4Response);

                // Assert
                Assert.True(spyHandler.IsCalled);
            }
        }

        public class GivenPullRequestResponseHandlerFacts
        {
            [Fact]
            public async Task ThenNextHandlerGetsResponse_IfNotOriginatedFromPullRequest()
            {
                // Arrange
                IAS4Response as4Response = CreateAnonymousAS4Response();

                var spyHandler = new SpyAS4ResponseHandler();
                var handler = new PullRequestResponseHandler(spyHandler);
               
                // Act
                await handler.HandleResponse(as4Response);

                // Assert
                Assert.True(spyHandler.IsCalled);
            }

            [Fact]
            public async Task ThenHandlerReturnsStoppedExecutionStepResult()
            {
                // Arrange
                IAS4Response stubAS4Response = CreateResponseWith(request: new PullRequest(), response: new PullRequestError());
                var handler = new PullRequestResponseHandler(CreateAnonymousNextHandler());

                // Act
                StepResult actualResult = await handler.HandleResponse(stubAS4Response);

                // Assert
                Assert.False(actualResult.CanProceed);
            }

            private static IAS4Response CreateResponseWith(SignalMessage request, SignalMessage response)
            {
                var stubAS4Response = new Mock<IAS4Response>();
                stubAS4Response.Setup(r => r.OriginalRequest).Returns(new InternalMessageBuilder().WithSignalMessage(request).Build());
                stubAS4Response.Setup(r => r.ResultedMessage).Returns(new InternalMessageBuilder().WithSignalMessage(response).Build());

                return stubAS4Response.Object;
            }
        }

        private static IAS4ResponseHandler CreateAnonymousNextHandler()
        {
            return new StubAS4ResponseHandler();
        }

        private static AS4Response CreateAnonymousAS4Response()
        {
            return AS4Response.Create(
                requestMessage: new InternalMessage(), 
                webResponse: new Mock<HttpWebResponse>().Object, 
                cancellation: CancellationToken.None).Result;
        }
    }
}
