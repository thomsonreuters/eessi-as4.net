using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
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
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Model.Core;
using Moq;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Extensions.AS4MessageExtensions;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

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

        public class GivenTailResponseHandlerFacts : GivenResponseHandlerFacts
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
                Assert.Equal(expectedResponse.ResultedMessage, actualResult.MessagingContext);
            }
        }

        public class GivenEmptyResponseHandlerFacts : GivenResponseHandlerFacts
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
                Assert.False(actualResult.CanProceed);
                MessagingContext expectedMessage = as4Response.OriginalRequest;
                MessagingContext actualMessage = actualResult.MessagingContext;
            }

            [Fact]
            public async Task ThenCannotProceed_IfStatusIsErroneous()
            {
                // Arrange
                IAS4Response as4Response = CreateAS4ResponseWithStatus(HttpStatusCode.InternalServerError);
                var handler = new EmptyBodyResponseHandler(CreateAnonymousNextHandler());

                // Act
                StepResult actualResult = await handler.HandleResponse(as4Response);

                // Assert               
                Assert.False(actualResult.CanProceed);
            }

            private static IAS4Response CreateAS4ResponseWithStatus(HttpStatusCode statusCode)
            {
                var stubAS4Response = new Mock<IAS4Response>();
                stubAS4Response.Setup(r => r.StatusCode).Returns(statusCode);
                stubAS4Response.Setup(r => r.ResultedMessage).Returns(new MessagingContext(EmptyAS4Message));
                stubAS4Response.Setup(r => r.OriginalRequest).Returns(new EmptyMessagingContext());

                return stubAS4Response.Object;
            }

            [Fact]
            public async Task ThenNextHandlerGetsTheResponse_IfAS4MessageIsReceived()
            {
                // Arrange
                AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(new Error()).Build();
                IAS4Response as4Response = CreateAS4ResponseWithResultedMessage(new MessagingContext(as4Message));

                var spyHandler = new SpyAS4ResponseHandler();
                var handler = new EmptyBodyResponseHandler(spyHandler);

                // Act
                await handler.HandleResponse(as4Response);

                // Assert
                Assert.True(spyHandler.IsCalled);
            }

            private static IAS4Response CreateAS4ResponseWithResultedMessage(MessagingContext resultedMessage)
            {
                var stubAS4Response = new Mock<IAS4Response>();
                stubAS4Response.Setup(r => r.ResultedMessage).Returns(resultedMessage);
                stubAS4Response.Setup(r => r.OriginalRequest).Returns(new EmptyMessagingContext());

                return stubAS4Response.Object;
            }
        }

        public class GivenPullRequestResponseHandlerFacts : GivenResponseHandlerFacts
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
            public async Task HandlerStopsExecution_IfResponseIsWarning()
            {
                // Arrange
                IAS4Response stubAS4Response = await CreatePullRequestWarning();
                var sut = new PullRequestResponseHandler(CreateAnonymousNextHandler());

                // Act
                StepResult actualResult = await sut.HandleResponse(stubAS4Response);

                // Assert
                Assert.False(actualResult.CanProceed);
            }

            private static async Task<IAS4Response> CreatePullRequestWarning()
            {
                var stubAS4Response = new Mock<IAS4Response>();

                MessagingContext pullRequest = new InternalMessageBuilder().WithSignalMessage(new PullRequest()).Build();
                stubAS4Response.Setup(r => r.OriginalRequest).Returns(pullRequest);

                AS4Message deserializedMessage = await DeserializePullRequestWarning();
                stubAS4Response.Setup(r => r.ResultedMessage).Returns(new MessagingContext(deserializedMessage));

                return stubAS4Response.Object;
            }

            private static async Task<AS4Message> DeserializePullRequestWarning()
            {
                AS4Message deserializedMessage;

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(as4_pullrequest_warning)))
                {
                    var serializer = new SoapEnvelopeSerializer();
                    deserializedMessage = await serializer.DeserializeAsync(stream, Constants.ContentTypes.Soap, CancellationToken.None);
                }

                return deserializedMessage;
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

                Func<SignalMessage, MessagingContext> buildContext =
                    m => new InternalMessageBuilder().WithSignalMessage(m).Build();

                stubAS4Response.Setup(r => r.OriginalRequest).Returns(buildContext(request));
                stubAS4Response.Setup(r => r.ResultedMessage).Returns(buildContext(response));

                return stubAS4Response.Object;
            }
        }

        private static IAS4ResponseHandler CreateAnonymousNextHandler()
        {
            return new TailResponseHandler();
        }

        private static AS4Response CreateAnonymousAS4Response()
        {           
            return AS4Response.Create(
                requestMessage: new EmptyMessagingContext(),
                webResponse: new Mock<HttpWebResponse>().Object, 
                cancellation: CancellationToken.None).Result;
        }
    }
}
