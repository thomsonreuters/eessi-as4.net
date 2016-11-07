using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.Steps.Services;
using Eu.EDelivery.AS4.UnitTests.Builders;
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
        private IInMessageService _repository;

        public GivenSendUpdateDatastoreFacts()
        {
            var registry = new Registry();

            this._repository = new InMessageService(
                new DatastoreRepository(() => new DatastoreContext(base.Options)));

            base.Step = new SendUpdateDataStoreStep(this._repository);
        }

        [Fact]
        public async Task ThenExecuteStepUpdateAsSentAsync()
        {
            // Arrange
            SignalMessage signalMessage = base.GetSignalMessage();
            InternalMessage internalMessage = new InternalMessageBuilder()
                .WithSignalMessage(signalMessage).Build();
            // Act
            await base.Step.ExecuteAsync(internalMessage, CancellationToken.None);
            // Assert
            await base.AssertOutMessages(signalMessage, base.Options, OutStatus.Sent);
        }
    }
}