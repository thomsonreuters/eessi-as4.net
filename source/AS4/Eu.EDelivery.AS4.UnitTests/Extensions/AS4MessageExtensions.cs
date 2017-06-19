using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="AS4Message"/>
    /// </summary>
    public static class AS4MessageExtensions
    {
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

        /// <summary>
        /// Serialize the content as a SOAP Envelope.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static async Task<AS4Message> SoapSerialize(this string content)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var serializer = new SoapEnvelopeSerializer();
                return await serializer.DeserializeAsync(stream, Constants.ContentTypes.Soap, CancellationToken.None);
            }
        }
    }
}
