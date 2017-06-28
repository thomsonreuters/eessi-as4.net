using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;

namespace Eu.EDelivery.AS4.UnitTests.Model.Deliver
{
    internal class EmptyDeliverEnvelope : DeliverMessageEnvelope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyDeliverEnvelope" /> class.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        public EmptyDeliverEnvelope(string messageId) : base(new MessageInfo(messageId, null), null, null) {}
    }
}