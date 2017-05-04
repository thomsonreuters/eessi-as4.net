using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Repositories
{
    internal class AS4MessageBodyFilePersister : IAS4MessageBodyPersister
    {
        private readonly ISerializerProvider _provider;
        private readonly string _storeLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4MessageBodyFilePersister"/> class.
        /// </summary>
        /// <param name="storeLocation">The store Location.</param>
        /// <param name="provider">The provider.</param>
        public AS4MessageBodyFilePersister(string storeLocation, ISerializerProvider provider)
        {
            _storeLocation = storeLocation;
            _provider = provider;

            if (Directory.Exists(_storeLocation) == false)
            {
                Directory.CreateDirectory(_storeLocation);
            }
        }

        public async Task<string> SaveAS4MessageAsync(AS4Message message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string messageId = message.GetPrimaryMessageId();

            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new AS4Exception("The AS4Message to store has no Primary Message Id");
            }

            string fileName = Path.Combine(_storeLocation, $"{messageId}.as4");

            if (!File.Exists(fileName))
            {
                using (FileStream fs = File.Create(fileName))
                {
                    ISerializer serializer = _provider.Get(message.ContentType);
                    await serializer.SerializeAsync(message, fs, cancellationToken);
                }
            }

            return $"file://{fileName}";
        }
    }

    public interface IAS4MessageBodyPersister
    {
        Task<string> SaveAS4MessageAsync(AS4Message message, CancellationToken cancellationToken);
    }
}