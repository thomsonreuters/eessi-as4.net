using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send.Response;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Eu.EDelivery.AS4.UnitTests.Model;
using Moq;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send.Response
{
    /// <summary>
    /// Testing <see cref="TailResponseHandler"/>
    /// </summary>
    public class GivenResponseHandlerFacts
    {
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
                Assert.Equal(expectedResponse.ReceivedStream, actualResult.MessagingContext.ReceivedMessage);
                AssertNoChangeInPModes(expectedResponse, actualResult);
            }
        }

        public class GivenEmptyResponseHandlerFacts : GivenResponseHandlerFacts
        {
            [Fact]
            public async Task ThenHandlerReturnsSameResultedMessage_IfStatusIsAccepted()
            {
                // Arrange
                IAS4Response as4Response = CreateEmptyAS4ResponseWithStatus(HttpStatusCode.Accepted);
                var handler = new EmptyBodyResponseHandler(CreateAnonymousNextHandler());

                // Act
                StepResult actualResult = await handler.HandleResponse(as4Response);

                // Assert               
                Assert.False(actualResult.CanProceed);
                AssertNoChangeInPModes(as4Response, actualResult);
            }

            [Fact]
            public async Task ThenCannotProceed_IfStatusIsErroneous()
            {
                // Arrange
                IAS4Response as4Response = CreateEmptyAS4ResponseWithStatus(HttpStatusCode.InternalServerError);
                var handler = new EmptyBodyResponseHandler(CreateAnonymousNextHandler());

                // Act
                StepResult actualResult = await handler.HandleResponse(as4Response);

                // Assert               
                Assert.False(actualResult.CanProceed);
                AssertNoChangeInPModes(as4Response, actualResult);
            }

            private static IAS4Response CreateEmptyAS4ResponseWithStatus(HttpStatusCode statusCode)
            {
                var stubAS4Response = new Mock<IAS4Response>();
                var context = new MessageContextBuilder()
                    .WithSendingPMode(new SendingProcessingMode())
                    .WithReceivingPMode(new ReceivingProcessingMode())
                    .Build();

                stubAS4Response.Setup(r => r.OriginalRequest).Returns(context);
                stubAS4Response.Setup(r => r.StatusCode).Returns(statusCode);
                stubAS4Response.Setup(r => r.ReceivedAS4Message).Returns(AS4Message.Empty);                

                return stubAS4Response.Object;
            }

            [Fact]
            public async Task ThenNextHandlerGetsTheResponse_IfAS4MessageIsReceived()
            {
                // Arrange
                AS4Message as4Message = AS4Message.Create(new Error($"user-{Guid.NewGuid()}"));
                IAS4Response as4Response = CreateAS4ResponseWithResultedMessage(as4Message);

                var spyHandler = new SpyAS4ResponseHandler();
                var handler = new EmptyBodyResponseHandler(spyHandler);

                // Act
                await handler.HandleResponse(as4Response);

                // Assert
                Assert.True(spyHandler.IsCalled);
            }

            private static IAS4Response CreateAS4ResponseWithResultedMessage(AS4Message resultedMessage)
            {
                var stubAS4Response = new Mock<IAS4Response>();
                var context = new MessageContextBuilder()
                    .WithSendingPMode(new SendingProcessingMode())
                    .WithReceivingPMode(new ReceivingProcessingMode())
                    .Build();

                stubAS4Response.Setup(r => r.OriginalRequest).Returns(context);
                stubAS4Response.Setup(r => r.ReceivedAS4Message).Returns(resultedMessage);

                return stubAS4Response.Object;
            }
        }

        public class GivenPullRequestResponseHandlerFacts : GivenResponseHandlerFacts
        {
            [Fact]
            public async Task ThenNextHandlerGetsResponse_IfNotOriginatedFromPullRequest()
            {
                // Arrange
                AS4Response as4Response = CreateAnonymousAS4Response();

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
                AssertNoChangeInPModes(stubAS4Response, actualResult);
            }

            private static async Task<IAS4Response> CreatePullRequestWarning()
            {
                var stubAS4Response = new Mock<IAS4Response>();

                MessagingContext pullRequest = new MessageContextBuilder()
                    .WithSignalMessage(new PullRequest(null))
                    .WithSendingPMode(new SendingProcessingMode())
                    .WithReceivingPMode(new ReceivingProcessingMode())
                    .Build();

                stubAS4Response.Setup(r => r.OriginalRequest).Returns(pullRequest);
                stubAS4Response.Setup(r => r.ReceivedAS4Message).Returns(await PullResponseWarning());

                return stubAS4Response.Object;
            }

            private static async Task<AS4Message> PullResponseWarning()
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(as4_pullrequest_warning)))
                {
                    var serializer = new SoapEnvelopeSerializer();
                    return await serializer.DeserializeAsync(stream, Constants.ContentTypes.Soap, CancellationToken.None);
                }
            }

            [Fact]
            public async Task ThenHandlerReturnsStoppedExecutionStepResult()
            {
                // Arrange
                IAS4Response stubAS4Response = CreateResponseWith(request: new PullRequest(null), response: new PullRequestError($"pull-{Guid.NewGuid()}"));
                var handler = new PullRequestResponseHandler(CreateAnonymousNextHandler());

                // Act
                StepResult actualResult = await handler.HandleResponse(stubAS4Response);

                // Assert
                Assert.False(actualResult.CanProceed);   
                AssertNoChangeInPModes(stubAS4Response, actualResult);
            }

            private static IAS4Response CreateResponseWith(SignalMessage request, SignalMessage response)
            {
                var stubAS4Response = new Mock<IAS4Response>();

                MessagingContext context = new MessageContextBuilder()
                    .WithSignalMessage(request)
                    .WithSendingPMode(new SendingProcessingMode())
                    .WithReceivingPMode(new ReceivingProcessingMode())
                    .Build();

                stubAS4Response.Setup(r => r.OriginalRequest).Returns(context);
                stubAS4Response.Setup(r => r.ReceivedAS4Message).Returns(AS4Message.Create(response));

                return stubAS4Response.Object;
            }
        }

        private static IAS4ResponseHandler CreateAnonymousNextHandler()
        {
            return new TailResponseHandler();
        }

        private static AS4Response CreateAnonymousAS4Response()
        {
            var stubResponse = new Mock<HttpWebResponse>();
            stubResponse.Setup(r => r.ContentType)
                        .Returns(Constants.ContentTypes.Soap);

            return AS4Response.Create(
                requestMessage: new EmptyMessagingContext
                {
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                },
                webResponse: stubResponse.Object).Result;
        }

        private static void AssertNoChangeInPModes(IAS4Response expected, StepResult actual)
        {
            Assert.Same(expected.OriginalRequest.SendingPMode, actual.MessagingContext.SendingPMode);
            Assert.Same(expected.OriginalRequest.ReceivingPMode, actual.MessagingContext.ReceivingPMode);
        }
    }
}
