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
    public class GivenSetMessageToBeSentStepFacts : GivenDatastoreFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        [Fact]
        public async Task StepSetsMessageToBeSent()
        {
            // Assert
            string messageId = Guid.NewGuid().ToString(),
                   expected = Guid.NewGuid().ToString();

            var sut = new SetMessageToBeSentStep(GetDataStoreContext, _messageBodyStore);

            var messagingContext = SetupMessagingContext(messageId, Operation.Processing, expected);

            // Act
            await sut.ExecuteAsync(messagingContext, CancellationToken.None);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                messageId,
                m =>
                {
                    Assert.Equal(expected, m.MessageLocation);
                    Assert.Equal(Operation.ToBeSent, OperationUtils.Parse(m.Operation));
                });
        }

        private MessagingContext SetupMessagingContext(string ebmsMessageId, Operation operation, string messageLocation)
        {
            var outMessage = new OutMessage(ebmsMessageId: ebmsMessageId)
            {
                MessageLocation = messageLocation
            };

            outMessage.SetOperation(operation);

            var insertedOutMessage = GetDataStoreContext.InsertOutMessage(outMessage, withReceptionAwareness: false);

            Assert.NotEqual(default(long), insertedOutMessage.Id);

            var receivedMessage = new ReceivedEntityMessage(insertedOutMessage);

            MessagingContext context = new MessagingContext(receivedMessage, MessagingContextMode.Send);

            context.ModifyContext(AS4Message.Create(new FilledUserMessage(ebmsMessageId)));

            return context;
        }

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }
    }
}
