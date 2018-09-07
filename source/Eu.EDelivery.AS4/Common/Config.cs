using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;
using Eu.EDelivery.AS4.Validators;
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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly Collection<AgentConfig> _agentConfigs = new Collection<AgentConfig>();

        private PModeWatcher<ReceivingProcessingMode> _receivingPModeWatcher;
        private PModeWatcher<SendingProcessingMode> _sendingPModeWatcher;
        private IPullAuthorizationMapProvider _pullRequestPullAuthorizationMapProvider;
        
        private Settings _settings;
        private TimeSpan _retention;
        private TimeSpan _retryPollingInterval;

        internal Config()
        {
            _configuration = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public static Config Instance => (Config) Singleton;

        /// <summary>
        /// Gets a value indicating whether the FE needs to be started in process.
        /// </summary>
        public bool FeInProcess => OnlyAfterInitialized(() => _settings?.FeInProcess ?? false);

        /// <summary>
        /// Gets a value indicating whether the Payload Service needs to be started in process.
        /// </summary>
        public bool PayloadServiceInProcess => OnlyAfterInitialized(() => _settings?.PayloadServiceInProcess ?? false);

        /// <summary>
        /// Gets the retention period (in days) for which the stored entities are cleaned-up.
        /// </summary>
        /// <value>The retention period in days.</value>
        public TimeSpan RetentionPeriod => OnlyAfterInitialized(() => _retention);

        /// <summary>
        /// Gets the retry polling interval for which the Retry Agent will poll 
        /// for 'to-be-retried' messages/exceptions for a delivery or notification operation.
        /// </summary>
        public TimeSpan RetryPollingInterval => OnlyAfterInitialized(() => _retryPollingInterval);

        private string StoreLocation =>
            OnlyAfterInitialized(() => _settings?.Database?.StoreLocation?.TrimEnd('\\') ?? @"file:///.\database");

        /// <summary>
        /// Gets the location path where the exceptions during an incoming operation are stored.
        /// </summary>
        public string InExceptionStoreLocation => StoreLocation + @"\exceptions\in";

        /// <summary>
        /// Gets the location path where the exceptions during an outgoing operation are stored.
        /// </summary>
        public string OutExceptionStoreLocation => StoreLocation + @"\exceptions\out";

        /// <summary>
        /// Gets the location path where the messages during an incoming operation are stored.
        /// </summary>
        public string InMessageStoreLocation => StoreLocation + @"\as4messages\in";

        /// <summary>
        /// Gets the location path where the messages during an outgoing operation are stored.
        /// </summary>
        public string OutMessageStoreLocation => StoreLocation + @"\as4messages\out";

        /// <summary>
        /// Gets the application path of the AS4.NET Component.
        /// </summary><value>The application path.
        /// </value>
        public static string ApplicationPath => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Gets a value indicating whether if the Configuration is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the specified settings file name.
        /// </summary>
        /// <param name="settingsFileName">Name of the settings file.</param>
        public void Initialize(string settingsFileName)
        {
            if (settingsFileName == null)
            {
                throw new ArgumentNullException(nameof(settingsFileName));
            }

            try
            {
                IsInitialized = true;
                RetrieveLocalConfiguration(settingsFileName);

                _sendingPModeWatcher =
                    new PModeWatcher<SendingProcessingMode>(
                        GetSendPModeFolder(),
                        SendingProcessingModeValidator.Instance);

                _receivingPModeWatcher =
                    new PModeWatcher<ReceivingProcessingMode>(
                        GetReceivePModeFolder(),
                        ReceivingProcessingModeValidator.Instance);

                LoadExternalAssemblies();

                _sendingPModeWatcher.Start();
                _receivingPModeWatcher.Start();
            }
            catch (Exception exception)
            {
                IsInitialized = false;
                Logger.Fatal(exception.Message);

                throw;
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
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return OnlyAfterInitialized(() => _sendingPModeWatcher.ContainsPMode(id));
        }

        /// <summary>
        /// Gets the file location for sending p mode.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Given Sending PMode key is null</exception>
        /// <exception cref="ConfigurationErrorsException">No entry found for the given id</exception>
        public string GetFileLocationForSendingPMode(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return OnlyAfterInitialized(() => GetPModeEntry(id, _sendingPModeWatcher).Filename);
        }

        /// <summary>
        /// Gets the file location for receiving p mode.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Given Receiving PMode key is null</exception>
        /// <exception cref="ConfigurationErrorsException">No entry found for the given id</exception>
        public string GetFileLocationForReceivingPMode(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return OnlyAfterInitialized(() => GetPModeEntry(id, _receivingPModeWatcher).Filename);
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Given Sending PMode key is null</exception>
        /// <exception cref="ConfigurationErrorsException">No entry found for the given id</exception>
        public SendingProcessingMode GetSendingPMode(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return OnlyAfterInitialized(() => GetPModeEntry(id, _sendingPModeWatcher).PMode as SendingProcessingMode);
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Given Receiving PMode key is null</exception>
        /// <exception cref="ConfigurationErrorsException">No entry found for the given id</exception>
        public ReceivingProcessingMode GetReceivingPMode(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return OnlyAfterInitialized(() => GetPModeEntry(id, _receivingPModeWatcher).PMode as ReceivingProcessingMode);
        }

        private static ConfiguredPMode GetPModeEntry<T>(string id, PModeWatcher<T> watcher) where T : class, IPMode
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException($"Given {typeof(T).Name} key is null");
            }

            var entry = watcher.GetPModeEntry(id);

            if (entry == null)
            {
                throw new ConfigurationErrorsException($"No {typeof(T).Name} found for {id}");
            }

            return entry;
        }

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key"> Registered Key for the Setting </param>
        /// <returns>
        /// </returns>
        public string GetSetting(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return OnlyAfterInitialized(() => _configuration.ReadOptionalProperty(key));
        }

        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AgentConfig> GetAgentsConfiguration() => OnlyAfterInitialized(() => _agentConfigs);

        /// <summary>
        /// Return all the configured <see cref="ReceivingProcessingMode" />
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReceivingProcessingMode> GetReceivingPModes() => 
            OnlyAfterInitialized(() => _receivingPModeWatcher.GetPModes().OfType<ReceivingProcessingMode>());

        /// <summary>
        /// Return all the configured <see cref="SendingProcessingMode"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SendingProcessingMode> GetSendingPModes() => 
            OnlyAfterInitialized(() => _sendingPModeWatcher.GetPModes().OfType<SendingProcessingMode>());

        /// <summary>
        /// Retrieve the URL's on which specific MinderSubmitReceiveAgents should listen.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents()
        {
            return OnlyAfterInitialized(
                () => _settings.Agents?.MinderTestAgents?.Where(a => a.Enabled)
                      ?? Enumerable.Empty<SettingsMinderAgent>());
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

            if (Directory.Exists(BaseDirCombine(Properties.Resources.externalfolder)))
            {
                directory = new DirectoryInfo(BaseDirCombine(Properties.Resources.externalfolder));
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
            return BaseDirCombine(Properties.Resources.configurationfolder, Properties.Resources.sendpmodefolder);
        }

        private static string GetReceivePModeFolder()
        {
            return BaseDirCombine(Properties.Resources.configurationfolder, Properties.Resources.receivepmodefolder);
        }

        private void RetrieveLocalConfiguration(string settingsFileName)
        {
            string path = BaseDirCombine(Properties.Resources.configurationfolder, settingsFileName);

            string fullPath = Path.GetFullPath(path);

            Logger.Trace($"Using local configuration settings at path: '{fullPath}'");

            if (Path.IsPathRooted(path) == false ||
                (File.Exists(fullPath) == false && StringComparer.OrdinalIgnoreCase.Equals(path, fullPath) == false))
            {
                path = Path.Combine(".", path);
            }

            if (File.Exists(path) == false)
            {
                throw new FileNotFoundException($"The settings file {path} could not be found.");
            }

            _settings = TryDeserialize<Settings>(path);
            if (_settings == null)
            {
                throw new XmlException("Invalid Settings file");
            }

            AddFixedSettings();
            AddCustomSettings();
            AddCustomAgents();

            ValidateAllSettings();
        }

        private static string BaseDirCombine(params string[] paths)
        {
            return Path.Combine(new[] {ApplicationPath}.Concat(paths).ToArray());
        }

        private static T TryDeserialize<T>(string path) where T : class
        {
            try
            {
                return Deserialize<T>(path);
            }
            catch (Exception ex)
            {
                Logger.Error($"Cannot Deserialize file on location {path}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message);
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

        private void AddFixedSettings()
        {
            _configuration["IdFormat"] = _settings.IdFormat;


            if (_settings.Database == null)
            {
                throw new InvalidOperationException(
                    "The settings file requires a '<Database/>' tag");
            }

            _configuration["Provider"] = _settings.Database.Provider;
            _configuration["ConnectionString"] = _settings.Database.ConnectionString;

            if (_settings.CertificateStore != null)
            {
                _configuration["CertificateStore"] = _settings.CertificateStore.StoreName;
                _configuration["CertificateRepository"] = _settings.CertificateStore?.Repository?.Type;
            }          

            _retention = ParseRetentionPeriod();
            _retryPollingInterval = ParseRetryPollingInterval();

            // TODO: this is hardcoded right now, should be configurable in the settings.xml
            string authorizationMap = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                Properties.Resources.configurationfolder, 
                "Security\\pull_authorizationmap.xml");

            _pullRequestPullAuthorizationMapProvider = new FilePullAuthorizationMapProvider(authorizationMap);
        }

        private TimeSpan ParseRetentionPeriod()
        {
            if (int.TryParse(_settings.RetentionPeriod, out int r) && r > 0)
            {
                return TimeSpan.FromDays(r);
            }

            const int defaultRetentionPeriod = 90;

            LogManager.GetCurrentClassLogger().Warn(
                $"No valid (> 0) Retention Period found: '{_settings.RetentionPeriod ?? "(null)"}', " +
                $"{defaultRetentionPeriod} days as default will be used");

            return TimeSpan.FromDays(defaultRetentionPeriod);
        }

        private TimeSpan ParseRetryPollingInterval()
        {
            if (_settings.RetryReliability != null
                && TimeSpan.TryParse(_settings.RetryReliability.PollingInterval, out TimeSpan t) 
                && t > default(TimeSpan))
            {
                return t;
            }

            const int defaultPollingRetryInterval = 5;

            LogManager.GetCurrentClassLogger().Warn(
                $"No valid (> 0:00:00) Retry Polling Interval found: '{_settings.RetryReliability?.PollingInterval ?? "(null)"}', " +
                $"{defaultPollingRetryInterval} seconds as default will be used");

            return TimeSpan.FromSeconds(defaultPollingRetryInterval);
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
            AddCustomAgentsIfNotNull(AgentType.Notify, _settings.Agents?.NotifyAgents);
            AddCustomAgentsIfNotNull(AgentType.Deliver, _settings.Agents?.DeliverAgents);
            AddCustomAgentsIfNotNull(AgentType.PushSend, _settings.Agents?.SendAgents);
            AddCustomAgentsIfNotNull(AgentType.Submit, _settings.Agents?.SubmitAgents);
            AddCustomAgentsIfNotNull(AgentType.Receive, _settings.Agents?.ReceiveAgents);
            AddCustomAgentsIfNotNull(AgentType.PullReceive, _settings.Agents?.PullReceiveAgents);
            AddCustomAgentsIfNotNull(AgentType.PullSend, _settings.Agents?.PullSendAgents);
            AddCustomAgentsIfNotNull(AgentType.OutboundProcessing, _settings.Agents?.OutboundProcessingAgents);
            AddCustomAgentsIfNotNull(AgentType.Forward, _settings.Agents?.ForwardAgents);
        }

        private void ValidateAllSettings()
        {
            IEnumerable<string> settingsFailures =
                _agentConfigs.Select(c => c.Settings)
                             .SelectMany(ValidateAgentSettings)
                             .Concat(ValidateFixedSettings());

            if (settingsFailures.Any())
            {
                string validationFailure =
                    $"Failure during reading settings file: {Environment.NewLine}"
                    + String.Join(Environment.NewLine, settingsFailures);

                throw new InvalidOperationException(validationFailure);
            }
        }

        private static IEnumerable<string> ValidateAgentSettings(AgentSettings settings)
        {
            if (settings.Receiver?.Type == null)
            {
                yield return $"Agent: {settings.Name} hasn't got a Receiver type configured";
            }
            else if (!GenericTypeBuilder.CanResolveType(settings.Receiver.Type))
            {
                yield return $"Agent: {settings.Name} Receiver type: {settings.Receiver.Type} cannot be resolved";
            }

            if (settings.Transformer?.Type == null)
            {
                yield return $"Agent: {settings.Name} hasn't got a Transformer type configured";
            }
            else if (!GenericTypeBuilder.CanResolveType(settings.Transformer.Type))
            {
                yield return $"Agent: {settings.Name} Transformer type: {settings.Transformer.Type} cannot be resolved";
            }

            if (settings.StepConfiguration?.NormalPipeline == null)
            {
                yield return $"Agent: {settings.Name} hasn't got a Steps.NormalPipeline Step type(s) configured";
            }
            else
            {
                foreach (Step s in settings.StepConfiguration.NormalPipeline)
                {
                    if (!GenericTypeBuilder.CanResolveType(s.Type))
                    {
                        yield return $"Agent: {settings.Name} has a Step in the NormalPipeline with type: {s.Type ?? "<null>"} that cannot be resolved";
                    }
                }
            }

            if (settings.StepConfiguration?.ErrorPipeline != null)
            {
                foreach (Step s in settings.StepConfiguration.ErrorPipeline)
                {
                    if (!GenericTypeBuilder.CanResolveType(s.Type))
                    {
                        yield return $"Agent: {settings.Name} has a Step in the NormalPipeline with type: {s.Type ?? "<null>"} that cannot be resolved";
                    }
                }
            }
        }

        private IEnumerable<string> ValidateFixedSettings()
        {
            string repoType = _settings.CertificateStore?.Repository?.Type;
            if (!GenericTypeBuilder.CanResolveType(repoType))
            {
                yield return $"Certificate store type: {repoType} cannot be resolved";
            }
        }

        private void AddCustomAgentsIfNotNull(AgentType type, params AgentSettings[] agents)
        {
            if (agents == null)
            {
                return;
            }
            
            foreach (AgentSettings setting in agents)
            {
                if (setting != null)
                {
                    _agentConfigs.Add(new AgentConfig(setting.Name)
                    {
                        Type = type,
                        Settings = setting
                    });
                }
            }
        } 

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _agentConfigs.Clear();
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

        /// <summary>
        /// Gets the IAuthorizationMapProvider that must be used when verifying PullRequests.
        /// </summary>
        /// <returns></returns>
        public IPullAuthorizationMapProvider PullRequestAuthorizationMapProvider => 
            OnlyAfterInitialized(() => _pullRequestPullAuthorizationMapProvider);

        private T OnlyAfterInitialized<T>(Func<T> f)
        {
            if (IsInitialized)
            {
                return f();
            }

            throw new InvalidOperationException(
                "Cannot use this member before the configuration is initialized. " + 
                $"Call the {nameof(Initialize)} method to initialize the configuration.");
        }
    }
}