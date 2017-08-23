using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                var as4Message = CreateAS4Message(new UserMessage()
                {
                    MessageId = "some-message-id",
                    RefToMessageId = "ref-to-message-id",
                    Mpc = Constants.Namespaces.EbmsDefaultMpc
                });

                var location = await Store.SaveAS4MessageAsync("", as4Message, CancellationToken.None);

                var receivedInMessage = CreateInMessage(as4Message);
                receivedInMessage.MessageLocation = location;

                using (var db = GetDataStoreContext())
                {
                    db.InMessages.Add(receivedInMessage);
                    await db.SaveChangesAsync();
                }

                var messagingContext = SetupMessagingContext();

                var sut = new CreateForwardMessageStep(StubConfig.Default, Store, GetDataStoreContext);

                await sut.ExecuteAsync(messagingContext, CancellationToken.None);

                // Verify if there exists a correct OutMessage record.
                using (var db = GetDataStoreContext())
                {
                    var outMessage = db.OutMessages.First(m => m.EbmsMessageId == receivedInMessage.EbmsMessageId);

                    Assert.NotNull(outMessage);
                    Assert.Equal(Operation.ToBeSent, outMessage.Operation);
                    Assert.Equal(messagingContext.SendingPMode.MessagePackaging.Mpc, outMessage.Mpc);
                    Assert.Equal(messagingContext.SendingPMode.MepBinding.ToString(), outMessage.MEP.ToString());

                    var inMessage = db.InMessages.First(m => m.EbmsMessageId == receivedInMessage.EbmsMessageId);

                    Assert.NotNull(inMessage);
                    Assert.Equal(Operation.Forwarded, inMessage.Operation);
                }
            }
           
            private MessagingContext SetupMessagingContext()
            {
                ReceivedMessageEntityMessage receivedMessage;

                using (var db = GetDataStoreContext())
                {
                    var inMessage = db.InMessages.First(m => m.Operation == Operation.ToBeForwarded);

                    receivedMessage = new ReceivedMessageEntityMessage(inMessage, Stream.Null, "");
                }

                var context = new MessagingContext(receivedMessage, MessagingContextMode.Forward)
                {
                    SendingPMode = CreateSendingPMode()
                };

                return context;
            }

            private static SendingProcessingMode CreateSendingPMode()
            {
                var pmode = new SendingProcessingMode()
                {
                    Id = "forward-sending-pmode",
                    Mep = MessageExchangePattern.OneWay,
                    MepBinding = MessageExchangePatternBinding.Pull,
                    MessagePackaging = new SendMessagePackaging()
                    {
                        Mpc = "Some-Modified-Mpc"
                    }
                };

                return pmode;
            }

            private static AS4Message CreateAS4Message(UserMessage userMessage)
            {
                return AS4Message.Create(userMessage);
            }

            private static InMessage CreateInMessage(AS4Message message)
            {
                var primaryMessageUnit = ((MessageUnit)message.PrimaryUserMessage ?? message.PrimarySignalMessage);

                var result = new InMessage
                {
                    EbmsMessageId = message.GetPrimaryMessageId(),
                    EbmsRefToMessageId = primaryMessageUnit.RefToMessageId,
                    ContentType = message.ContentType,
                    EbmsMessageType = MessageType.UserMessage,
                    Intermediary = true,
                    Operation = Operation.ToBeForwarded
                };

                result.AssignAS4Properties(primaryMessageUnit, CancellationToken.None);

                return result;
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        protected override void Disposing()
        {
            Store.Dispose();
        }
    }
}
