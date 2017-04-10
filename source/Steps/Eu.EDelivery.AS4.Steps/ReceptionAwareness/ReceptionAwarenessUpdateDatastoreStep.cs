using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.ReceptionAwareness
{
    /// <summary>
    /// Describes how the AS4 message has to be behave in a Reception Awareness scenario
    /// </summary>
    public class ReceptionAwarenessUpdateDatastoreStep : IStep
    {
        private readonly ILogger _logger;

        private Entities.ReceptionAwareness _receptionAwareness;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessUpdateDatastoreStep"/> class
        /// </summary>
        public ReceptionAwarenessUpdateDatastoreStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="internalMessage"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {

            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                _logger.Debug("Executing ReceptionAwarenessDataStoreStep");

                var repository = new DatastoreRepository(context);

                _receptionAwareness = internalMessage.ReceptionAwareness;

                if (IsMessageAlreadyAnswered(context))
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
                            UpdateForUnansweredMessage(repository, cancellationToken);
                        }
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
            }


            WaitRetryInterval("Waiting retry interval...");

            return await StepResult.SuccessAsync(internalMessage);
        }

        private bool IsMessageAlreadyAnswered(DatastoreContext context)
        {
            // TODO: only check for InMessages that are signalmessages (error / receipt) ?
            return context.InMessages.Any(m => m.EbmsRefToMessageId != null && m.EbmsRefToMessageId.Equals(_receptionAwareness.InternalMessageId));
        }

        private void UpdateForAnsweredMessage(IDatastoreRepository repository)
        {
            string messageId = _receptionAwareness.InternalMessageId;
            _logger.Info($"[{messageId}] Reception Awareness completed");
            UpdateReceptionAwareness(x => x.IsCompleted = true, repository);
        }

        private bool MessageNeedsToBeResend(IDatastoreRepository repository)
        {
            TimeSpan retryInterval = TimeSpan.Parse(_receptionAwareness.RetryInterval);

            TimeSpan gracePeriod = TimeSpan.FromTicks(retryInterval.Ticks);

            DateTimeOffset deadlineForResend = _receptionAwareness.LastSendTime.Add(gracePeriod);

            return
                _receptionAwareness.CurrentRetryCount < _receptionAwareness.TotalRetryCount &&
                repository.GetOutMessageById(_receptionAwareness.InternalMessageId)?.Operation != Operation.Sending &&
                DateTimeOffset.UtcNow.CompareTo(deadlineForResend) > 0 &&
                _receptionAwareness.IsCompleted == false;
        }

        private void UpdateForResendMessage(IDatastoreRepository repository)
        {
            string messageId = _receptionAwareness.InternalMessageId;
            _logger.Info($"[{messageId}] Update datastore so the ebMS message can be resend. (RetryCount = {_receptionAwareness.CurrentRetryCount + 1})");

            repository.UpdateOutMessage(messageId, x => x.Operation = Operation.ToBeSent);
        }

        private bool IsMessageUnanswered()
        {
            return _receptionAwareness.CurrentRetryCount >= _receptionAwareness.TotalRetryCount;
        }

        private void UpdateForUnansweredMessage(IDatastoreRepository repository, CancellationToken cancellationToken)
        {
            string messageId = _receptionAwareness.InternalMessageId;
            _logger.Info($"[{messageId}] ebMS message is unanswered");

            UpdateReceptionAwareness(awareness => awareness.IsCompleted = true, repository);
            repository.UpdateOutMessage(messageId, x => x.Operation = Operation.DeadLettered);

            Error errorMessage = CreateError();
            AS4Message as4Message = CreateAS4Message(errorMessage, repository);

            new InMessageService(repository).InsertError(errorMessage, as4Message, cancellationToken);
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