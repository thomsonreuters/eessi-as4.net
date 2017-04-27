using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the Messages.Out and Messages.In data stores get updated
    /// </summary>
    public class SendUpdateDataStoreStep : IStep
    {
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyPersister _messageBodyPersister;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep" /> class
        /// </summary>
        public SendUpdateDataStoreStep() : this(Registry.Instance.CreateDatastoreContext, Config.Instance.IncomingAS4MessageBodyPersister) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep"/> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyPersister"></param>
        public SendUpdateDataStoreStep(Func<DatastoreContext> createDatastoreContext, IAS4MessageBodyPersister messageBodyPersister)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyPersister = messageBodyPersister;
        }

        /// <summary>
        /// Execute the Update DataStore Step
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            using (DatastoreContext context = _createDatastoreContext())
            {
                var inMessageService = new InMessageService(new DatastoreRepository(context), _messageBodyPersister);
                var signalMessageUpdate = new SignalMessageStatement(internalMessage, inMessageService, cancellationToken);

                signalMessageUpdate.InsertSignalMessages();
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return await StepResult.SuccessAsync(internalMessage);
        }

        /// <summary>
        /// Method Object for the <see cref="SignalMessage"/> instances.
        /// </summary>
        private class SignalMessageStatement
        {
            private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

            private readonly InternalMessage _originalMessage;
            private readonly IInMessageService _messageService;
            private readonly CancellationToken _cancellation;

            /// <summary>
            /// Initializes a new instance of the <see cref="SignalMessageStatement"/> class.
            /// </summary>
            public SignalMessageStatement(
                InternalMessage originalMessage,
                IInMessageService messageService,
                CancellationToken cancellation)
            {
                _originalMessage = originalMessage;
                _messageService = messageService;
                _cancellation = cancellation;
            }

            /// <summary>
            /// Start updating the <see cref="SignalMessage"/> instances for the 'Send' operation.
            /// </summary>
            public void InsertSignalMessages()
            {
                foreach (SignalMessage signalMessage in _originalMessage.AS4Message.SignalMessages)
                {
                    Logger.Info($"{_originalMessage.Prefix} Update SignalMessage {signalMessage.MessageId}");

                    AttemptToInsertSignalMessage(signalMessage);
                }
            }

            private void AttemptToInsertSignalMessage(SignalMessage signalMessage)
            {
                try
                {
                    InsertSignalMessage(signalMessage);
                }
                catch (Exception exception)
                {
                    string description = $"Unable to update SignalMessage {signalMessage.MessageId}";
                    throw ThrowAS4UpdateDatastoreException(description, exception);
                }
            }

            private void InsertSignalMessage(SignalMessage signalMessage)
            {
                if (signalMessage is Receipt)
                {
                    InsertReceipt(signalMessage);
                }
                else if (signalMessage is Error)
                {
                    InsertError(signalMessage);
                }
            }

            private void InsertReceipt(SignalMessage signalMessage)
            {
                if (signalMessage is Receipt receipt && receipt.NonRepudiationInformation == null)
                {
                    receipt.NonRepudiationInformation = CreateNonRepudiationInformation();
                }

                _messageService.InsertReceipt(signalMessage, _originalMessage.AS4Message, _cancellation);
            }

            private NonRepudiationInformation CreateNonRepudiationInformation()
            {
                ArrayList references = _originalMessage.AS4Message.SecurityHeader.GetReferences();

                return new NonRepudiationInformationBuilder().WithSignedReferences(references).Build();
            }

            private void InsertError(SignalMessage signalMessage)
            {
                _messageService.InsertError(signalMessage, _originalMessage.AS4Message, _cancellation);
            }

            private AS4Exception ThrowAS4UpdateDatastoreException(string description, Exception innerException)
            {
                Logger.Error(description);

                return AS4ExceptionBuilder
                    .WithDescription(description)
                    .WithInnerException(innerException)
                    .WithMessageIds(_originalMessage.AS4Message.MessageIds)
                    .WithSendingPMode(_originalMessage.AS4Message.SendingPMode)
                    .Build();
            }
        }
    }
}