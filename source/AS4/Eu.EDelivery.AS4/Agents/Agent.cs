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
        private readonly ILogger _logger;
        private IReceiver _receiver;
        private readonly Model.Internal.Steps _stepConfiguration;
        private readonly Model.Internal.ConditionalStepConfig _conditionalStepConfig;
        private readonly Model.Internal.Transformer _transformerConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class. 
        /// </summary>
        /// <param name="receiver"> Receiver used for receiving messages inside the Agent </param>        
        /// <param name="transformerConfig"> Configuration of the Transformer that should be created to transform to a Central Messaging Model</param>
        /// <param name="stepConfiguration">StepConfiguration which describes the steps that will be executed when messages are received </param>        
        public Agent(IReceiver receiver, Model.Internal.Transformer transformerConfig, Model.Internal.Steps stepConfiguration)
        {
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));
            if (transformerConfig == null) throw new ArgumentNullException(nameof(transformerConfig));
            if (stepConfiguration == null) throw new ArgumentNullException(nameof(stepConfiguration));

            this._receiver = receiver;
            this._transformerConfiguration = transformerConfig;
            this._stepConfiguration = stepConfiguration;

            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class. 
        /// </summary>
        /// <param name="receiver"> Receiver used for receiving messages inside the Agent </param>
        /// <param name="transformerConfig"> Configuration of the Transformer that should be created to transform to a Central Messaging Model</param>
        /// <param name="stepConfiguration">ConditionalStepConfig which describes the steps that will be executed when messages are received </param>        
        public Agent(IReceiver receiver, Model.Internal.Transformer transformerConfig, ConditionalStepConfig stepConfiguration)
        {
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));
            if (transformerConfig == null) throw new ArgumentNullException(nameof(transformerConfig));
            if (stepConfiguration == null) throw new ArgumentNullException(nameof(stepConfiguration));

            this._receiver = receiver;
            this._transformerConfiguration = transformerConfig;
            this._conditionalStepConfig = stepConfiguration;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public AgentConfig AgentConfig { get; set; } = new NullAgentConfig();

        /// <summary>
        /// Start the Agent with the given Settings
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Start(CancellationToken cancellationToken)
        {
            this._logger.Debug($"Start {this.AgentConfig.Name}...");
            cancellationToken.Register(() => this._logger.Debug($"{this.AgentConfig.Name} closing.."));

            Task task = Task.Factory.StartNew(() => StartReceiver(cancellationToken), TaskCreationOptions.LongRunning);

            this._logger.Debug($"{this.AgentConfig.Name} Started!");
            return task;
        }

        /// <summary>
        /// Reset the <see cref="IReceiver"/> implementation inside the <see cref="Agent"/>
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="cancellationToken"></param>
        public void ResetReceiver(IReceiver receiver, CancellationToken cancellationToken)
        {
            this._receiver = receiver;
            this._logger.Info("Restarting Receiver...");
            StartReceiver(cancellationToken);
        }

        private void StartReceiver(CancellationToken cancellationToken)
        {
            try
            {
                this._logger.Debug($"{this.AgentConfig.Name} handling message...");
                this._receiver.StartReceiving(OnReceived, cancellationToken);
                this._logger.Debug($"{this.AgentConfig.Name} message handled");
            }
            catch (AS4Exception exception)
            {
                _logger.Error($"An AS4 Exception occured: {exception.Message}");

                var internalMessage = new InternalMessage { Exception = exception };

                var step = CreateSteps();

                step.ExecuteAsync(internalMessage, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.Fatal($"An unhandled exception occured: {exception.Message}");
                if (exception.InnerException != null)
                {
                    this._logger.Fatal($"Inner Exception: {exception.InnerException.Message}");
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
            InternalMessage internalMessage = await TryTransform(message, cancellationToken);

            if (internalMessage.Exception != null)
            {
                return internalMessage;
            }

            var step = CreateSteps();

            StepResult result = step.ExecuteAsync(internalMessage, cancellationToken).GetAwaiter().GetResult();

            if (result.Exception != null)
                this._logger.Warn($"Executing {this.AgentConfig.Name} Step failed: {result.Exception.Message}");

            return result.InternalMessage;
        }

        private IStep CreateSteps()
        {
            if (this._stepConfiguration != null)
            {
                return StepBuilder.FromSettings(this._stepConfiguration).Build();
            }

            if (this._conditionalStepConfig != null)
            {
                return StepBuilder.FromConditionalConfig(this._conditionalStepConfig).Build();
            }

            throw new InvalidOperationException("There is no StepConfiguration provided.");
        }

        private async Task<InternalMessage> TryTransform(ReceivedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var transformer = new GenericTypeBuilder().SetType(_transformerConfiguration.Type).Build<ITransformer>();
                return await transformer.TransformAsync(message, cancellationToken);
            }
            catch (AS4Exception exception)
            {
                this._logger.Error(exception.Message);
                return new InternalMessage(exception);
            }
        }
    }
}