using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace AS4.ParserService.Infrastructure
{
    internal class Serializer
    {
        public static byte[] ToByteArray(AS4Message message)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = SerializerProvider.Default.Get(message.ContentType);
                serializer.Serialize(message, stream, CancellationToken.None);

                return stream.ToArray();
            }
        }
    }
}