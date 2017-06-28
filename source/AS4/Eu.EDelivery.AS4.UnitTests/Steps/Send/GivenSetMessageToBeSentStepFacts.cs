using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    public class GivenSetMessageToBeSentStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task StepSetsMessageToBeSent()
        {
            // Assert
            string messageId = Guid.NewGuid().ToString(),
                expected = Guid.NewGuid().ToString();

            var sut = new SetMessageToBeSentStep(GetDataStoreContext, new StubMessageBodyStore(expected));

            InsertOutMessageWith(messageId, Operation.Processing, "not updated message location");

            // Act
            await ExerciseSetToBeSent(messageId, sut);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                messageId,
                m =>
                {
                    Assert.Equal(expected, m.MessageLocation);
                    Assert.Equal(Operation.ToBeSent, m.Operation);
                });

        }

        private static async Task ExerciseSetToBeSent(string id, IStep sut)
        {
            AS4Message as4Message = AS4Message.Create(new FilledUserMessage(id));

            await sut.ExecuteAsync(new MessagingContext(as4Message, MessagingContextMode.Send), CancellationToken.None);
        }

        private void InsertOutMessageWith(string id, Operation processing, string notUpdatedLocation)
        {
            var outMessage = new OutMessage
            {
                EbmsMessageId = id,
                Operation = processing,
                MessageLocation = notUpdatedLocation
            };

            GetDataStoreContext.InsertOutMessage(outMessage);
        }
    }
}
