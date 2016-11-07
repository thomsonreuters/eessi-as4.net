using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Null Object for the <see cref="InternalMessage"/> Model
    /// </summary>
    public class NullInternalMessage : InternalMessage
    {
        public static InternalMessage Instance { get; } = new NullInternalMessage();
    }
}
