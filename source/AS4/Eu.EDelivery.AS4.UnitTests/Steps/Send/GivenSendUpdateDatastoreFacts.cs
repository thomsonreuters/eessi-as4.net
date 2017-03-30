using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Eu.EDelivery.AS4.UnitTests.Common;
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
            Step = new SendUpdateDataStoreStep();
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        [Fact]
        public async Task ThenExecuteStepUpdateAsSentAsync()
        {
            // Arrange
            SignalMessage signalMessage = CreateReceipt();
            InternalMessage internalMessage = CreateReferencedInternalMessageWith(signalMessage);

            // Act
            await Step.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            await AssertOutMessages(signalMessage, Options, OutStatus.Ack);
        }

        private InternalMessage CreateReferencedInternalMessageWith(SignalMessage signalMessage)
        {
            return
                new InternalMessageBuilder().WithUserMessage(new UserMessage(ReceiptMessageId))
                                            .WithSignalMessage(signalMessage)
                                            .Build();
        }
    }
}