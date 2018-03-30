using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Steps.Forward
{
    [Description("Creates a copy of the received message so that it can be forwarded.")]
    [Info("Creates a copy of the received message so that it can be forwarded.")]
    public class CreateForwardMessageStep : IStep
    {
        private readonly IConfig _configuration;
        private readonly IAS4MessageBodyStore _messageStore;
        private readonly Func<DatastoreContext> _createDataStoreContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateForwardMessageStep"/> class.
        /// </summary>
        public CreateForwardMessageStep()
            : this(Config.Instance, Registry.Instance.MessageBodyStore, Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateForwardMessageStep" /> class.
        /// </summary>
        /// <param name="configuration">The local configuration.</param>
        /// <param name="messageStore">The store where the datastore persist its messages.</param>
        /// <param name="createDatastoreContext">Create a new datastore context.</param>
        public CreateForwardMessageStep(IConfig configuration, IAS4MessageBodyStore messageStore, Func<DatastoreContext> createDatastoreContext)
        {
            _configuration = configuration;
            _messageStore = messageStore;
            _createDataStoreContext = createDatastoreContext;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            var receivedInMessage = (messagingContext.ReceivedMessage as ReceivedMessageEntityMessage)?.MessageEntity as InMessage;

            if (receivedInMessage == null)
            {
                throw new InvalidOperationException("The MessagingContext must contain a ReceivedMessage that represents an InMessage.");
            }

            // Forward message by creating an OutMessage and set operation to 'ToBeProcessed'.
            using (Stream originalInMessage =
                await _messageStore.LoadMessageBodyAsync(receivedInMessage.MessageLocation))
            {
                string outLocation = await _messageStore.SaveAS4MessageStreamAsync(
                    _configuration.OutMessageStoreLocation,
                    originalInMessage);

                originalInMessage.Position = 0;

                AS4Message msg =
                    await SerializerProvider.Default
                        .Get(receivedInMessage.ContentType)
                        .DeserializeAsync(originalInMessage, receivedInMessage.ContentType, CancellationToken.None);

                using (DatastoreContext dbContext = _createDataStoreContext())
                {
                    var repository = new DatastoreRepository(dbContext);

                    // Only create an OutMessage for the primary message-unit.
                    OutMessage outMessage = OutMessageBuilder
                        .ForMessageUnit(
                            GetPrimaryMessageUnit(msg),
                            receivedInMessage.ContentType,
                            messagingContext.SendingPMode)
                        .Build();

                    outMessage.Intermediary = true;
                    outMessage.IsDuplicate = receivedInMessage.IsDuplicate;
                    outMessage.MessageLocation = outLocation;
                    outMessage.Mpc = messagingContext.SendingPMode.MessagePackaging?.Mpc ?? Constants.Namespaces.EbmsDefaultMpc;
                    outMessage.SetOperation(Operation.ToBeProcessed);

                    repository.InsertOutMessage(outMessage);

                    // Set the InMessage to Forwarded.
                    // We do this for all InMessages that are present in this AS4 Message
                    repository.UpdateInMessages(
                        m => msg.MessageIds.Contains(m.EbmsMessageId),
                        r => r.SetOperation(Operation.Forwarded));

                    await dbContext.SaveChangesAsync();
                }
            }

            return StepResult.Success(messagingContext);
        }

        private static MessageUnit GetPrimaryMessageUnit(AS4Message as4Message)
        {
            if (as4Message.IsUserMessage)
            {
                return as4Message.PrimaryUserMessage;
            }
            return as4Message.PrimarySignalMessage;
        }
    }
}
