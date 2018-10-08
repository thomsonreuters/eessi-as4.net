using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public class FileSettingsSource : ISettingsSource
    {
        private readonly string _settingsPath;

        private static readonly XmlWriterSettings DefaultXmlWriterSettings =
            new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = true,
            };

        public FileSettingsSource(IOptions<ApplicationSettings> appSettings)
        {
            if (appSettings == null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            if (appSettings.Value?.SettingsXml == null)
            {
                throw new ArgumentNullException(nameof(appSettings.Value.SettingsXml));
            }

            _settingsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                appSettings.Value.SettingsXml);
        }

        public Task<Model.Internal.Settings> Get()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var reader = new FileStream(_settingsPath, FileMode.Open))
                {
                    var xml = new XmlSerializer(typeof(Model.Internal.Settings));
                    return (Model.Internal.Settings)xml.Deserialize(reader);
                }
            });
        }

        public Task Save(Model.Internal.Settings settings)
        {
            return Task.Factory.StartNew(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(Model.Internal.Settings));
                using (var output = new FileStream(_settingsPath, FileMode.Create))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(output, DefaultXmlWriterSettings))
                    {
                        xmlSerializer.Serialize(xmlWriter, settings);
                    }
                }
            });
        }
    }
}