using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.ReceptionAwareness
{
    /// <summary>
    /// Describes how the AS4 message has to be behave in a Reception Awareness scenario
    /// </summary>
    public class ReceptionAwarenessUpdateDatastoreStep : IStep
    {
        private readonly IDatastoreRepository _repository;
        private readonly IInMessageService _service;
        private readonly ILogger _logger;

        private Entities.ReceptionAwareness _receptionAwareness;
        private UserMessage _userMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessUpdateDatastoreStep"/> class
        /// </summary>
        public ReceptionAwarenessUpdateDatastoreStep()
        {
            this._repository = Registry.Instance.DatastoreRepository;
            this._service = new InMessageService(this._repository);
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessUpdateDatastoreStep"/> class
        /// Create a <see cref="IStep"/> implementation  for the AS4 Reception Awareness
        /// </summary>
        /// <param name="repository">
        /// </param>
        /// <param name="service">
        /// </param>
        public ReceptionAwarenessUpdateDatastoreStep(IDatastoreRepository repository, IInMessageService service)
        {
            this._repository = repository;
            this._service = service;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._receptionAwareness = internalMessage.ReceiptionAwareness;

            if (IsMessageAlreadyAwnsered())
            {
                UpdateForAnsweredMessage();
                return StepResult.SuccessAsync(internalMessage);
            }

            if (MessageNeedsToBeResend())
            {
                UpdateForResendMessage();
            }
            else
            {
                WaitRetryInterval("Waiting ....");
                if (IsMessageUnawnserd())
                {
                    UpdateForUnawnseredMessage(cancellationToken);
                }
            }

            WaitRetryInterval("Waiting retry interval...");
            return StepResult.SuccessAsync(internalMessage);
        }

        private bool IsMessageAlreadyAwnsered()
        {
            string messageId = this._receptionAwareness.InternalMessageId;

            return this._repository.GetInMessage(
                inMessage => inMessage.EbmsRefToMessageId?.Equals(messageId) == true) != null;
        }

        private void UpdateForAnsweredMessage()
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            this._logger.Info($"[{messageId}] Reception Awareness completed");
            UpdateReceptionAwareness(x => x.IsCompleted = true);
        }

        private bool MessageNeedsToBeResend()
        {
            TimeSpan retryInterval = TimeSpan.Parse(this._receptionAwareness.RetryInterval);
            DateTimeOffset lastSendTime = this._receptionAwareness.LastSendTime.Add(retryInterval);

            return
                this._receptionAwareness.CurrentRetryCount < this._receptionAwareness.TotalRetryCount &&
                DateTimeOffset.UtcNow.CompareTo(lastSendTime) > 0 &&
                this._receptionAwareness.IsCompleted == false;
        }

        private void UpdateForResendMessage()
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            this._logger.Info($"[{messageId}] Update datastore so the ebMS message can be resend");
            this._repository.UpdateOutMessageAsync(messageId, x => x.Operation = Operation.ToBeSent);

            UpdateReceptionAwareness(awareness =>
            {
                awareness.CurrentRetryCount = awareness.CurrentRetryCount + 1;
                awareness.LastSendTime = DateTimeOffset.UtcNow;
            });
        }

        //private static readonly TimeSpan UnansweredGracePeriod = new TimeSpan(0, 0, 1, 0);

        private bool IsMessageUnawnserd()
        {
            return
                this._receptionAwareness.CurrentRetryCount ==
                this._receptionAwareness.TotalRetryCount;
            //      DateTime.Now - this._receptionAwareness.LastSendTime > UnansweredGracePeriod;

        }

        private void UpdateForUnawnseredMessage(CancellationToken cancellationToken)
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            this._logger.Info($"[{messageId}] ebMS message is unawsnered");

            UpdateReceptionAwareness(awareness => awareness.IsCompleted = true);

            Error errorMessage = CreateError();
            AS4Message as4Message = CreateAS4Message(errorMessage);
            this._service.InsertErrorAsync(errorMessage, as4Message, cancellationToken);
        }

        private Error CreateError()
        {
            AS4Exception as4Exception = CreateAS4Exception();
            string messageId = this._receptionAwareness.InternalMessageId;

            OutMessage outMessage = this._repository.GetOutMessageById(messageId);

            using (var memoryStream = new MemoryStream(outMessage.MessageBody))
            {
                ISerializer serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
                this._userMessage = serializer.DeserializeAsync(memoryStream, outMessage.ContentType, CancellationToken.None).Result.PrimaryUserMessage;
            }

            return new ErrorBuilder()
                .WithRefToEbmsMessageId(messageId)
                .WithAS4Exception(as4Exception)
                .Build();
        }

        private AS4Exception CreateAS4Exception()
        {
            string messageId = this._receptionAwareness.InternalMessageId;

            return new AS4ExceptionBuilder()
                .WithDescription($"[{messageId}] Missing Receipt")
                .WithMessageIds(this._receptionAwareness.InternalMessageId)
                .WithErrorCode(ErrorCode.Ebms0301)
                .Build();
        }

        private AS4Message CreateAS4Message(SignalMessage errorMessage)
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            OutMessage outMessage = this._repository.GetOutMessageById(messageId);
            var pmode = AS4XmlSerializer.Deserialize<SendingProcessingMode>(outMessage.PMode);

            return new AS4MessageBuilder()
                .WithSendingPMode(pmode)
                .WithSignalMessage(errorMessage)
                .WithUserMessage(this._userMessage)
                .Build();
        }

        private void UpdateReceptionAwareness(Action<Entities.ReceptionAwareness> updateAction)
        {
            string messageId = this._receptionAwareness.InternalMessageId;
            this._repository.UpdateReceptionAwarenessAsync(messageId, updateAction);
        }

        private void WaitRetryInterval(string description)
        {
            TimeSpan retryInterval = TimeSpan.Parse(this._receptionAwareness.RetryInterval);
            string messageId = this._receptionAwareness.InternalMessageId;

            this._logger.Info($"[{messageId}] {description}");
            // TODO: modify code below to await Task.Delay( ... ) ?
            Thread.Sleep(retryInterval);
        }
    }
}