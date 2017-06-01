using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Services
{
    public class ReceptionAwarenessService
    {
        private readonly IDatastoreRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessService" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public ReceptionAwarenessService(IDatastoreRepository repository)
        {
            _repository = repository;
        }

        public async Task DeadletterOutMessageAsync(string messageId, IAS4MessageBodyPersister messageBodyPersister, CancellationToken cancellationToken)
        {
            _repository.UpdateOutMessage(messageId, x => x.Operation = Operation.DeadLettered);

            SendingProcessingMode pmode = _repository.GetOutMessageData(
                messageId: messageId,
                selection: m => AS4XmlSerializer.FromString<SendingProcessingMode>(m.PMode));

            Error errorMessage = CreateError(messageId);

            AS4Message as4Message = CreateAS4Message(errorMessage, pmode);

            // We do not use the InMessageService to persist the incoming message here, since this is not really
            // an incoming message.  We create this InMessage in order to be able to notify the Message Producer
            // if he should be notified when a message cannot be sent.
            // (Maybe we should only create the InMessage when notification is enabled ?)

            string location = await messageBodyPersister.SaveAS4MessageAsync(as4Message, cancellationToken);

            InMessage inMessage = InMessageBuilder.ForSignalMessage(errorMessage, as4Message)
                                                  .WithPModeString(AS4XmlSerializer.ToString(as4Message.SendingPMode))
                                                  .Build(cancellationToken);
            inMessage.MessageLocation = location;

            inMessage.Operation = pmode.ErrorHandling.NotifyMessageProducer ? Operation.ToBeNotified : Operation.NotApplicable;

            _repository.InsertInMessage(inMessage);
        }

        private static Error CreateError(string messageId)
        {
            AS4Exception as4Exception = CreateAS4Exception(messageId);

            var error = new ErrorBuilder()
                .WithRefToEbmsMessageId(messageId)
                .WithAS4Exception(as4Exception)
                .Build();

            return error;
        }

        private static AS4Exception CreateAS4Exception(string messageId)
        {
            return AS4ExceptionBuilder
                .WithDescription($"[{messageId}] Missing Receipt")
                .WithMessageIds(messageId)
                .WithErrorCode(ErrorCode.Ebms0301)
                .Build();
        }

        private static AS4Message CreateAS4Message(SignalMessage errorMessage, SendingProcessingMode pmode)
        {
            var builder = new AS4MessageBuilder()
                .WithSendingPMode(pmode)
                .WithSignalMessage(errorMessage);

            return builder.Build();
        }

    }
}
