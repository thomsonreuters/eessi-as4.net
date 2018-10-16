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
            if (message == null)
            {
                return new byte[]{};
            }

            using (var stream = new MemoryStream())
            {
                var serializer = SerializerProvider.Default.Get(message.ContentType);
                serializer.Serialize(message, stream);

                return stream.ToArray();
            }
        }
    }
}