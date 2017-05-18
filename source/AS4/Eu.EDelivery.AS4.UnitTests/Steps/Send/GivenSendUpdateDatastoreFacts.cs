using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
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
            Step = new SendUpdateDataStoreStep(GetDataStoreContext, StubMessageBodyPersister.Default);
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        private InternalMessage CreateReferencedInternalMessageWith(SignalMessage signalMessage)
        {
            return
                new InternalMessageBuilder().WithUserMessage(new UserMessage(ReceiptMessageId))
                                            .WithSignalMessage(signalMessage)
                                            .Build();
        }

        [Fact]
        public async Task ThenExecuteStepSucceedsAsync()
        {
            // Arrange
            AS4Message message = new AS4MessageBuilder().Build();
            var internalMessage = new InternalMessage(message);

            // Act
            StepResult result = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        private static InternalMessage CreateInternalMessageWith(SignalMessage signalMessage)
        {
            InternalMessage internalMessage = new InternalMessageBuilder(signalMessage.RefToMessageId)
                           .WithSignalMessage(signalMessage).Build();

            internalMessage.AS4Message.SendingPMode = new SendingProcessingMode();
            internalMessage.AS4Message.ReceivingPMode = new ReceivingProcessingMode();

            return internalMessage;
        }
    }
}