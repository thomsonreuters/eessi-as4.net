using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="DeliverUpdateDatastoreStep" />
    /// </summary>
    public class GivenDeliverUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task ThenExecuteMethodSucceedsWithValidUserMessageAsync()
        {
            // Arrange
            string id = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertInMessage(
                CreateInMessage(id, InStatus.Received, Operation.ToBeDelivered));
            
            DeliverMessageEnvelope envelope = AnonymousDeliverEnvelope(id);
            var sut = new DeliverUpdateDatastoreStep();

            // Act
            await sut.ExecuteAsync(new MessagingContext(envelope));

            // Assert
            GetDataStoreContext.AssertInMessage(id, inmessage =>
            {
                Assert.NotNull(inmessage);
                Assert.Equal(InStatus.Delivered, InStatusUtils.Parse(inmessage.Status));
                Assert.Equal(Operation.Delivered, OperationUtils.Parse(inmessage.Operation));
            });
        }

        private static DeliverMessageEnvelope AnonymousDeliverEnvelope(string id)
        {
            return new DeliverMessageEnvelope(
                messageInfo: new MessageInfo { MessageId = id }, 
                deliverMessage: new byte[] { }, 
                contentType: string.Empty);
        }

        private static InMessage CreateInMessage(string id, InStatus status, Operation operation)
        {
            var m = new InMessage(id);
            m.SetStatus(status);
            m.SetOperation(operation);

            return m;
        }
    }
}