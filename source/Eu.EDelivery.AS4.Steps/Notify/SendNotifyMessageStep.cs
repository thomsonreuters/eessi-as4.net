using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Describes how a <see cref="NotifyMessage"/> is sent to the business application 
    /// </summary>
    [Info("Send notification message")]
    [Description("Send a notification message using the method that is configured in the PMode")]
    public class SendNotifyMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly INotifySenderProvider _provider;
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep"/> class
        /// </summary>
        public SendNotifyMessageStep()
            : this(Registry.Instance.NotifySenderProvider, Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendNotifyMessageStep" /> class.
        /// Create a <see cref="IStep" /> implementation
        /// to send a <see cref="NotifyMessage" />
        /// to the consuming business application
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="dataContextRetriever">The data context retriever.</param>
        public SendNotifyMessageStep(INotifySenderProvider provider, Func<DatastoreContext> dataContextRetriever)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (dataContextRetriever == null)
            {
                throw new ArgumentNullException(nameof(dataContextRetriever));
            }

            _provider = provider;
            _createContext = dataContextRetriever;
        }

        /// <summary>
        /// Start sending <see cref="NotifyMessage"/>
        /// to the consuming business application
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.NotifyMessage == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SendNotifyMessageStep)} requires a NotifyMessage to send but no NotifyMessage is present in the MessagingContext");
            }

            if (messagingContext.NotifyMessage.StatusCode == Status.Delivered
                && messagingContext.SendingPMode == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SendNotifyMessageStep)} requires a SendingPMode when the NotifyMessage is a Receipt to be notified, "
                    + "this is indicated by the NotifyMessage.StatusCode = Delivered");
            }

            if (messagingContext.NotifyMessage.StatusCode == Status.Error
                && messagingContext.SendingPMode == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SendNotifyMessageStep)} requires a SendingPMode when the NotifyMessage is an Error to be notified, "
                    + "this is indicated by the NotifyMessage.StatusCode = Error");
            }

            if (messagingContext.NotifyMessage.StatusCode == Status.Exception
                && messagingContext.SendingPMode == null
                && messagingContext.ReceivingPMode == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SendNotifyMessageStep)} requires either a SendingPMode ore ReceivingPMode when the NotifyMessage is an Exception to be notified, "
                    + "this is indicated by teh NotifyMessage.StatusCode = Exception");
            }

            Method notifyMethod = GetNotifyMethodBasedOnNotifyMessage(
                messagingContext.NotifyMessage,
                messagingContext.SendingPMode,
                messagingContext.ReceivingPMode);

            if (messagingContext.SendingPMode == null)
            {
                SendingProcessingMode pmode =
                    RetrieveSendingPModeForMessageWithEbmsMessageId(
                        messagingContext.NotifyMessage
                                        .MessageInfo
                                        ?.RefToMessageId);

                if (pmode != null)
                {
                    Logger.Debug(
                        $"Using SendingPMode {pmode.Id} based on the NotifyMessage.MessageInfo.RefToMessageId "
                        + $"{messagingContext.NotifyMessage.MessageInfo?.RefToMessageId} from the matching stored OutMessage");

                    messagingContext.SendingPMode = pmode;
                }
            }

            Logger.Trace("Start sending NotifyMessage...");
            SendResult result = await SendNotifyMessage(notifyMethod, messagingContext.NotifyMessage).ConfigureAwait(false);
            Logger.Trace($"NotifyMessage sent result in: {result}");

            await UpdateDatastoreAsync(
                messagingContext.NotifyMessage,
                messagingContext.MessageEntityId,
                result);

            return StepResult.Success(messagingContext);
        }

        private static Method GetNotifyMethodBasedOnNotifyMessage(
            NotifyMessageEnvelope notifyMessage,
            SendingProcessingMode sendingPMode,
            ReceivingProcessingMode receivingPMode)
        {
            switch (notifyMessage.StatusCode)
            {
                case Status.Delivered:
                    if (sendingPMode.ReceiptHandling?.NotifyMethod?.Type == null)
                    {
                        throw new InvalidOperationException(
                            $"SendingPMode {sendingPMode.Id} should have a ReceiptHandling.NotifyMethod "
                            + "with a <Type/> element indicating the notifying strategy when the NotifyMessage.StatusCode = Delivered. "
                            + "Default strategies are: 'FILE' and 'HTTP'. See 'Notify Uploading' for more information");
                    }

                    return sendingPMode.ReceiptHandling.NotifyMethod;
                case Status.Error:
                    if (sendingPMode.ErrorHandling?.NotifyMethod?.Type == null)
                    {
                        throw new InvalidOperationException(
                            $"SendingPMode {sendingPMode.Id} should have a ErrorHandling.NotifyMethod "
                            + "with a <Type/> element indicating the notifying strategy when the NotifyMessage.StatusCode = Error. "
                            + "Default strategies are: 'FILE' and 'HTTP'. See 'Notify Uploading' for more information");
                    }

                    return sendingPMode.ErrorHandling.NotifyMethod;
                case Status.Exception:
                    bool isNotifyMessageFormedBySending = sendingPMode?.Id != null;
                    if (isNotifyMessageFormedBySending)
                    {
                        if (sendingPMode?.ExceptionHandling?.NotifyMethod?.Type == null)
                        {
                            throw new InvalidOperationException(
                                $"SendingPMode {sendingPMode?.Id} should have a ExceptionHandling.NotifyMethod "
                                + "with a <Type/> element indicating the notifying strategy when the NotifyMessage.StatusCode = Exception. "
                                + "This means that the NotifyMessage is an Exception occured during a outbound sending operation. "
                                + "Default strategies are: 'FILE' and 'HTTP'. See 'Notify Uploading' for more information");
                        }

                        return sendingPMode.ExceptionHandling.NotifyMethod;
                    }

                    if (receivingPMode?.ExceptionHandling?.NotifyMethod?.Type == null)
                    {
                        throw new InvalidOperationException(
                            $"ReceivingPMode {receivingPMode?.Id} should have a ExceptionHandling.NotifyMethod "
                            + "with a <Type/> element indicating the notifying strategy when the NotifyMessage.StatusCode = Exception. "
                            + "This means that the NotifyMessage is an Exception occured during an inbound receiving operation. "
                            + "Default strategies are: 'FILE' and 'HTTP'. See 'Notify Uploading' for more information");
                    }

                    return receivingPMode.ExceptionHandling.NotifyMethod;
                default:
                    throw new ArgumentOutOfRangeException($"No NotifyMethod not defined for status {notifyMessage.StatusCode}");
            }
        }

        private SendingProcessingMode RetrieveSendingPModeForMessageWithEbmsMessageId(string ebmsMessageId)
        {
            if (ebmsMessageId == null)
            {
                Logger.Debug("Can't retrieve SendingPMode because NotifyMessage.MessageInfo.RefToMessageId is not present");
                return null;
            }

            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var outMessageData = 
                    repository.GetOutMessageData(
                                  where: m => m.EbmsMessageId == ebmsMessageId && m.Intermediary == false,
                                  selection: m => new { m.PMode, m.ModificationTime })
                              .OrderByDescending(m => m.ModificationTime)
                              .FirstOrDefault();

                if (outMessageData == null)
                {
                    Logger.Debug(
                        "Can't retrieve SendingPMode because no matching stored OutMessage found "
                        + $"for EbmsMessageId = {ebmsMessageId} AND Intermediary = false");

                    return null;
                }

                var pmode = AS4XmlSerializer.FromString<SendingProcessingMode>(outMessageData.PMode);
                if (pmode == null)
                {
                    Logger.Debug(
                        "Can't use SendingPMode from matching OutMessage for NotifyMessage.MessageInfo.RefToMessageId "
                        + $"{ebmsMessageId} because the PMode field can't be deserialized correctly to a SendingPMode");
                }

                return pmode;
            }
        }

        private async Task<SendResult> SendNotifyMessage(Method notifyMethod, NotifyMessageEnvelope notifyMessage)
        {
            INotifySender sender = _provider.GetNotifySender(notifyMethod.Type);
            if (sender == null)
            {
                throw new ArgumentNullException(
                    nameof(sender),
                    $@"No {nameof(INotifySender)} found for NotifyMethod.Type = {notifyMethod.Type}");
            }

            sender.Configure(notifyMethod);
            return await sender.SendAsync(notifyMessage).ConfigureAwait(false);
        }

        private async Task UpdateDatastoreAsync(
            NotifyMessageEnvelope notifyMessage, 
            long? messageEntityId,
            SendResult result)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new MarkForRetryService(repository);

                if (!messageEntityId.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Unable to update notified entities of type {notifyMessage.EntityType?.FullName} because no entity id is present");
                }

                if (notifyMessage.EntityType == typeof(InMessage))
                {
                    service.UpdateNotifyMessageForIncomingMessage(messageEntityId.Value, result);
                }
                else if (notifyMessage.EntityType == typeof(OutMessage))
                {
                    service.UpdateNotifyMessageForOutgoingMessage(messageEntityId.Value, result);
                }
                else if (notifyMessage.EntityType == typeof(InException))
                {
                    service.UpdateNotifyExceptionForIncomingMessage(messageEntityId.Value, result);
                }
                else if (notifyMessage.EntityType == typeof(OutException))
                {
                    service.UpdateNotifyExceptionForOutgoingMessage(messageEntityId.Value, result);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unable to update notified entities of type {notifyMessage.EntityType?.FullName}."
                        + "Please provide one of the following types in the notify message: "
                        + "InMessage, OutMessage, InException, and OutException are supported");
                }

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}