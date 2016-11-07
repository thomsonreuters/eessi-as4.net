using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
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
        private readonly IOutMessageService _outMessageService;
        private readonly IInExceptionService _inExceptionService;
        private readonly ILogger _logger;

        private AS4Message _originalAS4Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveExceptionStepDecorator"/> class
        /// </summary>
        /// <param name="step"></param>
        public ReceiveExceptionStepDecorator(IStep step)
        {
            this._step = step;
            this._outMessageService = new OutMessageService(Registry.Instance.DatastoreRepository);
            this._inExceptionService = new InExceptionService(Registry.Instance.DatastoreRepository);
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveExceptionStepDecorator"/> class
        /// Create a Decorator around a given <see cref="IStep"/> implementation
        /// </summary>
        /// <param name="step"> Step to catch</param>
        /// <param name="outMessageService"></param>
        /// <param name="exceptionService"></param>
        public ReceiveExceptionStepDecorator(
            IStep step, IOutMessageService outMessageService, IInExceptionService exceptionService)
        {
            this._step = step;
            this._outMessageService = outMessageService;
            this._inExceptionService = exceptionService;
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
            try
            {
                AS4Exception exception = GetPossibleThrownAS4Exception(internalMessage);
                if (exception == null)
                    return await this._step.ExecuteAsync(internalMessage, cancellationToken);

                this._originalAS4Message = internalMessage.AS4Message;
                return await HandleImplicitError(internalMessage.AS4Message);
            }
            catch (AS4Exception exception)
            {
                if (internalMessage.AS4Message?.IsSignalMessage == true)
                    return ReturnStepResult(internalMessage.AS4Message);

                InitializeFields(internalMessage);
                return await HandleInException(exception);
            }
            catch (Exception)
            {
                AssignResponseHttpCode(internalMessage);
                return ReturnStepResult(internalMessage.AS4Message);
            }
        }

        private AS4Exception GetPossibleThrownAS4Exception(InternalMessage internalMessage)
        {
            var error = internalMessage.AS4Message.PrimarySignalMessage as Error;
            return error?.Exception;
        }

        private async Task<StepResult> HandleImplicitError(AS4Message as4Message)
        {
            var errorMessage = as4Message.PrimarySignalMessage as Error;
            if (errorMessage?.Exception != null)
                await this._inExceptionService.InsertAS4ExceptionAsync(errorMessage.Exception);

            return ReturnStepResult(as4Message);
        }

        private void InitializeFields(InternalMessage internalMessage)
        {
            if (internalMessage.AS4Message?.SecurityHeader.IsSigned == true || 
                internalMessage.AS4Message?.SecurityHeader.IsEncrypted == true)
                internalMessage.AS4Message.SecurityHeader = new SecurityHeader();

            this._originalAS4Message = internalMessage?.AS4Message;
        }

        private async Task<StepResult> HandleInException(AS4Exception exception)
        {
            await this._inExceptionService.InsertAS4ExceptionAsync(exception);
            await InsertSignalsFromExceptionAsync(exception);

            StepResult stepResult = ReturnStepResult(this._originalAS4Message);
            stepResult.InternalMessage.Exception = exception;

            return stepResult;
        }

        private async Task InsertSignalsFromExceptionAsync(AS4Exception exception)
        {
            foreach (string messageId in exception.MessageIds)
                await this._outMessageService.InsertErrorAsync(messageId, this._originalAS4Message);
        }

        private StepResult ReturnStepResult(AS4Message as4Message)
        {
            this._logger.Info("Handled AS4 Exception");

            // TODO: is it the responsibility of the Decorator to create the empty SOAP envelope here?
            if (as4Message.ReceivingPMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback)
                as4Message = CreateEmptySoapEnvelope();

            var internalMessage = new InternalMessage(as4Message);
            return StepResult.Success(internalMessage);
        }

        private AS4Message CreateEmptySoapEnvelope()
        {
            return new AS4MessageBuilder()
                .WithReceivingPMode(this._originalAS4Message.ReceivingPMode).Build();
        }

        private void AssignResponseHttpCode(InternalMessage internalMessage)
        {
            ReceivingProcessingMode receivingPMode = internalMessage.AS4Message.ReceivingPMode;
            receivingPMode = receivingPMode ?? new ReceivingProcessingMode();
            receivingPMode.ErrorHandling.ResponseHttpCode = 500;
        }
    }
}