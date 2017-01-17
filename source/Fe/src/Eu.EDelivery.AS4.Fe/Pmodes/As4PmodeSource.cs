using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Eu.EDelivery.AS4.Model.PMode;
using Microsoft.Extensions.Options;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    public class As4PmodeSource : IAs4PmodeSource
    {
        private readonly IOptions<PmodeSettings> settings;

        public As4PmodeSource(IOptions<PmodeSettings> settings)
        {
            this.settings = settings;
        }

        public Task<IEnumerable<string>> GetReceivingNames()
        {
            return Task.Factory.StartNew(() => Directory
                .GetFiles(settings.Value.ReceivingPmodeFolder, "*.xml")
                .Select(Path.GetFileNameWithoutExtension));
        }

        public Task<ReceivingPmode> GetReceivingByName(string name)
        {
            return Task.Factory.StartNew(() => Directory
                .GetFiles(settings.Value.ReceivingPmodeFolder, "*.xml")
                .Where(file => Path.GetFileNameWithoutExtension(file) == name)
                .Select(pmode =>
                {
                    using (var reader = new FileStream(pmode, FileMode.Open))
                    {
                        var xml = new XmlSerializer(typeof(ReceivingProcessingMode));
                        var result = new ReceivingPmode
                        {
                            Name = Path.GetFileNameWithoutExtension(pmode),
                            Type = PmodeType.Receiving,
                            Pmode = (ReceivingProcessingMode) xml.Deserialize(reader)
                        };
                        return result;
                    }
                })
                .FirstOrDefault());
        }

        public Task<IEnumerable<string>> GetSendingNames()
        {
            return Task.Factory.StartNew(() => Directory
                .GetFiles(settings.Value.SendingPmodeFolder, "*.xml")
                .Select(Path.GetFileNameWithoutExtension));
        }

        public Task<SendingPmode> GetSendingByName(string name)
        {
            return Task.Factory.StartNew(() => Directory
                .GetFiles(settings.Value.SendingPmodeFolder, "*.xml")
                .Where(file => Path.GetFileNameWithoutExtension(file) == name)
                .Select(pmode =>
                {
                    using (var reader = new FileStream(pmode, FileMode.Open))
                    {
                        var xml = new XmlSerializer(typeof(SendingProcessingMode));
                        var result = new SendingPmode
                        {
                            Name = Path.GetFileNameWithoutExtension(pmode),
                            Type = PmodeType.Sending,
                            Pmode = (SendingProcessingMode) xml.Deserialize(reader)
                        };
                        return result;
                    }
                })
                .FirstOrDefault());
        }

        public Task CreateReceiving(ReceivingPmode pmode)
        {
            var xmlSerializer = new XmlSerializer(typeof(ReceivingProcessingMode));
            var path = Path.Combine(settings.Value.ReceivingPmodeFolder, $"{pmode.Name}.xml");

            return Task.Factory.StartNew(() =>
            {
                using (var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, pmode.Pmode);
                    File.WriteAllText(path, textWriter.ToString(), Encoding.Unicode);
                }
            });
        }

        public Task DeleteReceiving(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var path = Path.Combine(settings.Value.ReceivingPmodeFolder, $"{name}.xml");
                File.Delete(path);
            });
        }

        public Task DeleteSending(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var path = Path.Combine(settings.Value.SendingPmodeFolder, $"{name}.xml");
                File.Delete(path);
            });
        }

        public Task CreateSending(SendingPmode pmode)
        {
            var xmlSerializer = new XmlSerializer(typeof(SendingProcessingMode));
            var path = Path.Combine(settings.Value.SendingPmodeFolder, $"{pmode.Name}.xml");

            return Task.Factory.StartNew(() =>
            {
                using (var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, pmode.Pmode);
                    File.WriteAllText(path, textWriter.ToString(), Encoding.Unicode);
                }
            });
        }

        public async Task UpdateSending(SendingPmode pmode, string originalName)
        {
            await CreateSending(pmode);
        }

        public async Task UpdateReceiving(ReceivingPmode pmode, string originalName)
        {
            await CreateReceiving(pmode);
        }
    }
}