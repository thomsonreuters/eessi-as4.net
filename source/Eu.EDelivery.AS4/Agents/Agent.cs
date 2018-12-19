using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Services.Journal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Transformers;
using NLog;

namespace Eu.EDelivery.AS4.Agents
{
    internal class Agent : IAgent
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IReceiver _receiver;
        private readonly Transformer _transformerConfig;
        private readonly IAgentExceptionHandler _exceptionHandler;
        private readonly StepExecutioner _steps;
        private readonly IJournalLogger _journalLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="config">The config to add metadata to the agent.</param>
        /// <param name="receiver">The receiver on which the agent should listen for messages.</param>
        /// <param name="transformerConfig">The config to create <see cref="ITransformer"/> instances.</param>
        /// <param name="exceptionHandler">The handler to handle failures during the agent execution.</param>
        /// <param name="stepConfiguration">The config to create <see cref="IStep"/> normal & error pipelines.</param>
        internal Agent(
            AgentConfig config,
            IReceiver receiver,
            Transformer transformerConfig,
            IAgentExceptionHandler exceptionHandler,
            StepConfiguration stepConfiguration)
            : this(config, receiver, transformerConfig, exceptionHandler, stepConfiguration, NoopJournalLogger.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="config">The config to add metadata to the agent.</param>
        /// <param name="receiver">The receiver on which the agent should listen for messages.</param>
        /// <param name="transformerConfig">The config to create <see cref="ITransformer"/> instances.</param>
        /// <param name="exceptionHandler">The handler to handle failures during the agent execution.</param>
        /// <param name="stepConfiguration">The config to create <see cref="IStep"/> normal & error pipelines.</param>
        /// <param name="journalLogger">The logging implementation to write journal log entries for handled messages.</param>
        internal Agent(
            AgentConfig config,
            IReceiver receiver,
            Transformer transformerConfig,
            IAgentExceptionHandler exceptionHandler,
            StepConfiguration stepConfiguration,
            IJournalLogger journalLogger)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (receiver == null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (transformerConfig == null)
            {
                throw new ArgumentNullException(nameof(transformerConfig));
            }

            if (exceptionHandler == null)
            {
                throw new ArgumentNullException(nameof(exceptionHandler));
            }

            if (stepConfiguration == null)
            {
                throw new ArgumentNullException(nameof(stepConfiguration));
            }

            _receiver = receiver;
            _transformerConfig = transformerConfig;
            _exceptionHandler = exceptionHandler;
            _steps = new StepExecutioner(stepConfiguration, exceptionHandler);
            _journalLogger = journalLogger ?? NoopJournalLogger.Instance;

            AgentConfig = config;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="config">The config to add meta data information to the agent.</param>
        /// <param name="receiver">The receiver on which the agent should listen for messages.</param>
        /// <param name="transformerConfig">The config to create <see cref="ITransformer"/> instances.</param>
        /// <param name="exceptionHandler">The handler to handle failures during the agent execution.</param>
        /// <param name="pipelineConfig">The config to create <see cref="IStep"/> normal & error pipelines.</param>
        /// <remarks>This should only be used inside a 'Minder' scenario!</remarks>
        internal Agent(
            AgentConfig config,
            IReceiver receiver,
            Transformer transformerConfig,
            IAgentExceptionHandler exceptionHandler,
            (ConditionalStepConfig happyPath, ConditionalStepConfig unhappyPath) pipelineConfig)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (receiver == null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (transformerConfig == null)
            {
                throw new ArgumentNullException(nameof(transformerConfig));
            }

            if (exceptionHandler == null)
            {
                throw new ArgumentNullException(nameof(exceptionHandler));
            }

            _receiver = receiver;
            _transformerConfig = transformerConfig;
            _exceptionHandler = exceptionHandler;
            _steps = new StepExecutioner(pipelineConfig, exceptionHandler);
            _journalLogger = NoopJournalLogger.Instance;

            AgentConfig = config;
        }

        public AgentConfig AgentConfig { get; }

        /// <summary>
        /// Starts the specified agent.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public Task Start(CancellationToken cancellation)
        {
            Logger.Trace($"Starting {AgentConfig.Name}...");
            cancellation.Register(Stop);

            Task task = Task.Factory.StartNew(
                () => _receiver.StartReceiving(OnReceived, cancellation),
                TaskCreationOptions.LongRunning);

            Logger.Trace($"{AgentConfig.Name} Started!");
            return task;
        }

        protected async Task<MessagingContext> OnReceived(ReceivedMessage message, CancellationToken cancellation)
        {
            MessagingContext context;

            try
            {
                MessagingContext result =
                    await TransformerBuilder
                          .FromTransformerConfig(_transformerConfig)
                          .TransformAsync(message);

                if (result == null)
                {
                    throw new ArgumentNullException(
                        nameof(result),
                        $@"Transformer {_transformerConfig.Type} result in a 'null', transformers require to transform into a 'MessagingContext'");
                }

                context = result;
            }
            catch (Exception exception)
            {
                Logger.Error($"Could not transform message: {exception.Message}");
                Logger.Trace(exception.StackTrace);

                return await _exceptionHandler.HandleTransformationException(exception, message);
            }

            if (context?.ErrorResult != null)
            {
                return context;
            }

            StepResult stepResult = await _steps.ExecuteStepsAsync(context);
            IEnumerable<JournalLogEntry> journalWithAgentLocation =
                stepResult.Journal.Select(j =>
                {
                    j.AddAgentLocation(AgentConfig);
                    return j;
                });

            await _journalLogger.WriteLogEntriesAsync(journalWithAgentLocation);
            return stepResult.MessagingContext;
        }

        /// <summary>
        /// Stops this agent.
        /// </summary>
        public void Stop()
        {
            Logger.Debug($"Stopping {AgentConfig.Name} ...");
            _receiver.StopReceiving();
            Logger.Info($"{AgentConfig.Name} stopped.");
        }
    }
}
