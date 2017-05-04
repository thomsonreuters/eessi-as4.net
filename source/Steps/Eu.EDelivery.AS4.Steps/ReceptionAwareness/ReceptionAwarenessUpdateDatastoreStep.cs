using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.ReceptionAwareness
{
    /// <summary>
    /// Describes how the AS4 message has to be behave in a Reception Awareness scenario
    /// </summary>
    public class ReceptionAwarenessUpdateDatastoreStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IAS4MessageBodyPersister _inMessageBodyPersister;

        private Entities.ReceptionAwareness _receptionAwareness;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessUpdateDatastoreStep"/> class.
        /// </summary>
        public ReceptionAwarenessUpdateDatastoreStep() : this(Config.Instance.IncomingAS4MessageBodyPersister)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessUpdateDatastoreStep"/> class
        /// </summary>
        public ReceptionAwarenessUpdateDatastoreStep(IAS4MessageBodyPersister inMessageBodyPersister)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _inMessageBodyPersister = inMessageBodyPersister;
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="internalMessage"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                _logger.Debug("Executing ReceptionAwarenessDataStoreStep");

                var repository = new DatastoreRepository(context);

                _receptionAwareness = internalMessage.ReceptionAwareness;

                context.Attach(_receptionAwareness);

                if (IsMessageAlreadyAnswered(repository))
                {
                    _logger.Debug("Message has been answered, marking as complete");
                    UpdateForAnsweredMessage(repository);
                }
                else
                {
                    if (MessageNeedsToBeResend(repository))
                    {
                        _logger.Debug(
                            $"Updating message for resending.  RetryCount = {_receptionAwareness.CurrentRetryCount}");
                        UpdateForResendMessage(repository);
                    }
                    else
                    {
                        if (IsMessageUnanswered())
                        {
                            _logger.Debug("Message is unanswered.");
                            await UpdateForUnansweredMessage(repository, cancellationToken);
                        }
                        else
                        {
                            // In any other case, the Status should be reset to Pending.
                            repository.UpdateReceptionAwareness(_receptionAwareness.InternalMessageId, ra => ra.Status = ReceptionStatus.Pending);
                        }
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            WaitRetryInterval("Waiting retry interval...");

            return await StepResult.SuccessAsync(internalMessage);
        }

        private bool IsMessageAlreadyAnswered(IDatastoreRepository repository)
        {
            return repository.InMessageExists(m => m.EbmsRefToMessageId != null &&
                                                   m.EbmsRefToMessageId.Equals(_receptionAwareness.InternalMessageId));
        }

        private void UpdateForAnsweredMessage(IDatastoreRepository repository)
        {
            string messageId = _receptionAwareness.InternalMessageId;
            _logger.Info($"[{messageId}] Reception Awareness completed");
            UpdateReceptionAwareness(x => x.Status = ReceptionStatus.Completed, repository);
        }

        private bool MessageNeedsToBeResend(IDatastoreRepository repository)
        {
            TimeSpan retryInterval = TimeSpan.Parse(_receptionAwareness.RetryInterval);

            DateTimeOffset deadlineForResend = _receptionAwareness.LastSendTime.Add(retryInterval);

            return
                _receptionAwareness.CurrentRetryCount < _receptionAwareness.TotalRetryCount &&

                // Is it necessary that this is a repository method ?
                repository.GetOutMessageOperation(_receptionAwareness.InternalMessageId) != Operation.Sending &&
                DateTimeOffset.UtcNow.CompareTo(deadlineForResend) > 0 &&
                _receptionAwareness.Status != ReceptionStatus.Completed;
        }

        private void UpdateForResendMessage(IDatastoreRepository repository)
        {
            string messageId = _receptionAwareness.InternalMessageId;
            _logger.Info($"[{messageId}] Update datastore so the ebMS message can be resend. (RetryCount = {_receptionAwareness.CurrentRetryCount + 1})");

            repository.UpdateOutMessage(messageId, x => x.Operation = Operation.ToBeSent);
            repository.UpdateReceptionAwareness(messageId, ra => ra.Status = ReceptionStatus.Pending);
        }

        private bool IsMessageUnanswered()
        {
            return _receptionAwareness.CurrentRetryCount >= _receptionAwareness.TotalRetryCount;
        }

        private async Task UpdateForUnansweredMessage(IDatastoreRepository repository, CancellationToken cancellationToken)
        {
            string messageId = _receptionAwareness.InternalMessageId;
            _logger.Info($"[{messageId}] ebMS message is unanswered");

            UpdateReceptionAwareness(awareness => awareness.Status = ReceptionStatus.Completed, repository);
            repository.UpdateOutMessage(messageId, x => x.Operation = Operation.DeadLettered);

            Error errorMessage = CreateError();
            AS4Message as4Message = CreateAS4Message(errorMessage, repository);

            var inMessageService = new InMessageService(repository, _inMessageBodyPersister);
            await inMessageService.InsertAS4Message(as4Message, cancellationToken).ConfigureAwait(false);
        }

        private Error CreateError()
        {
            AS4Exception as4Exception = CreateAS4Exception();
            string messageId = _receptionAwareness.InternalMessageId;

            return new ErrorBuilder()
                .WithRefToEbmsMessageId(messageId)
                .WithAS4Exception(as4Exception)
                .Build();
        }

        private AS4Exception CreateAS4Exception()
        {
            string messageId = _receptionAwareness.InternalMessageId;

            return AS4ExceptionBuilder
                .WithDescription($"[{messageId}] Missing Receipt")
                .WithMessageIds(_receptionAwareness.InternalMessageId)
                .WithErrorCode(ErrorCode.Ebms0301)
                .Build();
        }

        private AS4Message CreateAS4Message(SignalMessage errorMessage, IDatastoreRepository repository)
        {
            string messageId = _receptionAwareness.InternalMessageId;

            var pmode = repository.RetrieveSendingPModeForOutMessage(messageId);

            var builder = new AS4MessageBuilder()
                .WithSendingPMode(pmode)
                .WithSignalMessage(errorMessage);

            return builder.Build();
        }

        private void UpdateReceptionAwareness(Action<Entities.ReceptionAwareness> updateAction, IDatastoreRepository repository)
        {
            string messageId = _receptionAwareness.InternalMessageId;
            repository.UpdateReceptionAwareness(messageId, updateAction);
        }

        private void WaitRetryInterval(string description)
        {
            TimeSpan retryInterval = TimeSpan.Parse(_receptionAwareness.RetryInterval);
            string messageId = _receptionAwareness.InternalMessageId;

            _logger.Info($"[{messageId}] {description}");
            Thread.Sleep(retryInterval);            
        }
    }
}