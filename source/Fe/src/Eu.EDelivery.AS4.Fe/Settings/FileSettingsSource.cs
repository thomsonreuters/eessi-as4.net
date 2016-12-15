using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public class FileSettingsSource : ISettingsSource
    {
        public Task<Model.Internal.Settings> Get()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var reader = new FileStream(@"settings.xml", FileMode.Open))
                {
                    var xml = new XmlSerializer(typeof(Model.Internal.Settings));
                    return (Model.Internal.Settings) xml.Deserialize(reader);
                }
            });
        }

        public Task Save(Model.Internal.Settings settings)
        {
            return Task.Factory.StartNew(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(Model.Internal.Settings));

                using (var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, settings);
                    File.WriteAllText(@"settings.xml", textWriter.ToString(), Encoding.Unicode);
                }
            });
        }
    }
}