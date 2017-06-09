using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Create a Stubbed Config for the tests
    /// </summary>
    public class StubConfig : PseudoConfig
    {
        private static readonly StubConfig Singleton = new StubConfig();
        private IDictionary<string, string> _configuration;
        private IDictionary<string, ReceivingProcessingMode> _receivingPmodes;
        private IDictionary<string, SendingProcessingMode> _sendingPModes;

        private StubConfig()
        {
            SetupSendingPModes();
            SetupReceivingPModes();
            SetupConfiguration();
        }

        private void SetupSendingPModes()
        {
            _sendingPModes = new Dictionary<string, SendingProcessingMode>
            {
                ["01-send"] = AS4XmlSerializer.FromString<SendingProcessingMode>(Properties.Resources.send_01)
            };
        }

        private void SetupReceivingPModes()
        {
            _receivingPmodes = new Dictionary<string, ReceivingProcessingMode>
            {
                ["01-receive"] = AS4XmlSerializer.FromString<ReceivingProcessingMode>(Properties.Resources.receive_01)
            };
        }

        private void SetupConfiguration()
        {
            _configuration = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
            {
                ["IdFormat"] = "{GUID}",
                ["Provider"] = "InMemory",
                ["ConnectionString"] = @"Filename=database\messages.db",
                ["CertificateStore"] = "My"
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StubConfig" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public StubConfig(IDictionary<string, string> configuration)
        {
            _configuration = configuration;
        }
        
        public static IConfig Instance => Singleton;

        /// <summary>
        /// Verify if the Configuration is IsInitialized
        /// </summary>
        public override bool IsInitialized => true;

        /// <summary>
        /// Retrieve Setting from the Global Configurations
        /// </summary>
        /// <param name="key">Registerd Key for the Setting</param>
        /// <returns></returns>
        public override string GetSetting(string key)
        {
            return _configuration[key];
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override SendingProcessingMode GetSendingPMode(string id)
        {
            return _sendingPModes[id];
        }

        /// <summary>
        /// Verify if the <see cref="IConfig"/> implementation contains a <see cref="SendingProcessingMode"/> for a given <paramref name="id"/>
        /// </summary>
        /// <param name="id">The Sending Processing Mode id for which the verification is done.</param>
        /// <returns></returns>
        public override bool ContainsSendingPMode(string id)
        {
            return _sendingPModes.ContainsKey(id);
        }
    }
}