using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="AS4Message"/>
    /// </summary>
    public static class AS4MessageExtensions
    {
        public static AS4Message EmptyAS4Message => new AS4MessageBuilder().Build();

        /// <summary>
        /// Serialize a given <paramref name="message"/> to a Stream instance.
        /// </summary>
        /// <param name="message">Given message to serialize.</param>
        /// <returns></returns>
        public static MemoryStream ToStream(this AS4Message message)
        {
            var memoryStream = new MemoryStream();

            ISerializerProvider provider = new SerializerProvider();
            ISerializer serializer = provider.Get(message.ContentType);
            serializer.Serialize(message, memoryStream, CancellationToken.None);

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
