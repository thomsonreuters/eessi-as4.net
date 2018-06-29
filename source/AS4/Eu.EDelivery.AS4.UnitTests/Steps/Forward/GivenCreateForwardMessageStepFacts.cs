using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Forward;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;
using MessageExchangePattern = Eu.EDelivery.AS4.Model.PMode.MessageExchangePattern;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Forward
{
    public class GivenCreateForwardMessageStepFacts : GivenDatastoreFacts
    {
        protected readonly InMemoryMessageBodyStore Store = new InMemoryMessageBodyStore();

        public class GivenValidMessagingContext : GivenCreateForwardMessageStepFacts
        {
            [Fact]
            public async Task ThenMessageIsForwarded()
            {
                // Arrange
                InMessage receivedInMessage = InPersistentUserMessage();
                await InsertInMessageIntoDatastore(receivedInMessage);

                MessagingContext messagingContext = ContextWithReferencedToBeForwardMessage();

                // Act
                await ExerciseCreateForwardMessage(messagingContext);

                // Assert
                // Verify if there exists a correct OutMessage record.
                using (DatastoreContext db = GetDataStoreContext())
                {
                    OutMessage outMessage = db.OutMessages.First(m => m.EbmsMessageId == receivedInMessage.EbmsMessageId);

                    Assert.Equal(Operation.ToBeProcessed, outMessage.Operation);
                    Assert.Equal(messagingContext.SendingPMode.MessagePackaging.Mpc, outMessage.Mpc);
                    Assert.Equal(messagingContext.SendingPMode.MepBinding.ToString(), outMessage.MEP.ToString());

                    InMessage inMessage = db.InMessages.First(m => m.EbmsMessageId == receivedInMessage.EbmsMessageId);

                    Assert.Equal(Operation.Forwarded, inMessage.Operation);
                }
            }

            private InMessage InPersistentUserMessage()
            {
                AS4Message as4Message = CreateAS4Message(new UserMessage
                {
                    MessageId = "some-message-id",
                    RefToMessageId = "ref-to-message-id",
                    Mpc = Constants.Namespaces.EbmsDefaultMpc
                });

                string location = Store.SaveAS4Message("", as4Message);

                InMessage receivedInMessage = CreateInMessage(as4Message);
                receivedInMessage.MessageLocation = location;

                return receivedInMessage;
            }

            private static AS4Message CreateAS4Message(UserMessage userMessage)
            {
                return AS4Message.Create(userMessage);
            }

            private static InMessage CreateInMessage(AS4Message message)
            {
                var result = new InMessage(message.GetPrimaryMessageId())
                {
                    EbmsRefToMessageId = message.PrimaryMessageUnit.RefToMessageId,
                    ContentType = message.ContentType,
                    Intermediary = true
                };

                result.EbmsMessageType = MessageType.UserMessage;
                result.Operation = Operation.ToBeForwarded;

                result.AssignAS4Properties(message.PrimaryMessageUnit);

                return result;
            }

            private MessagingContext ContextWithReferencedToBeForwardMessage()
            {
                ReceivedMessageEntityMessage receivedMessage;

                using (DatastoreContext db = GetDataStoreContext())
                {
                    InMessage inMessage =
                        db.InMessages.First(m => m.Operation == Operation.ToBeForwarded);

                    receivedMessage = new ReceivedMessageEntityMessage(inMessage, Stream.Null, "");
                }

                return new MessagingContext(receivedMessage, MessagingContextMode.Forward)
                {
                    SendingPMode = CreateSendingPMode()
                };
            }

            private static SendingProcessingMode CreateSendingPMode()
            {
                return new SendingProcessingMode
                {
                    Id = "forward-sending-pmode",
                    Mep = MessageExchangePattern.OneWay,
                    MepBinding = MessageExchangePatternBinding.Pull,
                    MessagePackaging = new SendMessagePackaging
                    {
                        Mpc = "Some-Modified-Mpc"
                    }
                };
            }

            private async Task InsertInMessageIntoDatastore(InMessage receivedInMessage)
            {
                using (DatastoreContext db = GetDataStoreContext())
                {
                    db.InMessages.Add(receivedInMessage);
                    await db.SaveChangesAsync();
                }
            }

            private async Task ExerciseCreateForwardMessage(MessagingContext messagingContext)
            {
                var sut = new CreateForwardMessageStep(StubConfig.Default, Store, GetDataStoreContext);

                await sut.ExecuteAsync(messagingContext);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void Disposing()
        {
            Store.Dispose();
        }
    }
}
