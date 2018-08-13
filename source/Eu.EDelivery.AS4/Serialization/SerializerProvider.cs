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
        /// <summary>
        /// Gets the specific <see cref="ISerializer"/> implementation based on the content type.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns></returns>
        ISerializer Get(string contentType);
    }

    /// <summary>
    /// Class to provide <see cref="ISerializer"/> implementations
    /// </summary>
    public class SerializerProvider : ISerializerProvider
    {
        private static readonly ISerializerProvider DefaultProvider = new SerializerProvider();
        private readonly IDictionary<string, ISerializer> _serializers;

        /// <summary>
        /// Gets the default.
        /// </summary><value>The default.
        /// </value>
        public static ISerializerProvider Default => DefaultProvider;

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

            throw new KeyNotFoundException($"No given Serializer found for a given Content Type: {contentType}");
        }

        private static bool KeyMatchesContentType(string contentType, string key)
        {
            return key.Equals(contentType) || contentType.StartsWith(key, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
