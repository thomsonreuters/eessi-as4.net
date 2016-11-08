using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Transformers;
using NLog;

namespace Eu.EDelivery.AS4.ServiceHandler.Agents
{
    /// <summary>
    /// Agent Provider/Manager Resposibility:
    /// manage the registered Agents (default and extendible)
    /// </summary>
    public class AgentProvider
    {
        private readonly IConfig _config;
        private readonly ICollection<IAgent> _agents;
        private readonly ILogger _logger;

        /// <summary>
        /// Create a <see cref="AgentProvider" />
        /// with the Core and Custom Agents
        /// </summary>
        public AgentProvider(IConfig config)
        {
            this._config = config;
            this._logger = LogManager.GetCurrentClassLogger();
            this._agents = new Collection<IAgent>();
            SetupAgentProvider();
        }

        /// <summary>
        /// Return all the Registered <see cref="IAgent" /> Implementations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAgent> GetAgents()
        {
            return this._agents;
        }

        private void SetupAgentProvider()
        {
            try
            {
                AddCustomAgentsToProvider();
            }
            catch (AS4Exception exception)
            {
                this._logger.Error(exception.Message);
            }
        }

        private void AddCustomAgentsToProvider()
        {
            foreach (SettingsAgent settingAgent in this._config.GetSettingsAgents())
            {
                IAgent agent = GetAgentFromSettings(settingAgent);
                agent.AgentConfig = new AgentConfig(settingAgent.Name);

                this._agents.Add(agent);
            }
        }

        private IAgent GetAgentFromSettings(SettingsAgent agent)
        {
            IReceiver receiver = GetReceiversFromSettings(agent.Receiver);
            var transformer = CreateInstance<ITransformer>(agent.Transformer.Type);
            IStep step = GetStepFromSettings(agent.Steps);

            return new Agent(receiver, transformer, step);
        }

        private IReceiver GetReceiversFromSettings(Receiver settingReceiver)
        {
            var receiver = CreateInstance<IReceiver>(settingReceiver.Type);
            ConfigureReceiverWithSettings(receiver, settingReceiver);

            return receiver;
        }

        private void ConfigureReceiverWithSettings(IReceiver receiver, Receiver settingsReceiver)
        {
            if (settingsReceiver.Setting == null) return;

            Dictionary<string, string> dictionary = settingsReceiver.Setting
                .ToDictionary(setting => setting.Key, setting => setting.Value);

            receiver.Configure(dictionary);
        }

        private IStep GetStepFromSettings(Model.Internal.Steps settingsSteps)
        {
            IStep decoratedStep = CreateDecoratorStep(settingsSteps);

            IList<IStep> unDecoratedSteps = settingsSteps.Step
                .Where(s => s.UnDecorated == true)
                .Select(settingStep => CreateInstance<IStep>(settingStep.Type))
                .ToList();

            if (unDecoratedSteps.Count == 0)
                return decoratedStep;

            unDecoratedSteps.Insert(0, decoratedStep);
            return new CompositeStep(unDecoratedSteps.ToArray());
        }

        private IStep CreateDecoratorStep(Model.Internal.Steps settingsSteps)
        {
            IStep[] decoratedSteps = settingsSteps.Step
                .Where(s => s.UnDecorated == false)
                .Select(settingStep => CreateInstance<IStep>(settingStep.Type))
                .ToArray();

            var compositeStep = new CompositeStep(decoratedSteps);
            return settingsSteps.Decorator != null
                ? CreateInstance<IStep>(settingsSteps.Decorator, compositeStep)
                : compositeStep;
        }

        private T CreateInstance<T>(string typeString, params object[] args) where T : class
        {
            Type type = ResolveType(typeString);
            if (type == null) throw new AS4Exception($"Not given class found for given Type: {typeString}");

            if (args != null) return Activator.CreateInstance(type, args) as T;
            return Activator.CreateInstance(type) as T;
        }

        private Type ResolveType(string type)
        {
            return Type.GetType(type) ?? Type.GetType(type, name =>
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                return assemblies.FirstOrDefault(z => z.FullName == name.FullName);
            }, typeResolver: null, throwOnError: true);
        }
    }
}