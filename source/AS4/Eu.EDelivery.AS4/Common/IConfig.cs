using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;
using Eu.EDelivery.AS4.Watchers;

namespace Eu.EDelivery.AS4.Common
{
    using System;

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
        /// Gets the in message store location.
        /// </summary>
        /// <value>The in message store location.</value>
        string InMessageStoreLocation { get; }

        /// <summary>
        /// Gets the out message store location.
        /// </summary>
        /// <value>The out message store location.</value>
        string OutMessageStoreLocation { get; }

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
        /// Gets the IAuthorizationMapProvider that must be used when verifying PullRequests.
        /// </summary>
        /// <returns></returns>
        IPullAuthorizationMapProvider PullRequestAuthorizationMapProvider { get; }

        /// <summary>
        /// Gets the receiving p mode watcher.
        /// </summary>
        /// <value>The receiving p mode watcher.</value>
        PModeWatcher<ReceivingProcessingMode> ReceivingPModeWatcher { get; }

        /// <summary>
        /// Gets the sending p mode watcher.
        /// </summary>
        /// <value>The sending p mode watcher.</value>
        PModeWatcher<SendingProcessingMode> SendingPModeWatcher { get; }
    }

    public enum PropertyType
    {
        Optional,
        Mandatory,
        None
    }
}