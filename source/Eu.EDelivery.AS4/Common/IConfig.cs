using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Common
{
    public interface IConfig
    {
        // TODO: add typed properties for mandatory configuration items ? (IdFormat, Database connectionstring, etc...) ?

        /// <summary>
        /// Initializes the specified settings file name.
        /// </summary>
        /// <param name="settingsFileName">Name of the settings file.</param>
        void Initialize(string settingsFileName);

        /// <summary>
        /// Gets a value indicating whether if the Configuration is IsInitialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the retention period (in days) for which the stored entities are cleaned-up.
        /// </summary>
        /// <value>The retention period in days.</value>
        TimeSpan RetentionPeriod { get; }

        /// <summary>
        /// Gets the location where the payloads should be retrieved.
        /// </summary>
        string PayloadRetrievalLocation { get; }

        /// <summary>
        /// Gets the location path where the messages during an incoming operation are stored.
        /// </summary>
        string InMessageStoreLocation { get; }

        /// <summary>
        /// Gets the location path where the messages during an outgoing operation are stored.
        /// </summary>
        string OutMessageStoreLocation { get; }

        /// <summary>
        /// Gets the location path where the exceptions during an incoming operation are stored.
        /// </summary>
        string InExceptionStoreLocation { get; }

        /// <summary>
        /// Gets the location path where the exceptions during an outgoing operation are stored.
        /// </summary>
        string OutExceptionStoreLocation { get; }

        /// <summary>
        /// Gets the format in which Ebms Message Identifiers should be generated.
        /// </summary>
        string EbmsMessageIdFormat { get; }

        /// <summary>
        /// Gets the configured database provider.
        /// </summary>
        string DatabaseProvider { get; }

        /// <summary>
        /// Gets the configured connection string to contact the database.
        /// </summary>
        string DatabaseConnectionString { get; }

        /// <summary>
        /// Gets the confgured certificate store name.
        /// </summary>
        string CertificateStore { get; }

        /// <summary>
        /// Gets the configured certificate repository type.
        /// </summary>
        string CertificateRepositoryType { get; }

        /// <summary>
        /// Gets the file path from where the authorization entries to verify PullRequests should be stored.
        /// </summary>
        string AuthorizationMapPath { get; }

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key">Registered Key for the Setting</param>
        /// <returns></returns>
        string GetSetting(string key);

        /// <summary>
        /// Verify if the <see cref="IConfig"/> implementation contains a <see cref="SendingProcessingMode"/> for a given <paramref name="id"/>
        /// </summary>
        /// <param name="id">The Sending Processing Mode id for which the verification is done.</param>
        /// <returns></returns>
        bool ContainsSendingPMode(string id);

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        SendingProcessingMode GetSendingPMode(string id);

        /// <summary>
        /// Return all the installed Receiving Processing Modes
        /// </summary>
        /// <returns></returns>
        IEnumerable<ReceivingProcessingMode> GetReceivingPModes();

        /// <summary>
        /// Gets the configuration of the Minder Test-Agents that are enabled.
        /// </summary>        
        /// <returns></returns>        
        /// <remarks>For every SettingsMinderAgent that is returned, a special Minder-Agent will be instantiated.</remarks>
        IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents();

        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        IEnumerable<AgentConfig> GetAgentsConfiguration();

        /// <summary>
        /// Gets the retry polling interval for which the Retry Agent will poll 
        /// for 'to-be-retried' messages/exceptions for a delivery or notification operation.
        /// </summary>
        TimeSpan RetryPollingInterval { get; }
    }

    public enum PropertyType
    {
        Optional,
        Mandatory,
        None
    }
}