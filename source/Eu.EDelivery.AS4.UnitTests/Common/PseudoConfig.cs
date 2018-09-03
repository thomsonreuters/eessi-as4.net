using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class PseudoConfig : IConfig
    {
        /// <summary>
        /// Initialize Configuration
        /// </summary>
        public virtual void Initialize(string settingsFileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether if the Configuration is IsInitialized
        /// </summary>
        public virtual bool IsInitialized { get; } = false;

        /// <summary>
        /// Gets the retention period (in days) for which the stored entities are cleaned-up.
        /// </summary>
        /// <value>The retention period in days.</value>
        public virtual TimeSpan RetentionPeriod => throw new NotImplementedException();

        /// <summary>
        /// Gets the retry polling interval for which the Retry Agent will poll 
        /// for 'to-be-retried' messages/exceptions for a delivery or notification operation.
        /// </summary>
        public virtual TimeSpan RetryPollingInterval => throw new NotImplementedException();

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key">Registered Key for the Setting</param>
        /// <returns></returns>
        public virtual string GetSetting(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verify if the <see cref="IConfig"/> implementation contains a <see cref="SendingProcessingMode"/> for a given <paramref name="id"/>
        /// </summary>
        /// <param name="id">The Sending Processing Mode id for which the verification is done.</param>
        /// <returns></returns>
        public virtual bool ContainsSendingPMode(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public virtual SendingProcessingMode GetSendingPMode(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return all the installed Receiving Processing Modes
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ReceivingProcessingMode> GetReceivingPModes()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the configuration of the Minder Test-Agents that are enabled.
        /// </summary>        
        /// <returns></returns>        
        /// <remarks>For every SettingsMinderAgent that is returned, a special Minder-Agent will be instantiated.</remarks>
        public virtual IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the in message store location.
        /// </summary>
        /// <value>The in message store location.</value>
        public string InMessageStoreLocation
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the out message store location.
        /// </summary>
        /// <value>The out message store location.</value>
        public string OutMessageStoreLocation
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the location where the payloads should be retrieved.
        /// </summary>
        public virtual string PayloadRetrievalLocation => throw new NotImplementedException();

        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AgentConfig> GetAgentsConfiguration()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the IAuthorizationMapProvider that must be used when verifying PullRequests.
        /// </summary>
        /// <returns></returns>
        public IPullAuthorizationMapProvider PullRequestAuthorizationMapProvider
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class PseudoConfigFacts : PseudoConfig
    {
        [Fact]
        public void FailsToInitialize()
        {
            Assert.False(new PseudoConfig().IsInitialized);
            Assert.ThrowsAny<Exception>(() =>
            {
                Initialize("settings.xml");
            });
        }

        [Fact]
        public void FailsToGetAgents()
        {
            Assert.ThrowsAny<Exception>(GetEnabledMinderTestAgents);
            Assert.ThrowsAny<Exception>(GetAgentsConfiguration);
        }

        [Fact]
        public void FailsToGetPModes()
        {
            Assert.ThrowsAny<Exception>(GetReceivingPModes);
            Assert.ThrowsAny<Exception>(() => GetSendingPMode("ignored string"));
            Assert.ThrowsAny<Exception>(() => ContainsSendingPMode("ignored string"));
        }

        [Fact]
        public void FailsToGetSetting()
        {
            Assert.ThrowsAny<Exception>(() => GetSetting("ignored string"));
        }
    }
}
