using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class PseudoConfig : IConfig
    {
        public static PseudoConfig Default => new PseudoConfig();

        /// <summary>
        /// Initialize Configuration
        /// </summary>
        public virtual void Initialize()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether if the Configuration is IsInitialized
        /// </summary>
        public virtual bool IsInitialized { get; } = false;

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key">Registered Key for the Setting</param>
        /// <returns></returns>
        public virtual string GetSetting(string key)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Verify if the <see cref="IConfig"/> implementation contains a <see cref="SendingProcessingMode"/> for a given <paramref name="id"/>
        /// </summary>
        /// <param name="id">The Sending Processing Mode id for which the verification is done.</param>
        /// <returns></returns>
        public virtual bool ContainsSendingPMode(string id)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public virtual SendingProcessingMode GetSendingPMode(string id)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Return all the installed Receiving Processing Modes
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ReceivingProcessingMode> GetReceivingPModes()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the settings agents.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual IEnumerable<AgentSettings> GetSettingsAgents()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the configuration of the Minder Test-Agents that are enabled.
        /// </summary>        
        /// <returns></returns>        
        /// <remarks>For every SettingsMinderAgent that is returned, a special Minder-Agent will be instantiated.</remarks>
        public virtual IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the in message store location.
        /// </summary>
        /// <value>The in message store location.</value>
        public string InMessageStoreLocation
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// Gets the out message store location.
        /// </summary>
        /// <value>The out message store location.</value>
        public string OutMessageStoreLocation
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class PseudeConfigFacts : PseudoConfig
    {
        [Fact]
        public void FailsToInitialize()
        {
            Assert.False(Default.IsInitialized);
            Assert.ThrowsAny<Exception>(() => Initialize());
        }

        [Fact]
        public void FailsToGetAgents()
        {
            Assert.ThrowsAny<Exception>(GetSettingsAgents);
            Assert.ThrowsAny<Exception>(GetEnabledMinderTestAgents);
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
            Assert.ThrowsAny<Exception>(() => InMessageStoreLocation);
            Assert.ThrowsAny<Exception>(() => OutMessageStoreLocation);
        }
    }
}
