using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model.Core;
using Microsoft.EntityFrameworkCore;
using SimpleHttpMock;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SendAS4MessageStep"/>
    /// </summary>
    public class GivenSendAS4MessageStepFacts
    {
        private static readonly string SharedUrl = $"http://localhost:{new Random().Next(0, 9999)}";
        private readonly IStep _step;

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenSendAS4MessageStepFacts"/> class.
        /// </summary>
        public GivenSendAS4MessageStepFacts()
        {
            _step = new SendAS4MessageStep();

            IdentifierFactory.Instance.SetContext(StubConfig.Instance);

            DbContextOptions<DatastoreContext> options =
                new DbContextOptionsBuilder<DatastoreContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            Registry.Instance.CreateDatastoreContext = () => new DatastoreContext(options);
        }

        [Fact(Skip = "Test needs to run in elevated mode")]
        public async Task SendReturnsStopExecutionResult()
        {
            // Arrange
            AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(new PullRequestError()).Build();

            using (CreateMockedHttpServerThatReturns(as4Message))
            {
                InternalMessage dummyMessage = CreateAnonymousMessage();

                // Act
                StepResult actualResult = await _step.ExecuteAsync(dummyMessage, CancellationToken.None);

                // Assert
                Assert.False(actualResult.CanProceed);
            }
        }

        private static MockedHttpServer CreateMockedHttpServerThatReturns(AS4Message as4Message)
        {
            var builder = new MockedHttpServerBuilder();

            builder.WhenPost(SharedUrl)
                   .RespondContent(HttpStatusCode.OK, request => HttpContentFor(as4Message));

            return builder.Build(SharedUrl);
        }

        private static HttpContent HttpContentFor(AS4Message message)
        {
            using (var messageStream = new MemoryStream())
            {
                new SoapEnvelopeSerializer().Serialize(message, messageStream, CancellationToken.None);
                messageStream.Position = 0;
                byte[] messageBytes = messageStream.ToArray();

                return new StringContent(
                    Encoding.UTF8.GetString(messageBytes),
                    Encoding.UTF8,
                    Constants.ContentTypes.Soap);
            }
        }

        [Fact(Skip = "Test needs to run in elevated mode")]
        public async Task SendReturnsEmptyResponseForEmptyRequest()
        {
            // Arrange
            using (CreateMockedHttpServerThatReturnsStatusCode(HttpStatusCode.Accepted))
            {
                InternalMessage dummyMessage = CreateAnonymousMessage();

                // Act
                StepResult actualResult = await _step.ExecuteAsync(dummyMessage, CancellationToken.None);

                // Assert
                Assert.True(actualResult.InternalMessage.AS4Message.IsEmpty);
            }
        }

        private static MockedHttpServer CreateMockedHttpServerThatReturnsStatusCode(HttpStatusCode statusCode)
        {
            var builder = new MockedHttpServerBuilder();

            builder.WhenPost(SharedUrl)
                .Respond(statusCode);

            return builder.Build(SharedUrl);
        }

        private static InternalMessage CreateAnonymousMessage()
        {
            var builder = new AS4MessageBuilder();

            builder.WithSendingPMode(CreateValidSendingPMode()).WithSignalMessage(new PullRequest());

            return new InternalMessage(builder.Build());
        }

        private static SendingProcessingMode CreateValidSendingPMode()
        {
            return new SendingProcessingMode {PullConfiguration = new PullConfiguration {Protocol = {Url = SharedUrl}}};
        }
    }
}