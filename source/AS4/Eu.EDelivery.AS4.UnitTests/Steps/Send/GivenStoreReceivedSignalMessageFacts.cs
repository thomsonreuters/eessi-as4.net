using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SaveReceivedMessageStep" />
    /// </summary>
    public class GivenStoreReceivedSignalMessageFacts : GivenDatastoreStepFacts
    {
        public GivenStoreReceivedSignalMessageFacts()
        {
            Step = new SaveReceivedMessageStep(GetDataStoreContext, StubMessageBodyStore.Default);
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        [Fact(Skip="StoreReceivedSignalMessage will be replaced by SaveReceivedMessageStep")]
        public async Task ThenExecuteStepSucceedsAsync()
        {
            // Arrange
            var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);

            // Act
            StepResult result = await Step.ExecuteAsync(context, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}