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

        private AS4Message _originalAS4Message;

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
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var inExceptionService = new InExceptionService(new DatastoreRepository(context));

                try
                {
                    return await ExecuteNormalStepFlow(internalMessage, context, inExceptionService, cancellationToken);
                }
                catch (AS4Exception exception)
                {
                    if (internalMessage.AS4Message?.IsSignalMessage == true)
                    {
                        // We were unable to process the received signal-message.  
                        // Make sure the InternalMessage contains an empty AS4Message so that
                        // no AS4 Message is written to the response stream.
                        internalMessage.Exception = exception;
                        internalMessage.AS4Message = new AS4MessageBuilder().Build();

                        return StepResult.Failed(exception, internalMessage);
                    }

                    InitializeFields(internalMessage);

                    StepResult result =
                        await HandleInException(exception, internalMessage.AS4Message, inExceptionService)
                            .ConfigureAwait(false);
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    return result;
                }
                catch (Exception exception)
                {
                    _logger.Error($"An unexpected error occured: {exception.Message}");
                    _logger.Trace(exception.StackTrace);
                    AssignResponseHttpCode(internalMessage);

                    return await ReturnStepResult(internalMessage.AS4Message);
                }
            }
        }

        private async Task<StepResult> ExecuteNormalStepFlow(
            InternalMessage internalMessage,
            DbContext context,
            IInExceptionService inExceptionService,
            CancellationToken cancellationToken)
        {
            AS4Exception exception = GetPossibleThrownAS4Exception(internalMessage);

            if (exception == null)
            {
                return await _step.ExecuteAsync(internalMessage, cancellationToken).ConfigureAwait(false);
            }

            _originalAS4Message = internalMessage.AS4Message;

            StepResult result =
                await HandleImplicitError(internalMessage.AS4Message, inExceptionService).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return result;
        }

        private static AS4Exception GetPossibleThrownAS4Exception(InternalMessage internalMessage)
        {
            var error = internalMessage.AS4Message?.PrimarySignalMessage as Error;
            return error?.Exception;
        }

        private async Task<StepResult> HandleImplicitError(
            AS4Message as4Message,
            IInExceptionService inExceptionService)
        {
            var errorMessage = as4Message.PrimarySignalMessage as Error;
            if (errorMessage?.Exception != null)
            {
                inExceptionService.InsertAS4Exception(errorMessage.Exception, as4Message);
            }

            return await ReturnStepResult(as4Message);
        }

        private void InitializeFields(InternalMessage internalMessage)
        {
            if (internalMessage.AS4Message?.SecurityHeader.IsSigned == true ||
                internalMessage.AS4Message?.SecurityHeader.IsEncrypted == true)
            {
                internalMessage.AS4Message.SecurityHeader = new SecurityHeader();
            }

            _originalAS4Message = internalMessage.AS4Message;
        }

        private async Task<StepResult> HandleInException(
            AS4Exception exception,
            AS4Message as4Message,
            IInExceptionService inExceptionService)
        {
            inExceptionService.InsertAS4Exception(exception, as4Message);

            StepResult stepResult = await ReturnStepResult(_originalAS4Message);
            stepResult.InternalMessage.Exception = exception;

            return stepResult;
        }

        private async Task<StepResult> ReturnStepResult(AS4Message as4Message)
        {
            _logger.Info("Handled AS4 Exception");

            var internalMessage = new InternalMessage(as4Message);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private static void AssignResponseHttpCode(InternalMessage internalMessage)
        {
            ReceivingProcessingMode receivingPMode = internalMessage.ReceivingPMode;
            receivingPMode = receivingPMode ?? new ReceivingProcessingMode();
            receivingPMode.ErrorHandling.ResponseHttpCode = 500;
        }
    }
}