using System.IO;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// <see cref="ReceivedMessage"/> to receive a <see cref="Entity"/>
    /// </summary>
    public class ReceivedEntityMessage : ReceivedMessage
    {
        public IEntity Entity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedEntityMessage"/> class. 
        /// Create a new <see cref="ReceivedEntityMessage"/>
        /// with a required <see cref="Entity"/>
        /// </summary>
        /// <param name="entity">
        /// </param>
        public ReceivedEntityMessage(IEntity entity) : this(entity, Stream.Null, string.Empty)
        {
        }

        public ReceivedEntityMessage(IEntity entity, Stream underlyingStream, string contentType) : base(underlyingStream, contentType)
        {
            this.Entity = entity;
        }
    }
}