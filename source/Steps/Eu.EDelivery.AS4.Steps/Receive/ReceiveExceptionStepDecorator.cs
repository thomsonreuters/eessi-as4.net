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
using Eu.EDelivery.AS4.Steps.Services;
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
            this._step = step;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Execute the given Step, so it can be catch
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var inExceptionService = new InExceptionService(new DatastoreRepository(context));
                
                try
                {
                    AS4Exception exception = GetPossibleThrownAS4Exception(internalMessage);

                    if (exception == null)
                    {
                        return await _step.ExecuteAsync(internalMessage, cancellationToken);
                    }

                    _originalAS4Message = internalMessage.AS4Message;

                    return await HandleImplicitError(internalMessage.AS4Message, inExceptionService);
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
                    return await HandleInException(exception, internalMessage.AS4Message, inExceptionService);
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

        private static AS4Exception GetPossibleThrownAS4Exception(InternalMessage internalMessage)
        {
            var error = internalMessage.AS4Message.PrimarySignalMessage as Error;
            return error?.Exception;
        }

        private async Task<StepResult> HandleImplicitError(AS4Message as4Message, InExceptionService inExceptionService)
        {
            var errorMessage = as4Message.PrimarySignalMessage as Error;
            if (errorMessage?.Exception != null)
            {
                await inExceptionService.InsertAS4ExceptionAsync(errorMessage.Exception, as4Message);
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

        private async Task<StepResult> HandleInException(AS4Exception exception, AS4Message as4Message, InExceptionService inExceptionService)
        {
            await inExceptionService.InsertAS4ExceptionAsync(exception, as4Message);

            StepResult stepResult = await ReturnStepResult(_originalAS4Message);
            stepResult.InternalMessage.Exception = exception;

            return stepResult;
        }

        private async Task<StepResult> ReturnStepResult(AS4Message as4Message)
        {
            this._logger.Info("Handled AS4 Exception");

            var internalMessage = new InternalMessage(as4Message);
            return await StepResult.SuccessAsync(internalMessage);
        }
       

        private static void AssignResponseHttpCode(InternalMessage internalMessage)
        {
            ReceivingProcessingMode receivingPMode = internalMessage.AS4Message.ReceivingPMode;
            receivingPMode = receivingPMode ?? new ReceivingProcessingMode();
            receivingPMode.ErrorHandling.ResponseHttpCode = 500;
        }
    }
}