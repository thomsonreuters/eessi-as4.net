using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Watchers;
using NLog;

namespace Eu.EDelivery.AS4.Common
{
    /// <summary>
    /// Responsible for making sure that every child (ex. Step) is executed in the same Context
    /// </summary>
    public sealed class Config : IConfig, IDisposable
    {
        private static readonly IConfig Singleton = new Config();
        private readonly IDictionary<string, string> _configuration;
        private readonly ILogger _logger;

        private readonly List<AgentSettings> _agents = new List<AgentSettings>();
        private readonly Collection<AgentConfig> _agentConfigs = new Collection<AgentConfig>();

        private PModeWatcher<ReceivingProcessingMode> _receivingPModeWatcher;
        private PModeWatcher<SendingProcessingMode> _sendingPModeWatcher;
        private Settings _settings;

        internal Config()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _configuration = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public static Config Instance => (Config)Singleton;

        /// <summary>
        /// Gets a value indicating whether the FE needs to be started in process.
        /// </summary>
        public bool FeInProcess { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the Payload Service needs to be started in process.
        /// </summary>
        public bool PayloadServiceInProcess { get; private set; }

        /// <summary>
        /// Gets the in message store location.
        /// </summary>
        /// <value>The in message store location.</value>
        public string InMessageStoreLocation => _settings?.Database?.InMessageStoreLocation ?? @"file:///.\database\as4messages\in";

        /// <summary>
        /// Gets the out message store location.
        /// </summary>
        /// <value>The out message store location.</value>
        public string OutMessageStoreLocation => _settings?.Database?.OutMessageStoreLocation ?? @"file:///.\database\as4messages\out";

        /// <summary>
        /// Gets a value indicating whether if the Configuration is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initialize Configuration
        /// </summary>
        public void Initialize()
        {
            try
            {
                IsInitialized = true;
                RetrieveLocalConfiguration();

                _sendingPModeWatcher = new PModeWatcher<SendingProcessingMode>(GetSendPModeFolder());
                _receivingPModeWatcher = new PModeWatcher<ReceivingProcessingMode>(GetReceivePModeFolder());

                LoadExternalAssemblies();

                _sendingPModeWatcher.Start();
                _receivingPModeWatcher.Start();
            }
            catch (Exception exception)
            {
                IsInitialized = false;
                _logger.Error(exception.Message);
            }
        }

        /// <summary>
        /// Verify if the <see cref="IConfig" /> implementation contains a <see cref="SendingProcessingMode" /> for a given
        /// <paramref name="id" />
        /// </summary>
        /// <param name="id">The Sending Processing Mode id for which the verification is done.</param>
        /// <returns></returns>
        public bool ContainsSendingPMode(string id)
        {
            return _sendingPModeWatcher.ContainsPMode(id);
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SendingProcessingMode GetSendingPMode(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("Given Sending PMode key is null");
            }

            IPMode pmode = _sendingPModeWatcher.GetPMode(id);

            if (pmode == null)
            {
                throw new ConfigurationErrorsException($"No Sending Processing Mode found for {id}");
            }

            return pmode as SendingProcessingMode;
        }

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key"> Registered Key for the Setting </param>
        /// <returns>
        /// </returns>
        public string GetSetting(string key) => _configuration.ReadOptionalProperty(key);

        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AgentConfig> GetAgentsConfiguration() => _agentConfigs;

        /// <summary>
        /// Return all the configured <see cref="ReceivingProcessingMode" />
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReceivingProcessingMode> GetReceivingPModes() => _receivingPModeWatcher.GetPModes().OfType<ReceivingProcessingMode>();

        /// <summary>
        /// Retrieve the URL's on which specific MinderSubmitReceiveAgents should listen.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents()
        {
            if (_settings.Agents.MinderTestAgents == null)
            {
                return new SettingsMinderAgent[] { };
            }

            return _settings.Agents.MinderTestAgents.Where(a => a.Enabled);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private static void LoadExternalAssemblies()
        {
            DirectoryInfo externalDictionary = GetExternalDirectory();
            if (externalDictionary != null)
            {
                LoadExternalAssemblies(externalDictionary);
            }
        }

        private static DirectoryInfo GetExternalDirectory()
        {
            DirectoryInfo directory = null;

            if (Directory.Exists(Properties.Resources.externalfolder))
            {
                directory = new DirectoryInfo(Properties.Resources.externalfolder);
            }

            return directory;
        }

        private static void LoadExternalAssemblies(DirectoryInfo externalDictionary)
        {
            foreach (FileInfo assemblyFile in externalDictionary.GetFiles("*.dll"))
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile.FullName);
                AppDomain.CurrentDomain.Load(assembly.GetName());
            }
        }

