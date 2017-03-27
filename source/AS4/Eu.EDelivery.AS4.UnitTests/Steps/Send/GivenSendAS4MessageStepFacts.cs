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
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenSendAS4MessageStepFacts"/> class.
        /// </summary>
        public GivenSendAS4MessageStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);

            DbContextOptions<DatastoreContext> options =
                new DbContextOptionsBuilder<DatastoreContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            Registry.Instance.CreateDatastoreContext = () => new DatastoreContext(options);
        }

        [Fact]
        public async Task SendReturnsStopExecutionResult()
        {
            // Arrange
            AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(new PullRequestError()).Build();

            using (CreateMockedHttpServerWithResponse(as4Message))
            {
                InternalMessage internalMessage = CreateValidAS4Message();
                var step = new SendAS4MessageStep();

                // Act
                StepResult actualResult = await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.False(actualResult.CanExecute);
            }
        }

        private static MockedHttpServer CreateMockedHttpServerWithResponse(AS4Message message)
        {
            const string sharedUrl = "http://localhost:1122";
            MockedHttpServerBuilder builder;

            using (var messageStream = new MemoryStream())
            {
                new SoapEnvelopeSerializer().Serialize(message, messageStream, CancellationToken.None);
                messageStream.Position = 0;
                byte[] messageBytes = messageStream.ToArray();

                builder = new MockedHttpServerBuilder();
                builder.WhenPost(sharedUrl)
                       .RespondContent(
                           HttpStatusCode.OK,
                           request =>
                               new StringContent(
                                   Encoding.UTF8.GetString(messageBytes),
                                   Encoding.UTF8,
                                   Constants.ContentTypes.Soap));
            }

            return builder.Build(sharedUrl);
        }

        private static InternalMessage CreateValidAS4Message()
        {
            var builder = new AS4MessageBuilder();

            builder.WithSendingPMode(CreateValidSendingPMode()).WithSignalMessage(new PullRequest());

            return new InternalMessage(builder.Build());
        }

        private static SendingProcessingMode CreateValidSendingPMode()
        {
            return new SendingProcessingMode {PushConfiguration = {Protocol = {Url = "http://localhost:1122/"}}};
        }
    }
}