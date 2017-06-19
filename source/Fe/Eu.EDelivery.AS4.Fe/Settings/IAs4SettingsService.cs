using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public interface IAs4SettingsService : IModular
    {
        Task<Model.Internal.Settings> GetSettings();
        Task SaveBaseSettings(BaseSettings settings);
        Task SaveCustomSettings(CustomSettings settings);
        Task SaveDatabaseSettings(SettingsDatabase settings);
        Task CreateAgent(AgentSettings settingsAgent, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents);
        Task DeleteAgent(string name, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents);
        Task UpdateAgent(AgentSettings settingsAgent, string originalAgentName, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents);
    }
}