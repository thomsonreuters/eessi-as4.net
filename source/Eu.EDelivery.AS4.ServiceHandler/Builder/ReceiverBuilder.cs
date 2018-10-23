using System;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;

namespace Eu.EDelivery.AS4.ServiceHandler.Builder
{
    /// <summary>
    /// Builder to make <see cref="IReceiver"/> implementations
    /// from <see cref="Receiver"/> settings
    /// </summary>
    internal class ReceiverBuilder
    {
        private Receiver _settingReceiver;

        /// <summary>
        /// Set the configued <see cref="Receiver"/> settings
        /// </summary>
        /// <param name="settingReceiver"></param>
        /// <returns></returns>
        public ReceiverBuilder SetSettings(Receiver settingReceiver)
        {
            if (settingReceiver == null)
            {
                throw new ArgumentNullException(nameof(settingReceiver));
            }

            _settingReceiver = settingReceiver;
            return this;
        }

        /// <summary>
        /// Build the <see cref="IReceiver"/> implementation
        /// </summary>
        /// <returns></returns>
        public IReceiver Build()
        {
            if (!GenericTypeBuilder.CanResolveTypeImplementedBy<IReceiver>(_settingReceiver.Type))
            {
                throw new InvalidOperationException(
                    $"Cannot resolve a valid {nameof(IReceiver)} implementation for the {_settingReceiver.Type} fully-qualified assembly name");
            }

            var receiver = GenericTypeBuilder.FromType(_settingReceiver.Type).Build<IReceiver>();

            if (_settingReceiver.Setting != null)
            {
                receiver.Configure(_settingReceiver.Setting);
            }

            return receiver;
        }
    }
}