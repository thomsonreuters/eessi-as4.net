using System.IO;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;

namespace Eu.EDelivery.AS4.UnitTests.Model.Internal
{
    public class SaboteurReceivedMessage : ReceivedMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaboteurReceivedMessage"/> class.
        /// </summary>
        public SaboteurReceivedMessage() : base(Stream.Null)
        {
        }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage" />
        /// </summary>
        /// <param name="messagingContext"></param>
        public override void AssignPropertiesTo(MessagingContext messagingContext)
        {
            throw new SaboteurException("Sabotage assignment of properties");
        }
    }
}
