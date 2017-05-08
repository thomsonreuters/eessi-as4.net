using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Interface to provide <see cref="ISerializer"/> implementations
    /// </summary>
    public interface ISerializerProvider
    {
        ISerializer Get(string contentType);
    }

    /// <summary>
    /// Class to provide <see cref="ISerializer"/> implementations
    /// </summary>
    public class SerializerProvider : ISerializerProvider
    {
        private readonly IDictionary<string, ISerializer> _serializers;

        public static ISerializerProvider Default = new SerializerProvider();

        internal SerializerProvider()
        {
            _serializers = new Dictionary<string, ISerializer>();

            var soapSerializer = new SoapEnvelopeSerializer();
            _serializers.Add(Constants.ContentTypes.Soap, soapSerializer);
            _serializers.Add(Constants.ContentTypes.Mime, new MimeMessageSerializer(soapSerializer));
        }

        /// <summary>
        /// Get the <see cref="ISerializer"/> implementation
        /// based on a given Content Type
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public ISerializer Get(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentException(@"No content-type specified", nameof(contentType));
            }

            foreach (string key in _serializers.Keys)
            {
                if (KeyMatchesContentType(contentType, key))
                {
                    return _serializers[key];
                }
            }

            throw new AS4Exception($"No given Serializer found for a given Content Type: {contentType}");
        }

        private static bool KeyMatchesContentType(string contentType, string key)
        {
            return key.Equals(contentType) || contentType.StartsWith(key, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
