using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    /// <summary>
    /// Service to manage settings.xml
    /// </summary>
    public interface IAs4SettingsService : IModular
    {
        /// <summary>
        /// Saves the base settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        Task SaveBaseSettings(BaseSettings settings);

        /// <summary>
        /// Saves the custom settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        Task SaveCustomSettings(CustomSettings settings);

        /// <summary>
        /// Saves the database settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        Task SaveDatabaseSettings(SettingsDatabase settings);

        /// <summary>
        /// Saves the pull send settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        Task SavePullSendSettings(SettingsPullSend settings);

        /// <summary>
        /// Creates the agent.
        /// </summary>
        /// <param name="settingsAgent">The settings agent.</param>
        /// <param name="getAgents">The get agents.</param>
        /// <param name="setAgents">The set agents.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.AlreadyExistsException">Indicates that an agent with the name already exists.</exception>
        Task CreateAgent(AgentSettings settingsAgent, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents);

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
        Task UpdateAgent(AgentSettings settingsAgent, string originalAgentName, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents);

        /// <summary>
        /// Deletes the agent.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="getAgents">The get agents.</param>
        /// <param name="setAgents">The set agents.</param>
        /// <returns></returns>
        /// <exception cref="Eu.EDelivery.AS4.Fe.NotFoundException"></exception>
        Task DeleteAgent(string name, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents);

        /// <summary>
        /// Get settings
        /// </summary>
        /// <returns>Setting</returns>
        Task<Model.Internal.Settings> GetSettings();
    }
}