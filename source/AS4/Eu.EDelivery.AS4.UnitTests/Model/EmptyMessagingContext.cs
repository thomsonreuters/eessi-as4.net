using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Empty <see cref="MessagingContext"/> implementation.
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Model.Internal.MessagingContext" />
    public class EmptyMessagingContext : MessagingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyMessagingContext"/> class.
        /// </summary>
        public EmptyMessagingContext() : base(as4Message: null) {}
    }
}
