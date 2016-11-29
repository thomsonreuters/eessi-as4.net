using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;
using System.Linq;
using System;

namespace Eu.EDelivery.AS4.Fe.Services
{
    public class As4SettingsService : IAs4SettingsService
    {
        private readonly IMapper mapper;
        private readonly ISettingsSource settingsSource;

        public As4SettingsService(IMapper mapper) //, ISettingsSource settingsSource)
        {
            this.mapper = mapper;
            //this.settingsSource = settingsSource;
        }     

        public async Task SaveBaseSettings(BaseSettings settings)
        {
            var file = await GetSettings();
            mapper.Map(settings, file);
            await SaveToXml(file);
        }

        public async Task SaveCustomSettings(CustomSettings settings)
        {
            var file = await GetSettings();
            file.CustomSettings = settings;
            await SaveToXml(file);
        }

        public async Task SaveDatabaseSettings(SettingsDatabase settings)
        {
            var file = await GetSettings();
            mapper.Map(settings, file.Database);
            await SaveToXml(file);
        }

        public async Task UpdateOrCreateSubmitAgent(SettingsAgent settingsAgent)
        {
            var file = await GetSettings();

            var existing = file.Agents.SubmitAgents.FirstOrDefault(agent => agent.Name == settingsAgent.Name);
            if (existing == null)
            {
                var list = file.Agents.SubmitAgents.ToList();
                list.Add(settingsAgent);
                file.Agents.SubmitAgents = list.ToArray();
            }
            else mapper.Map(settingsAgent, existing);

            await SaveToXml(file);
        }

        public async Task DeleteSubmitAgent(SettingsAgent settingsAgent)
        {
            var file = await GetSettings();
        }

        public Task GetByInterface<TInterface>()
        {
            return Task.FromResult(0);
        }

        public Task<AS4Model.Settings> GetSettings()
        {
            //return await settingsSource.Get();
            return Task.Factory.StartNew(() =>
            {
                using (var reader = new FileStream(@"settings.xml", FileMode.Open))
                {
                    var xml = new XmlSerializer(typeof(AS4Model.Settings));
                    return (AS4Model.Settings)xml.Deserialize(reader);
                }
            });
        }

        private async Task SaveToXml(AS4Model.Settings applicationSettings)
        {
            await Task.Factory.StartNew(() =>
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(AS4Model.Settings));

                using (var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, applicationSettings);
                    File.WriteAllText(@"settings.xml", textWriter.ToString(), Encoding.Unicode);
                }
            });
        }
    }

    public interface ISettingsSource
    {
        Task<AS4Model.Settings> Get();
        void Save(AS4Model.Settings settings);
    }

    public class SettingsSource : ISettingsSource
    {
        public Task<AS4Model.Settings> Get()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var reader = new FileStream(@"settings.xml", FileMode.Open))
                {
                    var xml = new XmlSerializer(typeof(AS4Model.Settings));
                    return (AS4Model.Settings)xml.Deserialize(reader);
                }
            });
        }

        public void Save(AS4Model.Settings settings)
        {
            throw new NotImplementedException();
        }
    }
}