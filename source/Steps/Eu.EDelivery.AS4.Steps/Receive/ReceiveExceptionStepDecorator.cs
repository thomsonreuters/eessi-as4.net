using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Exception Handling Step: acts as a Decorator for the <see cref="CompositeStep"/>
    /// Responsibility: describes what to do in case an exception occurs within a AS4 Send/Submit operation
    /// </summary>
    public class ReceiveExceptionStepDecorator : IStep
    {
        private readonly IStep _step;
        private readonly ILogger _logger;

        private MessagingContext _originalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveExceptionStepDecorator"/> class
        /// </summary>
        /// <param name="step"></param>
        public ReceiveExceptionStepDecorator(IStep step)
        {
            _step = step;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Execute the given Step, so it can be catch
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var inExceptionService = new InExceptionService(new DatastoreRepository(context));

                try
                {
                    return await ExecuteNormalStepFlow(messagingContext, context, inExceptionService, cancellationToken);
                }
                catch (AS4Exception exception)
                {
                    if (messagingContext.AS4Message?.IsSignalMessage == true)
                    {
                        // We were unable to process the received signal-message.  
                        // Make sure the InternalMessage contains an empty AS4Message so that
                        // no AS4 Message is written to the response stream.
                        messagingContext.AS4Exception = exception;

                        return StepResult.Failed(exception, messagingContext);
                    }

                    InitializeFields(messagingContext);

                    StepResult result =
                        await HandleInException(exception, messagingContext.AS4Message, inExceptionService)
                            .ConfigureAwait(false);
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    return result;
                }
                catch (Exception exception)
                {
                    _logger.Error($"An unexpected error occured: {exception.Message}");
                    _logger.Trace(exception.StackTrace);
                    AssignResponseHttpCode(messagingContext);

                    return await ReturnStepResult(messagingContext);
                }
            }
        }

        private async Task<StepResult> ExecuteNormalStepFlow(
            MessagingContext messagingContext,
            DbContext context,
            IInExceptionService inExceptionService,
            CancellationToken cancellationToken)
        {
            AS4Exception exception = GetPossibleThrownAS4Exception(messagingContext);

            if (exception == null)
            {
                return await _step.ExecuteAsync(messagingContext, cancellationToken).ConfigureAwait(false);
            }

            StepResult result =
                await HandleImplicitError(messagingContext, inExceptionService).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return result;
        }

        private static AS4Exception GetPossibleThrownAS4Exception(MessagingContext messagingContext)
        {
            var error = messagingContext.AS4Message?.PrimarySignalMessage as Error;
            return error?.Exception;
        }

        private async Task<StepResult> HandleImplicitError(
            MessagingContext message,
            IInExceptionService inExceptionService)
        {
            var errorMessage = message.AS4Message.PrimarySignalMessage as Error;
            if (errorMessage?.Exception != null)
            {
                inExceptionService.InsertAS4Exception(errorMessage.Exception, message.AS4Message);
            }

            return await ReturnStepResult(message);
        }

        private void InitializeFields(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message?.SecurityHeader.IsSigned == true ||
                messagingContext.AS4Message?.SecurityHeader.IsEncrypted == true)
            {
                messagingContext.AS4Message.SecurityHeader = new SecurityHeader();
            }

            _originalMessage = messagingContext;
        }

        private async Task<StepResult> HandleInException(
            AS4Exception exception,
            AS4Message as4Message,
            IInExceptionService inExceptionService)
        {
            inExceptionService.InsertAS4Exception(exception, as4Message);

            StepResult stepResult = await ReturnStepResult(_originalMessage);
            stepResult.MessagingContext.AS4Exception = exception;

            return stepResult;
        }

        private async Task<StepResult> ReturnStepResult(MessagingContext message)
        {
            _logger.Info("Handled AS4 Exception");

            // TODO: why do we create a new MessagingContext here ?
            var internalMessage = new MessagingContext(message.AS4Message, MessagingContextMode.Receive) {SendingPMode = message.SendingPMode, ReceivingPMode = message.ReceivingPMode};
            return await StepResult.SuccessAsync(internalMessage);
        }

        private static void AssignResponseHttpCode(MessagingContext messagingContext)
        {
            ReceivingProcessingMode receivingPMode = messagingContext.ReceivingPMode;
            receivingPMode = receivingPMode ?? new ReceivingProcessingMode();
            receivingPMode.ErrorHandling.ResponseHttpCode = 500;
        }
    }
}