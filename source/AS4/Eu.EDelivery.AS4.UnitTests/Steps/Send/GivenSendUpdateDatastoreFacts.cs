using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SendUpdateDataStoreStep" />
    /// </summary>
    public class GivenSendUpdateDatastoreFacts : GivenDatastoreStepFacts
    {
        public GivenSendUpdateDatastoreFacts()
        {
            Step = new SendUpdateDataStoreStep(GetDataStoreContext, StubMessageBodyStore.Default);
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        [Fact]
        public async Task ThenExecuteStepSucceedsAsync()
        {
            // Arrange
            var internalMessage = new MessagingContext(AS4Message.Empty);

            // Act
            StepResult result = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}