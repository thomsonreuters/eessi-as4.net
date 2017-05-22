using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Utilities;

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

        /// <summary>
        /// Saves a given <see cref="AS4Message"/> to a given location.
        /// </summary>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellationToken">The Cancellation.</param>
        /// <returns>Location where the <paramref name="message"/> is saved.</returns>
        public async Task<string> SaveAS4MessageAsync(AS4Message message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string fileName = AssembleStoreLocationWith(message);

            if (!File.Exists(fileName))
            {
                await SaveMessageToFile(message, fileName, cancellationToken);
            }

            return $"file:///{fileName}";
        }

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location where the existing AS4Message body can be found.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // TODO: this is a quickfix.  If we should support multiple implementations of the IAS4MessageBodyPersister
            //       then the InMessageService should be reponsible for retrieving the correct implementation that should
            //       be used to overwrite.
            if (location.StartsWith("file:///"))
            {
                location = location.Substring(8);
            }
            
            if (File.Exists(location) == false)
            {
                throw new FileNotFoundException("The messagebody that must be updated could not be found.");
            }

            await SaveMessageToFile(message, location, cancellationToken);
        }

        private string AssembleStoreLocationWith(AS4Message message)
        {

#if DEBUG
            string messageId = message.GetPrimaryMessageId();

            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new AS4Exception("The AS4Message to store has no Primary Message Id");
            }

            string fileName = FilenameSanitizer.EnsureValidFilename(messageId);
#else
            string fileName = Guid.NewGuid().ToString();
#endif

            return Path.Combine(_storeLocation, $"{fileName}.as4");
        }

        private async Task SaveMessageToFile(AS4Message message, string fileName, CancellationToken cancellationToken)
        {
            using (FileStream fs = File.Create(fileName))
            {
                ISerializer serializer = _provider.Get(message.ContentType);
                await serializer.SerializeAsync(message, fs, cancellationToken);
            }
        }
    }

    public interface IAS4MessageBodyPersister
    {
        /// <summary>
        /// Saves a given <see cref="AS4Message"/> to a given location.
        /// </summary>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellationToken">The Cancellation.</param>
        /// <returns>Location where the <paramref name="message"/> is saved.</returns>
        Task<string> SaveAS4MessageAsync(AS4Message message, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location where the existing AS4Message body can be found.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken);
    }
}