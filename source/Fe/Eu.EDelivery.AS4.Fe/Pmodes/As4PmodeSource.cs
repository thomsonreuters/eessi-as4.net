using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.Hash;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Microsoft.Extensions.Options;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    /// <summary>
    /// As4 PMode source
    /// </summary>
    /// <seealso cref="IAs4PmodeSource" />
    public class As4PmodeSource : IAs4PmodeSource
    {
        private readonly IOptionsSnapshot<PmodeSettings> _settings;
        private readonly IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="As4PmodeSource"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public As4PmodeSource(IOptionsSnapshot<PmodeSettings> settings)
        {
            _settings = settings;
            _config = Config.Instance;
        }

        /// <summary>
        /// Gets the receiving names.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetReceivingNames()
        {
            return Task
                .Factory
                .StartNew(() => _config.ReceivingPModeWatcher.GetPModes().Select(p => p.Id));
        }

        /// <summary>
        /// Gets the name of the receiving by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Task<ReceivingBasePmode> GetReceivingByName(string name)
        {
            return Task
                .Factory
                .StartNew(() =>
                {
                    if (_config.ReceivingPModeWatcher.GetPMode(name) is ReceivingProcessingMode pmode)
                    {
                        return new ReceivingBasePmode
                        {
                            Name = pmode.Id,
                            Type = PmodeType.Receiving,
                            Pmode = pmode,
                            Hash = AS4XmlSerializer.ToString(pmode).GetMd5Hash()
                        };
                    }

                    return null;
                });
        }

        /// <summary>
        /// Gets the sending names.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetSendingNames()
        {
            return Task
                .Factory
                .StartNew(() => _config.SendingPModeWatcher.GetPModes().Select(p => p.Id));
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
                .StartNew(() =>
                {
                    if (_config.SendingPModeWatcher.GetPMode(name) is SendingProcessingMode pmode)
                    {
                        return new SendingBasePmode
                        {
                            Name = pmode.Id,
                            Type = PmodeType.Sending,
                            Pmode = pmode,
                            Hash = AS4XmlSerializer.ToString(pmode).GetMd5Hash()
                        };
                    }

                    return null;
                });
        }

        /// <summary>
        /// Creates the receiving.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <returns></returns>
        public async Task CreateReceiving(ReceivingBasePmode basePmode)
        {
            string fileName = Path
                .GetInvalidFileNameChars()
                .Aggregate(basePmode.Name, (acc, c) => acc.Replace(c.ToString(), string.Empty));

            string pmodeFile = Path.Combine(_settings.Value.ReceivingPmodeFolder, fileName + ".xml");

            if (File.Exists(pmodeFile))
            {
                pmodeFile = Path.Combine(_settings.Value.ReceivingPmodeFolder, fileName + "-" + Guid.NewGuid() + ".xml");
            }

            string pmodeString = await AS4XmlSerializer.ToStringAsync(basePmode.Pmode);
            File.WriteAllText(pmodeFile, pmodeString);
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
                string path = _config.ReceivingPModeWatcher.GetPModeEntry(name).Filename;
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
                string path = _config.SendingPModeWatcher.GetPModeEntry(name).Filename;
                File.Delete(path);
            });
        }

        /// <summary>
        /// Creates the sending.
        /// </summary>
        /// <param name="basePmode">The base pmode.</param>
        /// <returns></returns>
        public async Task CreateSending(SendingBasePmode basePmode)
        {
            string fileName = Path
                .GetInvalidFileNameChars()
                .Aggregate(basePmode.Name, (acc, c) => acc.Replace(c.ToString(), string.Empty));

            string pmodeFile = Path.Combine(_settings.Value.SendingPmodeFolder, fileName + ".xml");

            if (File.Exists(pmodeFile))
            {
                pmodeFile = Path.Combine(_settings.Value.SendingPmodeFolder, fileName + "-" + Guid.NewGuid() + ".xml");
            }

            string pmodeString = await AS4XmlSerializer.ToStringAsync(basePmode.Pmode);
            File.WriteAllText(pmodeFile, pmodeString);
        }

        /// <summary>
        /// Gets the pmode number.
        /// </summary>
        /// <param name="pmodeString">The pmode string.</param>
        /// <returns></returns>
        public string GetPmodeNumber(string pmodeString)
        {
            return XDocument.Parse(pmodeString).Root?.Descendants().FirstOrDefault(x => x.Name.LocalName == "Id")?.Value;
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
            File.Delete(_config.SendingPModeWatcher.GetPModeEntry(originalName).Filename);
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
            File.Delete(_config.ReceivingPModeWatcher.GetPModeEntry(originalName).Filename);
        }
    }
}