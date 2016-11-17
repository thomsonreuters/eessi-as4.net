using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Services;

namespace Eu.EDelivery.AS4.Fe.FileSettings
{
    public class As4SettingsService : IAs4SettingsService
    {
        private readonly IMapper mapper;

        public As4SettingsService(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public async Task<Settings> GetSettings()
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
            mapper.Map(settings, file.CustomSettings);
            await SaveToXml(file);
        }

        public async Task SaveDatabaseSettings(SettingsDatabase settings)
        {
            var file = await GetSettings();
            mapper.Map(settings, file.Database);
            await SaveToXml(file);
        }

        private async Task<Settings> GetFromXml()
        {
            return await Task.Factory.StartNew(() =>
            {
                using (var reader = new FileStream(@"settings.xml", FileMode.Open))
                {
                    var xml = new XmlSerializer(typeof(Settings));
                    return (Settings) xml.Deserialize(reader);
                }
            });
        }

        private async Task SaveToXml(Settings applicationSettings)
        {
            await Task.Factory.StartNew(() =>
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));

                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, applicationSettings);
                    File.WriteAllText(@"settings.xml", textWriter.ToString(), Encoding.Unicode);
                }
            });
        }
    }
}