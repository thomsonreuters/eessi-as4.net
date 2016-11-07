using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// <see cref="ReceivedMessage"/> to receive a <see cref="Entity"/>
    /// </summary>
    public class ReceivedEntityMessage : ReceivedMessage
    {
        public Entity Entity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedEntityMessage"/> class. 
        /// Create a new <see cref="ReceivedEntityMessage"/>
        /// with a required <see cref="Entity"/>
        /// </summary>
        /// <param name="entity">
        /// </param>
        public ReceivedEntityMessage(Entity entity)
        {
            this.Entity = entity;
        }
    }
}