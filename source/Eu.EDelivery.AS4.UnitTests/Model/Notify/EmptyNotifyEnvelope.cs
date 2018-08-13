using System;
using Eu.EDelivery.AS4.Model.Notify;

namespace Eu.EDelivery.AS4.UnitTests.Model.Notify
{
    internal class EmptyNotifyEnvelope : NotifyMessageEnvelope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyNotifyEnvelope" /> class.
        /// </summary>
        /// <param name="refToMessageId">The reference to message identifier.</param>
        public EmptyNotifyEnvelope(string refToMessageId)
            : base(new MessageInfo {RefToMessageId = refToMessageId}, default(Status), null, null, default(Type)) {}
    }
}