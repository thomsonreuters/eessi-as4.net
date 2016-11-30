using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public class As4SettingsService : IAs4SettingsService
    {
        private readonly IMapper mapper;
        private readonly ISettingsSource settingsSource;

        public As4SettingsService(IMapper mapper, ISettingsSource settingsSource)
        {
            this.mapper = mapper;
            this.settingsSource = settingsSource;
        }

        public async Task SaveBaseSettings(BaseSettings settings)
        {
            var file = await GetSettings();
            mapper.Map(settings, file);
            await settingsSource.Save(file);
        }

        public async Task SaveCustomSettings(CustomSettings settings)
        {
            var file = await GetSettings();
            file.CustomSettings = settings;
            await settingsSource.Save(file);
        }

        public async Task SaveDatabaseSettings(SettingsDatabase settings)
        {
            var file = await GetSettings();
            mapper.Map(settings, file.Database);
            await settingsSource.Save(file);
        }

        public async Task UpdateOrCreateAgent(SettingsAgent settingsAgent, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents)
        {
            if (settingsAgent == null) throw new ArgumentNullException(nameof(SettingsAgent), $"Parameter {nameof(SettingsAgent)} cannot be null");
            if (getAgents == null) throw new ArgumentNullException(nameof(getAgents), $"Parameter {nameof(getAgents)} cannot be null");
            if (setAgents == null) throw new ArgumentNullException(nameof(setAgents), $"Paramter {nameof(setAgents)} cannot be null");

            var file = await GetSettings();
            var agents = getAgents(file.Agents).ToList();
            var existing = agents.FirstOrDefault(agent => agent.Name == settingsAgent.Name);
            if (existing == null)
            {
                agents.Add(settingsAgent);
                setAgents(file.Agents, agents.ToArray());
            }
            else mapper.Map(settingsAgent, existing);

            await settingsSource.Save(file);
        }

        public async Task DeleteAgent(string name, Func<SettingsAgents, SettingsAgent[]> getAgents, Action<SettingsAgents, SettingsAgent[]> setAgents)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Parameter name cannot be empty");
            if (getAgents == null) throw new ArgumentNullException(nameof(getAgents), $"{nameof(getAgents)} cannot be null");
            if(setAgents == null) throw new ArgumentNullException(nameof(setAgents), $"{nameof(setAgents)} cannot be null");

            var file = await GetSettings();
            var agents = getAgents(file.Agents);

            var agent = agents.FirstOrDefault(agt => agt.Name == name);
            if (agent == null) throw new Exception($"Submit agent {name} could not be found");
            var newList = agents.ToList();
            newList.Remove(agent);
            setAgents(file.Agents, newList.ToArray());

            await settingsSource.Save(file);
        }

        public async Task<AS4Model.Settings> GetSettings()
        {
            return await settingsSource.Get();
        }

        public Task GetByInterface<TInterface>()
        {
            return Task.FromResult(0);
        }
    }
}