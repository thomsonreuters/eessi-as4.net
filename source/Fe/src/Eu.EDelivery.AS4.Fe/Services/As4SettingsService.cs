using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;
using System.Linq;

namespace Eu.EDelivery.AS4.Fe.Services
{
    public class As4SettingsService : IAs4SettingsService
    {
        private readonly IMapper mapper;

        public As4SettingsService(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public async Task<AS4Model.Settings> GetSettings()
        {
            return await GetFromXml();
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
            
            var existing = file.Agents.SubtmitAgents.FirstOrDefault(agent => agent.Name == settingsAgent.Name);
            if (existing == null)
            {
                var list = file.Agents.SubtmitAgents.ToList();
                list.Add(settingsAgent);
                file.Agents.SubtmitAgents = list.ToArray();
            }
            else mapper.Map(settingsAgent, existing);

            await SaveToXml(file);
        }

        public Task GetByInterface<TInterface>()
        {
            return Task.FromResult(0);
        }

        private async Task<AS4Model.Settings> GetFromXml()
        {
            return await Task.Factory.StartNew(() =>
            {
                using (var reader = new FileStream(@"settings.xml", FileMode.Open))
                {
                    var xml = new XmlSerializer(typeof(AS4Model.Settings));
                    return (AS4Model.Settings) xml.Deserialize(reader);
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
}