        private static string GetSendPModeFolder()
        {
            return Path.Combine(Properties.Resources.configurationfolder, Properties.Resources.sendpmodefolder);
        }

        private static string GetReceivePModeFolder()
        {
            return Path.Combine(Properties.Resources.configurationfolder, Properties.Resources.receivepmodefolder);
        }

        private void RetrieveLocalConfiguration()
        {
            string path = Path.Combine(Properties.Resources.configurationfolder, Properties.Resources.settingsfilename);

            string fullPath = Path.GetFullPath(path);

            if (Path.IsPathRooted(path) == false || 
                (File.Exists(fullPath) == false && StringComparer.OrdinalIgnoreCase.Equals(path, fullPath) == false))
            {
                path = Path.Combine(".", path);
            }

            _settings = TryDeserialize<Settings>(path);
            if (_settings == null)
            {
                throw new XmlException("Invalid Settings file");
            }

            AssignSettingsToGlobalConfiguration();
        }

        private T TryDeserialize<T>(string path) where T : class
        {
            try
            {
                return Deserialize<T>(path);
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot Deserialize file on location {path}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.Error(ex.InnerException.Message);
                }

                return null;
            }
        }

        private static T Deserialize<T>(string path) where T : class
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(fileStream) as T;
            }
        }

        private void AssignSettingsToGlobalConfiguration()
        {
            AddFixedSettings();
            AddCustomSettings();
            AddCustomAgents();
        }

        private void AddFixedSettings()
        {
            _configuration["IdFormat"] = _settings.IdFormat;
            _configuration["Provider"] = _settings.Database.Provider;
            _configuration["ConnectionString"] = _settings.Database.ConnectionString;
            _configuration["CertificateStore"] = _settings.CertificateStore.StoreName;
            _configuration["CertificateRepository"] = _settings.CertificateStore?.Repository?.Type;

            FeInProcess = _settings.FeInProcess;
            PayloadServiceInProcess = _settings.PayloadServiceInProcess;
        }

        private void AddCustomSettings()
        {
            if (_settings.CustomSettings?.Setting == null)
            {
                return;
            }

            foreach (Setting setting in _settings.CustomSettings.Setting)
            {
                _configuration[setting.Key] = setting.Value;
            }
        }

        private void AddCustomAgents()
        {
            AddCustomAgentsIfNotNull(AgentType.ReceptionAwareness, _settings.Agents.ReceptionAwarenessAgent);
            AddCustomAgentsIfNotNull(AgentType.NotifyConsumer, _settings.Agents.NotifyConsumerAgents);
            AddCustomAgentsIfNotNull(AgentType.NotifyProducer, _settings.Agents.NotifyProducerAgents);
            AddCustomAgentsIfNotNull(AgentType.Deliver, _settings.Agents.DeliverAgents);
            AddCustomAgentsIfNotNull(AgentType.PushSend, _settings.Agents.SendAgents);
            AddCustomAgentsIfNotNull(AgentType.Submit, _settings.Agents.SubmitAgents);
            AddCustomAgentsIfNotNull(AgentType.Receive, _settings.Agents.ReceiveAgents);
            AddCustomAgentsIfNotNull(AgentType.PullReceive, _settings.Agents.PullReceiveAgents);
            AddCustomAgentsIfNotNull(AgentType.PullSend, _settings.Agents.PullSendAgents);
            AddCustomAgentsIfNotNull(AgentType.OutboundProcessing, _settings.Agents.OutboundProcessingAgents);
            AddCustomAgentsIfNotNull(AgentType.Forward, _settings.Agents.ForwardAgents);
        }

        private void AddCustomAgentsIfNotNull(AgentType type, params AgentSettings[] agents)
        {
            if (agents == null)
            {
                return;
            }

            _agents.AddRange(agents.Where(a => a != null));

            foreach (AgentSettings setting in agents)
            {
                if (setting != null)
                {
                    _agentConfigs.Add(new AgentConfig(setting.Name) { Type = type, Settings = setting }); 
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_sendingPModeWatcher != null)
            {
                _sendingPModeWatcher.Stop();
                _sendingPModeWatcher.Dispose();
            }

            if (_receivingPModeWatcher != null)
            {
                _receivingPModeWatcher.Stop();
                _receivingPModeWatcher.Dispose();
            }
        }
    }
}