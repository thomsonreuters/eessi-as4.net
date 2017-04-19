using System;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Repositories
{
    internal class AS4MessageBodyFilePersister : IAS4MessageBodyPersister
    {
        private readonly string _storeLocation;
        private readonly ISerializerProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4MessageBodyFilePersister"/> class.
        /// </summary>
        public AS4MessageBodyFilePersister(string storeLocation, ISerializerProvider provider)
        {
            _storeLocation = storeLocation;
            _provider = provider;

            if (Directory.Exists(_storeLocation) == false)
            {
                Directory.CreateDirectory(_storeLocation);
            }
        }

        public string SaveAS4Message(AS4Message message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string messageId = message.GetPrimaryMessageId();

            if (String.IsNullOrWhiteSpace(messageId))
            {
                throw new AS4Exception("The AS4Message to store has no Primary Message Id");
            }

            string fileName = Path.Combine(_storeLocation, $"{messageId}.as4");

            using (var fs = File.Create(fileName))
            {
                var serializer = _provider.Get(message.ContentType);
                serializer.Serialize(message, fs, cancellationToken);
            }

            return $"file://{fileName}";
        }
    }

    public interface IAS4MessageBodyPersister
    {
        string SaveAS4Message(AS4Message message, CancellationToken cancellationToken);
    }

}