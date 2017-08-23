using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Create a Stubbed Config for the tests
    /// </summary>
    public class StubConfig : PseudoConfig
    {
        private readonly IDictionary<string, string> _configuration;
        private readonly IDictionary<string, ReceivingProcessingMode> _receivingPmodes;
        private readonly IDictionary<string, SendingProcessingMode> _sendingPModes;

        public static readonly StubConfig Default = new StubConfig();
       
        /// <summary>
        /// Initializes a new instance of the <see cref="StubConfig"/> class.
        /// </summary>
        private StubConfig()
            : this(sendingPModes: new Dictionary<string, SendingProcessingMode>(),
                   receivingPModes: new Dictionary<string, ReceivingProcessingMode>())
        {
        }

        private static Dictionary<string, string> GetDefaultConfigSettings()
        {
            return new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
            {
                ["IdFormat"] = "{GUID}",
                ["Provider"] = "InMemory",
                ["ConnectionString"] = @"Filename=database\messages.db",
                ["CertificateStore"] = "My"
            };
        }

        public StubConfig(IDictionary<string, SendingProcessingMode> sendingPModes,
                          IDictionary<string, ReceivingProcessingMode> receivingPModes) :
            this(configSettings: GetDefaultConfigSettings(),
                sendingPModes: sendingPModes,
                receivingPModes: receivingPModes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StubConfig" /> class.
        /// </summary>
        /// <param name="configSettings"></param>
        /// <param name="sendingPModes"></param>
        /// <param name="receivingPModes"></param>
        public StubConfig(IDictionary<string, string> configSettings,
                          IDictionary<string, SendingProcessingMode> sendingPModes,
                          IDictionary<string, ReceivingProcessingMode> receivingPModes)
        {
            _configuration = configSettings;
            _sendingPModes = sendingPModes;
            _receivingPmodes = receivingPModes;
        }

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
            if (_sendingPModes.ContainsKey(id) == false)
            {
                return null;
            }

            return _sendingPModes[id];
        }
        
        /// <summary>
        /// Verify if the configuration implementation contains a <see cref="SendingProcessingMode"/> for a given <paramref name="id"/>
        /// </summary>
        /// <param name="id">The Sending Processing Mode id for which the verification is done.</param>
        /// <returns></returns>
        public override bool ContainsSendingPMode(string id)
        {
            return _sendingPModes.ContainsKey(id);
        }
    }
}