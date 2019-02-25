using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.TestUtils.Stubs
{
    public class TestConfig : IConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestConfig"/> class.
        /// </summary>
        public TestConfig(string provider, string connectionString, string storeLocation)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            DatabaseProvider = provider;
            DatabaseConnectionString = connectionString;

            CertificateStore = "My";
            CertificateRepositoryType = typeof(CertificateRepository).AssemblyQualifiedName;

            string basePath = storeLocation?.TrimEnd('\\') ?? "file:///.\\database";
            OutExceptionStoreLocation = basePath + "\\exceptions\\out";
            InExceptionStoreLocation = basePath + "\\exceptions\\in";
            OutMessageStoreLocation = basePath + "\\messages\\out";
            InMessageStoreLocation = basePath + "\\messages\\in";
        }

        /// <summary>
        /// Creates a <see cref="IConfig"/> implementation that only initializes database-related values.
        /// </summary>
        /// <param name="settings">The settings to take the database-related values from.</param>
        public static IConfig Create(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.Database == null)
            {
                throw new ArgumentException(@"Requires a 'Database' value to create this DbConfig", nameof(settings.Database));
            }

            return new TestConfig(
                settings.Database.Provider,
                settings.Database.ConnectionString,
                settings.Database.StoreLocation);
        }

        /// <summary>
        /// Initializes the specified settings file name.
        /// </summary>
        /// <param name="settingsFileName">Name of the settings file.</param>
        public void Initialize(string settingsFileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether if the Configuration is IsInitialized
        /// </summary>
        public bool IsInitialized => throw new NotImplementedException();

        /// <summary>
        /// Gets the retention period (in days) for which the stored entities are cleaned-up.
        /// </summary>
        /// <value>The retention period in days.</value>
        public TimeSpan RetentionPeriod => throw new NotImplementedException();

        /// <summary>
        /// Gets the location where the payloads should be retrieved.
        /// </summary>
        public string PayloadRetrievalLocation => throw new NotImplementedException();

        /// <summary>
        /// Gets the location path where the messages during an incoming operation are stored.
        /// </summary>
        public string InMessageStoreLocation { get; }

        /// <summary>
        /// Gets the location path where the messages during an outgoing operation are stored.
        /// </summary>
        public string OutMessageStoreLocation { get; }

        /// <summary>
        /// Gets the location path where the exceptions during an incoming operation are stored.
        /// </summary>
        public string InExceptionStoreLocation { get; }

        /// <summary>
        /// Gets the location path where the exceptions during an outgoing operation are stored.
        /// </summary>
        public string OutExceptionStoreLocation { get; }

        /// <summary>
        /// Gets the format in which Ebms Message Identifiers should be generated.
        /// </summary>
        public string EbmsMessageIdFormat => throw new NotImplementedException();

        /// <summary>
        /// Gets the configured database provider.
        /// </summary>
        public string DatabaseProvider { get; }

        /// <summary>
        /// Gets the configured connection string to contact the database.
        /// </summary>
        public string DatabaseConnectionString { get; }

        /// <summary>
        /// Gets the configured certificate store name.
        /// </summary>
        public string CertificateStore { get; }

        /// <summary>
        /// Gets the configured certificate repository type.
        /// </summary>
        public string CertificateRepositoryType { get; }

        /// <summary>
        /// Gets the file path from where the authorization entries to verify PullRequests should be stored.
        /// </summary>
        public string AuthorizationMapPath => throw new NotImplementedException();

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key">Registered Key for the Setting</param>
        /// <returns></returns>
        public string GetSetting(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verify if the <see cref="IConfig"/> implementation contains a <see cref="SendingProcessingMode"/> for a given <paramref name="id"/>
        /// </summary>
        /// <param name="id">The Sending Processing Mode id for which the verification is done.</param>
        /// <returns></returns>
        public bool ContainsSendingPMode(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public SendingProcessingMode GetSendingPMode(string id)
        {
            string pmodePath = Path.Combine(Environment.CurrentDirectory, "config", "send-pmodes");
            return Directory.GetFiles(pmodePath, "*.xml")
                            .Select(File.ReadAllText)
                            .Select(AS4XmlSerializer.FromString<SendingProcessingMode>)
                            .FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Return all the installed Receiving Processing Modes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReceivingProcessingMode> GetReceivingPModes()
        {
            string pmodePath = Path.Combine(Environment.CurrentDirectory, "config", "receive-pmodes");
            return Directory.GetFiles(pmodePath, "*.xml")
                            .Select(File.ReadAllText)
                            .Select(AS4XmlSerializer.FromString<ReceivingProcessingMode>);
        }

        /// <summary>
        /// Gets the configuration of the Minder Test-Agents that are enabled.
        /// </summary>        
        /// <returns></returns>        
        /// <remarks>For every SettingsMinderAgent that is returned, a special Minder-Agent will be instantiated.</remarks>
        public IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AgentConfig> GetAgentsConfiguration()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the retry polling interval for which the Retry Agent will poll 
        /// for 'to-be-retried' messages/exceptions for a delivery or notification operation.
        /// </summary>
        public TimeSpan RetryPollingInterval => throw new NotImplementedException();
    }
}
