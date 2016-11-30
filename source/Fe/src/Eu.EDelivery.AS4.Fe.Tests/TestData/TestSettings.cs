using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Settings;

namespace Eu.EDelivery.AS4.Fe.Tests.TestData
{
    public class TestSettings : IAs4SettingsService
    {
        public Task<AS4Model.Settings> GetSettings()
        {
            throw new NotImplementedException();
        }

        public Task SaveBaseSettings(BaseSettings settings)
        {
            throw new NotImplementedException();
        }

        public Task SaveCustomSettings(CustomSettings settings)
        {
            throw new NotImplementedException();
        }

        public Task SaveDatabaseSettings(SettingsDatabase settings)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOrCreateAgent(SettingsAgent settingsAgent, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAgent(string name, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents)
        {
            throw new NotImplementedException();
        }
    }
}