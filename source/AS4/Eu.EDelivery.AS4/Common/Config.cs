using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using Eu.EDelivery.AS4.Watchers;
using NLog;

namespace Eu.EDelivery.AS4.Common
{
    /// <summary>
    /// Responsible for making sure that every child (ex. Step) is executed in the same Context
    /// </summary>
    public sealed class Config : IConfig
    {
        // TODO: to many fields?
        private static readonly IConfig Singleton = new Config();
        private readonly ILogger _logger;

        private readonly IDictionary<string, string> _configuration;
        private readonly IDictionary<string, SendingProcessingMode> _sendingPModes;
        private readonly IDictionary<string, ReceivingProcessingMode> _receivingPmodes;

        private readonly ConcurrentDictionary<string, SendingProcessingMode> _concurrentSendingPModes;
        private readonly ConcurrentDictionary<string, ReceivingProcessingMode> _concurrentReceivingPModes;

        private Settings _settings;
        private List<SettingsAgent> _agents;

        // Singleton
        public static Config Instance => (Config)Singleton;
        public bool IsInitialized { get; private set; }

        internal Config()
        {
            this._sendingPModes = new Dictionary<string, SendingProcessingMode>();
            this._logger = LogManager.GetCurrentClassLogger();
            this._receivingPmodes = new Dictionary<string, ReceivingProcessingMode>();
            this._configuration = new Dictionary<string, string>(
                StringComparer.CurrentCultureIgnoreCase);

            this._concurrentSendingPModes = new ConcurrentDictionary<string, SendingProcessingMode>(
                StringComparer.CurrentCultureIgnoreCase);
            this._concurrentReceivingPModes = new ConcurrentDictionary<string, ReceivingProcessingMode>(
                StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Initialize Configuration
        /// <exception cref="AS4Exception">Thrown when Local Configuration doesn't get retrieved correctly</exception>
        /// </summary>
        public void Initialize()
        {
            try
            {
                this.IsInitialized = true;
                RetrieveLocalConfiguration();

                new PModeWatcher<ReceivingProcessingMode>(GetReceivePModeFolder(), this._concurrentReceivingPModes);
                new PModeWatcher<SendingProcessingMode>(GetSendPModeFolder(), this._concurrentSendingPModes);
            }
            catch (Exception exception)
            {
                this._logger.Error(exception.Message);
            }
        }

        private void RetrievePModes(string pmodeFolder, Action<FileSystemInfo> assign)
        {
            var startDir = new DirectoryInfo(pmodeFolder);

            FileInfo[] files = startDir.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo file in files) assign(file);
        }

        private string GetSendPModeFolder()
        {
            return Path.Combine(
                Properties.Resources.configurationfolder,
                Properties.Resources.sendpmodefolder);
        }

        private string GetReceivePModeFolder()
        {
            return Path.Combine(
                Properties.Resources.configurationfolder,
                Properties.Resources.receivepmodefolder);
        }

        private void AssignPModeToDictionary<T>(FileSystemInfo file, IDictionary<string, T> dictionary, IValidator<T> validator) where T : class, IPMode
        {
            var pmode = TryDeserialize<T>(file.FullName);
            if (pmode == null) return;
            validator.Validate(pmode);

            if (dictionary.ContainsKey(pmode.Id))
                this._logger.Warn($"Multiple PModes configuration files are found with Id: {pmode.Id}");

            if (pmode.Id != null) dictionary[pmode.Id] = pmode;
        }

        private void RetrieveLocalConfiguration()
        {
            string path = Path.Combine(
                Properties.Resources.configurationfolder,
                Properties.Resources.settingsfilename);

            this._settings = TryDeserialize<Settings>(path);
            AssignSettingsToGlobalConfiguration();
        }

        private T TryDeserialize<T>(string path) where T : class
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(fileStream) as T;
                }
            }
            catch (Exception)
            {
                this._logger.Error($"Cannot Deserialize PMode on location: {path}");
                return null;
            }
        }

        private void AssignSettingsToGlobalConfiguration()
        {
            AddFixedSettigns();
            AddCustomSettings();
            AddCustomAgents();
        }

        private void AddFixedSettigns()
        {
            this._configuration["IdFormat"] = this._settings.IdFormat;
            this._configuration["Provider"] = this._settings.Database.Provider;
            this._configuration["ConnectionString"] = this._settings.Database.ConnectionString;
            this._configuration["CertificateStore"] = this._settings.CertificateStoreName;
        }

        private void AddCustomSettings()
        {
            if (this._settings.CustomSettings?.Setting == null) return;
            foreach (Setting setting in this._settings.CustomSettings.Setting)
                this._configuration[setting.Key] = setting.Value;
        }

        private void AddCustomAgents()
        {
            this._agents = new List<SettingsAgent>();
            this._agents.AddRange(this._settings.Agents.ReceiveAgents);
            this._agents.AddRange(this._settings.Agents.SubtmitAgents);
            this._agents.AddRange(this._settings.Agents.SendAgents);
            this._agents.AddRange(this._settings.Agents.DeliverAgents);
            this._agents.AddRange(this._settings.Agents.NotifyAgents);
            this._agents.Add(this._settings.Agents.ReceptionAwarenessAgent);
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="AS4Exception"></exception>
        /// <returns></returns>
        public SendingProcessingMode GetSendingPMode(string id)
        {
            if (id == null)
                throw new AS4Exception("Given Sending PMode key is null");

            SendingProcessingMode pmode = null;
            //this._sendingPModes.TryGetValue(id, out pmode);
            this._concurrentSendingPModes.TryGetValue(id, out pmode);

            if (pmode == null)
                throw new AS4Exception("Multiple keys found for Sending Processing Mode");

            return pmode;
        }

        public ReceivingProcessingMode GetReceivingPMode(string id)
        {
            if (id == null)
                throw new AS4Exception("Given Receiving PMode key is null");

            ReceivingProcessingMode pmode = null;
            //this._receivingPmodes.TryGetValue(id, out pmode);
            this._concurrentReceivingPModes.TryGetValue(id, out pmode);

            if (pmode == null)
                throw new AS4Exception("Multiple keys found for Receiving Processing Mode");

            return pmode;
        }

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key"> Registered Key for the Setting </param>
        /// <returns>
        /// </returns>
        public string GetSetting(string key) => this._configuration.ReadOptionalProperty(key);

        /// <summary>
        /// Get the configured settings agents
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SettingsAgent> GetSettingsAgents() => this._agents;

        /// <summary>
        /// Return all the configured <see cref="ReceivingProcessingMode"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReceivingProcessingMode> GetReceivingPModes() => this._concurrentReceivingPModes.Values;
    }
}