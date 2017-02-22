using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
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
            base.Step = new SendUpdateDataStoreStep();
        }

        [Fact(Skip="This test should be reviewed.")]        
        public async Task ThenExecuteStepUpdateAsSentAsync()
        {
            // Arrange
            SignalMessage signalMessage = base.GetReceipt();
            InternalMessage internalMessage = new InternalMessageBuilder()
                .WithSignalMessage(signalMessage).Build();
            // Act
            await base.Step.ExecuteAsync(internalMessage, CancellationToken.None);

            // Assert
            await base.AssertOutMessages(signalMessage, base.Options, OutStatus.Sent);
        }
    }
}