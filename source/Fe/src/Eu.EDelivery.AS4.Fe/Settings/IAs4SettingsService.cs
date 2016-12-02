using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Services;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public interface IAs4SettingsService : IModular
    {
        Task<AS4Model.Settings> GetSettings();
        Task SaveBaseSettings(BaseSettings settings);
        Task SaveCustomSettings(CustomSettings settings);
        Task SaveDatabaseSettings(SettingsDatabase settings);
        Task CreateAgent(SettingsAgent settingsAgent, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents);
        Task DeleteAgent(string name, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents);
        Task UpdateAgent(SettingsAgent settingsAgent, string originalAgentName, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents);
    }
}