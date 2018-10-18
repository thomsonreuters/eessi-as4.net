using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using NLog;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using PullRequest = Eu.EDelivery.AS4.Model.Core.PullRequest;
using SignalMessage = Eu.EDelivery.AS4.Model.Core.SignalMessage;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    [Info("Create an AS4 Error message")]
    [Description("Create an AS4 Error message to inform the sender that something went wrong processing the received AS4 message")]
    public class CreateAS4ErrorStep : IStep
    {
        private readonly IConfig _config;
        private readonly Func<DatastoreContext> _createDatastoreContext;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep" /> class.
        /// </summary>
        public CreateAS4ErrorStep() : this(Config.Instance, Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep"/> class.
        /// </summary>
        public CreateAS4ErrorStep(
            IConfig config,
            Func<DatastoreContext> createDatastoreContext)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (createDatastoreContext == null)
            {
                throw new ArgumentNullException(nameof(createDatastoreContext));
            }

            _config = config;
            _createDatastoreContext = createDatastoreContext;
        }

        /// <summary>
        /// Start creating <see cref="Error"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            bool noAS4MessagePresent = 
                messagingContext?.AS4Message == null 
                || messagingContext.AS4Message.IsEmpty;

            if (noAS4MessagePresent && messagingContext?.ErrorResult == null)
            {
                Logger.Warn("Skip creating AS4 Error because AS4Message and ErrorResult is empty in the MessagingContext");
                return await StepResult.SuccessAsync(messagingContext);
            }

            ErrorResult errorResult = messagingContext.ErrorResult;
            SendingProcessingMode responseSendPMode =
                messagingContext.GetReferencedSendingPMode(messagingContext.ReceivingPMode, _config);

            AS4Message errorMessage = CreateAS4ErrorWithPossibleMultihop(
                sendPMode: responseSendPMode,
                referenced: messagingContext.AS4Message,
                occurredError: errorResult);

            if (errorResult != null)
            {
                Logger.Debug(
                    $"AS4 Error(s) created with {errorResult.Code.GetString()} \"{errorResult.Alias}, {errorResult.Description}\"");

                await InsertInExceptionsForNowExceptionedInMessageAsync(
                    messagingContext.AS4Message.SignalMessages,
                    messagingContext.ErrorResult,
                    messagingContext.ReceivingPMode);
            }

            messagingContext.ModifyContext(errorMessage);
            messagingContext.SendingPMode = responseSendPMode;

            if (Logger.IsInfoEnabled && errorMessage.MessageUnits.Any())
            {
                Logger.Info(
                    $"{messagingContext.LogTag} {errorMessage.MessageUnits.Count()} Error(s) has been created for received AS4 UserMessages");
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static AS4Message CreateAS4ErrorWithPossibleMultihop(
            SendingProcessingMode sendPMode, 
            AS4Message referenced,
            ErrorResult occurredError)
        {
            Error ToError(UserMessage u)
            {
                var routedUserMessage = AS4Mapper.Map<RoutingInputUserMessage>(u);
                if (routedUserMessage == null)
                {
                    return occurredError == null 
                        ? new Error(u.MessageId) 
                        : new Error(u.MessageId, ErrorLine.FromErrorResult(occurredError));
                }

                return occurredError == null 
                    ? new Error(u.MessageId, routedUserMessage) 
                    : new Error(u.MessageId, ErrorLine.FromErrorResult(occurredError), routedUserMessage);
            }

            IEnumerable<Error> errors = referenced?.UserMessages.Select(ToError) ?? new Error[0];
            AS4Message errorMessage = AS4Message.Create(errors, sendPMode);
            errorMessage.SigningId = referenced?.SigningId;

            return errorMessage;
        }

        private async Task InsertInExceptionsForNowExceptionedInMessageAsync(
            IEnumerable<SignalMessage> signalMessages,
            ErrorResult occurredError,
            ReceivingProcessingMode receivePMode)
        {
            if (signalMessages.Any() == false)
            {
                return;
            }

            using (DatastoreContext dbContext = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(dbContext);

                foreach (SignalMessage signal in signalMessages.Where(s => !(s is PullRequest)))
                {
                    var ex = InException.ForEbmsMessageId(signal.MessageId, occurredError.Description);
                    await ex.SetPModeInformationAsync(receivePMode);

                    Logger.Debug(
                        $"Insert InException for {signal.GetType().Name} {signal.MessageId} with {{Exception={occurredError.Description}}}");

                    repository.InsertInException(ex);
                }

                IEnumerable<string> ebmsMessageIds = signalMessages.Select(s => s.MessageId).ToArray();
                repository.UpdateInMessages(
                    m => ebmsMessageIds.Contains(m.EbmsMessageId),
                    m =>
                    {
                        Logger.Debug($"Update {m.EbmsMessageType} InMessage {m.EbmsMessageId} Status=Exception");
                        m.SetStatus(InStatus.Exception);
                    });

                await dbContext.SaveChangesAsync();
            }
        }
    }
}