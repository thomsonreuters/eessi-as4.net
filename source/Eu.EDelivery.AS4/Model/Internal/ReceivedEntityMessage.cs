using System;
using System.IO;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// <see cref="ReceivedMessage"/> to receive a <see cref="Entity"/>
    /// </summary>
    public class ReceivedEntityMessage : ReceivedMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedEntityMessage"/> class. 
        /// </summary>
        /// <param name="entity"> </param>
        public ReceivedEntityMessage(Entity entity) 
            : this(entity, Stream.Null, string.Empty) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedEntityMessage"/> class. 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="contentType"></param>
        public ReceivedEntityMessage(Entity entity, string contentType) 
            : this(entity, Stream.Null, contentType) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedEntityMessage"/> class. 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="underlyingStream"></param>
        /// <param name="contentType"></param>
        public ReceivedEntityMessage(Entity entity, Stream underlyingStream, string contentType) : base(underlyingStream, contentType)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Entity = entity;
        }

        public Entity Entity { get; }
    }
}