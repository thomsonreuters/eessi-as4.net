using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class PseudoConfig : IConfig
    {
        /// <summary>
        /// Initialize Configuration
        /// </summary>
        public virtual void Initialize()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets or sets a value indicating whether if the Configuration is IsInitialized
        /// </summary>
        public bool IsInitialized { get; protected set; }

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
        public virtual IEnumerable<SettingsAgent> GetSettingsAgents()
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
    }
}
