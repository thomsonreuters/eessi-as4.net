using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Fe.Hash;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Eu.EDelivery.AS4.Model.PMode;
using Microsoft.Extensions.Options;
using System.Xml;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    /// <summary>
    /// As4 PMode source
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Pmodes.IAs4PmodeSource" />
    public class As4PmodeSource : IAs4PmodeSource
    {
        private readonly IOptions<PmodeSettings> settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="As4PmodeSource"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public As4PmodeSource(IOptions<PmodeSettings> settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Gets the receiving names.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetReceivingNames()
        {
            return Task
                .Factory
                .StartNew(() => Directory.GetFiles(settings.Value.ReceivingPmodeFolder, "*.xml")
                .Select(Path.GetFileNameWithoutExtension));
        }

        /// <summary>
        /// Gets the name of the receiving by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Task<ReceivingBasePmode> GetReceivingByName(string name)
        {
            return Task.Factory.StartNew(() => Directory
                .GetFiles(settings.Value.ReceivingPmodeFolder, "*.xml")
                .Where(file => Path.GetFileNameWithoutExtension(file) == name)
                .Select(pmode =>
                {
                    using (var reader = new FileStream(pmode, FileMode.Open))
                    {
                        var xml = new XmlSerializer(typeof(ReceivingProcessingMode));
                        var result = new ReceivingBasePmode
                        {
                            Name = Path.GetFileNameWithoutExtension(pmode),
                            Type = PmodeType.Receiving,
                            Pmode = (ReceivingProcessingMode)xml.Deserialize(reader)
                        };
                        return result;
                    }
                })
                .FirstOrDefault());
        }

        /// <summary>
        /// Gets the sending names.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetSendingNames()
        {
            return Task
                .Factory
                .StartNew(() => Directory.GetFiles(settings.Value.SendingPmodeFolder, "*.xml")
                .Select(Path.GetFileNameWithoutExtension));
        }

        /// <summary>
        /// Gets the name of the sending by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Task<SendingBasePmode> GetSendingByName(string name)
        {
            return Task
                .Factory
                .StartNew(() => Directory.GetFiles(settings.Value.SendingPmodeFolder, "*.xml")
                .Where(file => Path.GetFileNameWithoutExtension(file) == name)
                .Select(pmode =>
                {
                    var xmlString = File.ReadAllText(pmode);
                    using (var reader = XmlReader.Create(new StringReader(xmlString)))
                    {
                        var xml = new XmlSerializer(typeof(SendingProcessingMode));
                        var result = new SendingBasePmode
                        {
                            Name = Path.GetFileNameWithoutExtension(pmode),
                            Type = PmodeType.Sending,
                            Pmode = (SendingProcessingMode)xml.Deserialize(reader),
                            Hash = xmlString.GetMd5Hash()
                        };
                        return result;
                    }
                })
                .FirstOrDefault());
        }

        /// <summary>
        /// Creates the receiving.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <returns></returns>
        public Task CreateReceiving(ReceivingBasePmode basePmode)
        {
            var xmlSerializer = new XmlSerializer(typeof(ReceivingProcessingMode));
            var path = Path.Combine(settings.Value.ReceivingPmodeFolder, $"{basePmode.Name}.xml");

            return Task.Factory.StartNew(() =>
            {
                using (var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, basePmode.Pmode);
                    File.WriteAllText(path, textWriter.ToString(), Encoding.Unicode);
                }
            });
        }

        /// <summary>
        /// Deletes the receiving.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Task DeleteReceiving(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var path = Path.Combine(settings.Value.ReceivingPmodeFolder, $"{name}.xml");
                File.Delete(path);
            });
        }

        /// <summary>
        /// Deletes the sending.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Task DeleteSending(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var path = Path.Combine(settings.Value.SendingPmodeFolder, $"{name}.xml");
                File.Delete(path);
            });
        }

        /// <summary>
        /// Creates the sending.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <returns></returns>
        public Task CreateSending(SendingBasePmode basePmode)
        {
            var xmlSerializer = new XmlSerializer(typeof(SendingProcessingMode));
            var path = Path.Combine(settings.Value.SendingPmodeFolder, $"{basePmode.Name}.xml");

            return Task.Factory.StartNew(() =>
            {
                using (var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, basePmode.Pmode);
                    File.WriteAllText(path, textWriter.ToString(), Encoding.Unicode);
                }
            });
        }

        /// <summary>
        /// Gets the pmode number.
        /// </summary>
        /// <param name="pmodeString">The pmode string.</param>
        /// <returns></returns>
        public string GetPmodeNumber(string pmodeString)
        {
            return XDocument.Parse(pmodeString).Root?.Descendants()?.FirstOrDefault(x => x.Name.LocalName == "Id")?.Value;
        }

        /// <summary>
        /// Updates the sending.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        public async Task UpdateSending(SendingBasePmode basePmode, string originalName)
        {
            await CreateSending(basePmode);
        }

        /// <summary>
        /// Updates the receiving.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <param name="originalName">Name of the original.</param>
        /// <returns></returns>
        public async Task UpdateReceiving(ReceivingBasePmode basePmode, string originalName)
        {
            await CreateReceiving(basePmode);
        }
    }
}