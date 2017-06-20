using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Transformers;
using NLog;

namespace Eu.EDelivery.AS4.Agents
{
    public class AgentBase : IAgent
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IReceiver _receiver;
        private readonly Transformer _transformerConfig;
        private readonly IAgentExceptionHandler _exceptionHandler;
        private readonly (Model.Internal.Steps happyPath, Model.Internal.Steps unhappyPath) _pipelineConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentBase" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="transformerConfig">The transformer.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="pipelineConfig">The pipeline configuration.</param>
        internal AgentBase(
            string name,
            IReceiver receiver,
            Transformer transformerConfig,
            IAgentExceptionHandler exceptionHandler,
            (Model.Internal.Steps happyPath, Model.Internal.Steps unhappyPath) pipelineConfig)
        {
            _receiver = receiver;
            _transformerConfig = transformerConfig;
            _exceptionHandler = exceptionHandler;
            _pipelineConfig = pipelineConfig;

            AgentConfig = new AgentConfig(name);
        }

        public AgentConfig AgentConfig { get; }

        /// <summary>
        /// Starts the specified agent.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public Task Start(CancellationToken cancellation)
        {
            Logger.Debug($"Start {AgentConfig.Name}...");
            cancellation.Register(Stop);

            Task task = Task.Factory.StartNew(
                () => _receiver.StartReceiving(OnReceived, cancellation),
                TaskCreationOptions.LongRunning);

            Logger.Info($"{AgentConfig.Name} Started!");
            return task;
        }

        protected async Task<MessagingContext> OnReceived(ReceivedMessage message, CancellationToken cancellation)
        {
            MessagingContext context;

            try
            {
                var transformer = GenericTypeBuilder.FromType(_transformerConfig.Type).Build<ITransformer>();
                context = await transformer.TransformAsync(message, cancellation);
            }
            catch (Exception exception)
            {
                Logger.Error("Could not transform message");
                return await _exceptionHandler.HandleTransformationException(message.RequestStream, exception);
            }

            return await TryExecuteSteps(context, cancellation);
        }

        private async Task<MessagingContext> TryExecuteSteps(
            MessagingContext currentContext,
            CancellationToken cancellation)
        {
            if (_pipelineConfig.happyPath == null)
            {
                return currentContext;
            }

            StepResult result = StepResult.Success(currentContext);

            try
            {
                result = await ExecuteSteps(_pipelineConfig.happyPath, currentContext, cancellation);
            }
            catch (Exception exception)
            {
                return await _exceptionHandler.HandleExecutionException(exception, result.MessagingContext);
            }

            try
            {
                if (result.Succeeded == false && _pipelineConfig.unhappyPath != null)
                {
                    result = await ExecuteSteps(_pipelineConfig.unhappyPath, result.MessagingContext, cancellation);
                }

                return result.MessagingContext;
            }
            catch (Exception exception)
            {
                return await _exceptionHandler.HandleErrorException(exception, result.MessagingContext);
            }
        }

        private static async Task<StepResult> ExecuteSteps(
            Model.Internal.Steps pipelineConfig,
            MessagingContext context,
            CancellationToken cancellation)
        {
            IEnumerable<IStep> steps = StepBuilder.FromSettings(pipelineConfig).BuildSteps();
            StepResult result = StepResult.Success(context);

            foreach (IStep step in steps)
            {
                result = await step.ExecuteAsync(context, cancellation).ConfigureAwait(false);

                if (result.CanProceed == false || result.Succeeded == false)
                {
                    return result;
                }

                if (result.MessagingContext != null)
                {
                    context = result.MessagingContext;
                }
            }

            return result;
        }

        /// <summary>
        /// Stops this agent.
        /// </summary>
        public void Stop()
        {
            Logger.Debug($"Stopping {AgentConfig.Name} ...");
            _receiver?.StopReceiving();

            Logger.Info($"{AgentConfig.Name} stopped.");
        }
    }
}
