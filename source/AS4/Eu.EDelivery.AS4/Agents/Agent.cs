using System;
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
    /// <summary>
    /// Agent Responsibility:
    /// - Receiver (Delegate Receive Message)
    /// - Steps (Delegate to steps)
    /// - Adapter (delegate to Adapter)
    /// </summary>
    public class Agent : IAgent
    {
        private readonly Model.Internal.Steps _stepConfiguration;
        private readonly ConditionalStepConfig _conditionalStepConfig;
        private readonly Transformer _transformerConfiguration;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private IReceiver _receiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class. 
        /// </summary>
        /// <param name="agentConfig"></param>
        /// <param name="receiver"> Receiver used for receiving messages inside the Agent </param>        
        /// <param name="transformerConfig"> Configuration of the Transformer that should be created to transform to a Central Messaging Model</param>
        /// <param name="stepConfiguration">StepConfiguration which describes the steps that will be executed when messages are received </param>        
        public Agent(
            AgentConfig agentConfig,
            IReceiver receiver,
            Transformer transformerConfig,
            Model.Internal.Steps stepConfiguration) : this(agentConfig, receiver, transformerConfig)
        {
            if (stepConfiguration == null) throw new ArgumentNullException(nameof(stepConfiguration));

            _stepConfiguration = stepConfiguration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class. 
        /// </summary>
        /// <param name="agentConfig">The agent Config.</param>
        /// <param name="receiver">Receiver used for receiving messages inside the Agent </param>
        /// <param name="transformerConfig">Configuration of the Transformer that should be created to transform to a Central Messaging Model</param>
        /// <param name="stepConfiguration">ConditionalStepConfig which describes the steps that will be executed when messages are received </param>
        public Agent(
            AgentConfig agentConfig,
            IReceiver receiver,
            Transformer transformerConfig,
            ConditionalStepConfig stepConfiguration) : this(agentConfig, receiver, transformerConfig)
        {
            if (stepConfiguration == null) throw new ArgumentNullException(nameof(stepConfiguration));

            _conditionalStepConfig = stepConfiguration;
        }

        private Agent(AgentConfig agentConfig, IReceiver receiver, Transformer transformerConfig)
        {
            if (agentConfig == null) throw new ArgumentNullException(nameof(agentConfig));
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));
            if (transformerConfig == null) throw new ArgumentNullException(nameof(transformerConfig));

            AgentConfig = agentConfig;

            _receiver = receiver;
            _transformerConfiguration = transformerConfig;
        }

        public AgentConfig AgentConfig { get; }

        /// <summary>
        /// Start the Agent with the given Settings
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Start(CancellationToken cancellationToken)
        {
            Logger.Debug($"Start {AgentConfig.Name}...");
            cancellationToken.Register(Stop);

            Task task = Task.Factory.StartNew(() => StartReceiver(cancellationToken), TaskCreationOptions.LongRunning);

            Logger.Info($"{AgentConfig.Name} Started!");
            return task;
        }

        public void Stop()
        {
            Logger.Debug($"Stopping {AgentConfig.Name} ...");
            _receiver?.StopReceiving();

            Logger.Info($"{AgentConfig.Name} stopped.");
        }

        /// <summary>
        /// Reset the <see cref="IReceiver"/> implementation inside the <see cref="Agent"/>
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="cancellationToken"></param>
        public void ResetReceiver(IReceiver receiver, CancellationToken cancellationToken)
        {
            _receiver?.StopReceiving();
            _receiver = receiver;
            Logger.Info("Restarting Receiver...");
            StartReceiver(cancellationToken);
        }

        private void StartReceiver(CancellationToken cancellationToken)
        {
            try
            {
                _receiver.StartReceiving(OnReceived, cancellationToken);
            }
            catch (AS4Exception exception)
            {
                Logger.Error($"An AS4 Exception occured: {exception.Message}");

                var internalMessage = new InternalMessage {Exception = exception};

                IStep step = CreateSteps();

                step.ExecuteAsync(internalMessage, cancellationToken);
            }
            catch (Exception exception)
            {
                Logger.Fatal($"An unhandled exception occured: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Logger.Fatal($"Inner Exception: {exception.InnerException.Message}");
                }

                throw;
            }
        }

        /// <summary>
        /// Perform action when Message is received
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        public virtual async Task<InternalMessage> OnReceived(
            ReceivedMessage message,
            CancellationToken cancellationToken)
        {
            Logger.Debug($"{this.AgentConfig.Name} received and starts handling message with id {message.Id}");

            InternalMessage internalMessage = await TryTransform(message, cancellationToken);

            if (internalMessage.Exception != null)
            {
                return internalMessage;
            }

            IStep step = CreateSteps();

            StepResult result = await step.ExecuteAsync(internalMessage, cancellationToken);

            LogIfStepResultFailed(result, message);

            return result.InternalMessage;
        }

        private async Task<InternalMessage> TryTransform(ReceivedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var transformer = GenericTypeBuilder.FromType(_transformerConfiguration.Type).Build<ITransformer>();
                return await transformer.TransformAsync(message, cancellationToken);
            }
            catch (AS4Exception exception)
            {
                Logger.Error(exception.Message);
                return new InternalMessage(exception);
            }
        }

        private IStep CreateSteps()
        {
            if (_stepConfiguration != null)
            {
                return StepBuilder.FromSettings(_stepConfiguration).Build();
            }

            if (_conditionalStepConfig != null)
            {
                return StepBuilder.FromConditionalConfig(_conditionalStepConfig).Build();
            }

            throw new InvalidOperationException("There is no StepConfiguration provided.");
        }

        private void LogIfStepResultFailed(StepResult result, ReceivedMessage message)
        {
            if (result.Exception != null)
            {
                Logger.Warn($"Executing {AgentConfig.Name} Step failed: {result.Exception.Message}");

                if (result.Exception.InnerException != null && Logger.IsTraceEnabled)
                {
                    Logger.Trace(result.Exception.InnerException.StackTrace);
                }
            }

            Logger.Debug($"{AgentConfig.Name} finished handling message with id {message.Id}");
        }
    }
}