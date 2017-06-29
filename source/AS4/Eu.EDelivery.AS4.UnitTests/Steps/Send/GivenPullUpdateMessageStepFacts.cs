using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    public class GivenPullUpdateMessageStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task UpdateMessageStatusToSent()
        {
            // Assert
            string id = InsertOutMessage();
            AS4Message as4Message = AS4Message.Create(new FilledUserMessage(id));

            // Act
            await ExerciseUpdatingMessageStatus(as4Message);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                id,
                m =>
                {
                    Assert.Equal(OutStatus.Sent, m.Status);
                    Assert.Equal(Operation.Sent, m.Operation);
                });
        }

        private string InsertOutMessage()
        {
            string id = Guid.NewGuid().ToString();

            GetDataStoreContext.InsertOutMessage(
                new OutMessage { EbmsMessageId = id, Status = OutStatus.Submitted, Operation = Operation.Sending });

            return id;
        }

        private async Task ExerciseUpdatingMessageStatus(AS4Message as4Message)
        {
            var sut = new PullUpdateMessageStatusStep(GetDataStoreContext);

            await sut.ExecuteAsync(new MessagingContext(as4Message, MessagingContextMode.Send), CancellationToken.None);
        }
    }
}
