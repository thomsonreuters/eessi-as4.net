using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Services;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public interface IAs4SettingsService : IModular
    {
        Task<Model.Internal.Settings> GetSettings();
        Task SaveBaseSettings(BaseSettings settings);
        Task SaveCustomSettings(CustomSettings settings);
        Task SaveDatabaseSettings(SettingsDatabase settings);
        Task CreateAgent(SettingsAgent settingsAgent, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents);
        Task DeleteAgent(string name, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents);
        Task UpdateAgent(SettingsAgent settingsAgent, string originalAgentName, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents);
    }
}