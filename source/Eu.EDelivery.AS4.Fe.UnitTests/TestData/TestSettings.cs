using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Fe.Tests.TestData
{
    public class TestSettings : IAs4SettingsService
    {
        public Task<Model.Internal.Settings> GetSettings()
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

        public async Task SavePullSendSettings(SettingsPullSend settings)
        {
            throw new NotImplementedException();
        }

        public Task CreateAgent(AgentSettings settingsAgent, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAgent(string name, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAgent(AgentSettings settingsAgent, string originalAgentName, Func<SettingsAgents, AgentSettings[]> getAgents, Action<SettingsAgents, AgentSettings[]> setAgents)
        {
            throw new NotImplementedException();
        }
    }
}