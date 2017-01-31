using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;

namespace Eu.EDelivery.AS4.ServiceHandler.Builder
{
    /// <summary>
    /// Builder to make <see cref="IReceiver"/> implementations
    /// from <see cref="Receiver"/> settings
    /// </summary>
    public class ReceiverBuilder
    {
        private Receiver _settingReceiver;

        /// <summary>
        /// Set the configued <see cref="Receiver"/> settings
        /// </summary>
        /// <param name="settingReceiver"></param>
        /// <returns></returns>
        public ReceiverBuilder SetSettings(Receiver settingReceiver)
        {
            this._settingReceiver = settingReceiver;
            return this;
        }

        /// <summary>
        /// Build the <see cref="IReceiver"/> implementation
        /// </summary>
        /// <returns></returns>
        public IReceiver Build()
        {
            var receiver = new GenericTypeBuilder().SetType(this._settingReceiver.Type).Build<IReceiver>();
            ConfigureReceiverWithSettings(receiver, this._settingReceiver);

            return receiver;
        }

        private static void ConfigureReceiverWithSettings(IReceiver receiver, Receiver settingsReceiver)
        {
            if (settingsReceiver.Setting == null) return;

            Dictionary<string, string> dictionary = settingsReceiver.Setting
                .ToDictionary(setting => setting.Key, setting => setting.Value);

            receiver.Configure(dictionary);
        }
    }
}
