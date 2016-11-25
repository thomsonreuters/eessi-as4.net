using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Services;

namespace Eu.EDelivery.AS4.Fe.Tests
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

        public Task UpdateOrCreateSubmitAgent(SettingsAgent settingsAgent)
        {
            throw new NotImplementedException();
        }
    }
}