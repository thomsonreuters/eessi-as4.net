using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EnsureThat;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    /// <summary>
    /// Service to manage settings.xml
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Settings.IAs4SettingsService" />
    public class As4SettingsService : IAs4SettingsService
    {
        private readonly IMapper mapper;
        private readonly ISettingsSource settingsSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="As4SettingsService"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="settingsSource">The settings source.</param>
        public As4SettingsService(IMapper mapper, ISettingsSource settingsSource)
        {
            this.mapper = mapper;
            this.settingsSource = settingsSource;
        }

        /// <summary>
        /// Saves the base settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public async Task SaveBaseSettings(BaseSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            var file = await GetSettings();
            mapper.Map(settings, file);
            await settingsSource.Save(file);
        }

        /// <summary>
        /// Saves the custom settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public async Task SaveCustomSettings(CustomSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            var file = await GetSettings();
            file.CustomSettings = settings;
            await settingsSource.Save(file);
        }

        /// <summary>
        /// Saves the database settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public async Task SaveDatabaseSettings(SettingsDatabase settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));
            var file = await GetSettings();
            mapper.Map(settings, file.Database);
            await settingsSource.Save(file);
        }

        /// <summary>
        /// Creates the agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="getAgents">The get agents.</param>
        /// <param name="setAgents">The set agents.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Indicates that an agent with the name already exists.</exception>
        public async Task CreateAgent(AgentSettings settingsAgent, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNull(getAgents, nameof(getAgents));
            EnsureArg.IsNotNull(setAgents, nameof(setAgents));

            var file = await GetSettings();
            var agents = GetAgents(getAgents, file);
            var existing = agents.FirstOrDefault(agent => StringComparer.OrdinalIgnoreCase.Equals(agent.Name, settingsAgent.Name));
            if (existing != null)
            {
                throw new AlreadyExistsException($"Agent with name {settingsAgent.Name} already exists");
            }

            agents.Add(settingsAgent);
            setAgents(file.Agents, agents.ToArray());

            await settingsSource.Save(file);
        }

        /// <summary>
        /// Updates the agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="originalAgentName">Name of the original agent.</param>
        /// <param name="getAgents">The get agents.</param>
        /// <param name="setAgents">The set agents.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Indicates that an agent with the name already exists</exception>
        /// <exception cref="Eu.EDelivery.AS4.Fe.NotFoundException">Agent doesn't exist</exception>
        public async Task UpdateAgent(AgentSettings settingsAgent, string originalAgentName, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents)
        {
            EnsureArg.IsNotNull(settingsAgent, nameof(settingsAgent));
            EnsureArg.IsNotNullOrEmpty(originalAgentName, nameof(originalAgentName));
            EnsureArg.IsNotNull(getAgents, nameof(getAgents));
            EnsureArg.IsNotNull(setAgents, nameof(setAgents));

            var file = await GetSettings();
            var agents = getAgents(file.Agents);
            // If a rename of an agent is requested then validate that no other agent with the new name exists yet
            if (agents.Any(agt => agt.Name.ToLower() == settingsAgent.Name.ToLower() && agt.Name.ToLower() != originalAgentName.ToLower()))
            {
                throw new AlreadyExistsException($"An agent with name {settingsAgent.Name} already exists");
            }

            var agent = agents.FirstOrDefault(agt => agt.Name.ToLower() == originalAgentName.ToLower());
            if (agent == null) throw new NotFoundException($"{originalAgentName} agent doesn't exist");

            mapper.Map(settingsAgent, agent);
            await settingsSource.Save(file);
        }

        /// <summary>
        /// Deletes the agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="getAgents">The get agents.</param>
        /// <param name="setAgents">The set agents.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.NotFoundException"></exception>
        public async Task DeleteAgent(string name, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            EnsureArg.IsNotNull(getAgents, nameof(getAgents));
            EnsureArg.IsNotNull(setAgents, nameof(setAgents));

            var file = await GetSettings();
            var agents = getAgents(file.Agents);

            var agent = agents.FirstOrDefault(agt => agt.Name.ToLower() == name.ToLower());
            if (agent == null) throw new NotFoundException($"Submit agent {name} could not be found");
            var newList = agents.ToList();
            newList.Remove(agent);
            setAgents(file.Agents, newList.ToArray());

            await settingsSource.Save(file);
        }

        /// <summary>
        /// Get settings
        /// </summary>
        /// <returns>Setting</returns>
        public async Task<Model.Internal.Settings> GetSettings()
        {
            return await settingsSource.Get();
        }

        private static IList<AgentSettings> GetAgents(Func<SettingsAgents, AgentSettings[]> getAgents, Model.Internal.Settings settings)
        {
            var get = getAgents(settings.Agents);
            return get?.ToList() ?? Enumerable.Empty<AgentSettings>().ToList();
        }
    }
}
