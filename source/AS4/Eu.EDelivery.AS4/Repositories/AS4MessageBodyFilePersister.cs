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

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4MessageBodyFilePersister" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public AS4MessageBodyFilePersister(ISerializerProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Saves a given <see cref="AS4Message" /> to a given location.
        /// </summary>
        /// <param name="storeLocation">The store location.</param>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Location where the <paramref name="message" /> is saved.
        /// </returns>
        public async Task<string> SaveAS4MessageAsync(
            string storeLocation,
            AS4Message message,
            CancellationToken cancellation)
        {
            string location = SubstringWithoutFileUri(storeLocation);

            if (Directory.Exists(location) == false)
            {
                Directory.CreateDirectory(location);
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string fileName = AssembleUniqueMessageLocation(location, message);

            if (!File.Exists(fileName))
            {
                await SaveMessageToFile(message, fileName, cancellation);
            }

            return $"file:///{fileName}";
        }

        private static string AssembleUniqueMessageLocation(string storeLocation, AS4Message message)
        {
            string messageId = message.GetPrimaryMessageId();

            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new AS4Exception("The AS4Message to store has no Primary Message Id");
            }

            return Path.Combine(storeLocation, $"{messageId}.as4");
        }

        /// <summary>
        /// Updates an existing AS4 Message body.
        /// </summary>
        /// <param name="location">The location where the existing AS4Message body can be found.</param>
        /// <param name="message">The message that should overwrite the existing messagebody.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UpdateAS4MessageAsync(
            string location,
            AS4Message message,
            CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // TODO: this is a quickfix.  If we should support multiple implementations of the IAS4MessageBodyPersister
            // then the InMessageService should be reponsible for retrieving the correct implementation that should
            // be used to overwrite.
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

        private async Task SaveMessageToFile(AS4Message message, string fileName, CancellationToken cancellationToken)
        {
            using (FileStream fs = File.Create(fileName))
            {
                ISerializer serializer = _provider.Get(message.ContentType);
                await serializer.SerializeAsync(message, fs, cancellationToken);
            }
        }

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location on which the <see cref="Stream" /> is stored.</param>
        /// <returns></returns>
        public Stream LoadAS4MessageStream(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            string absoluteLocation = SubstringWithoutFileUri(location);

            if (!File.Exists(absoluteLocation))
            {
                return null;
            }

            return File.OpenRead(absoluteLocation);
        }

        private static string SubstringWithoutFileUri(string location)
        {
            if (location.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
            {
                return location.Substring("file:///".Length);
            }

            return location;
        }
    }

    public interface IAS4MessageBodyPersister : IAS4MessageBodyRetriever
    {
        /// <summary>
        /// Saves a given <see cref="AS4Message" /> to a given location.
        /// </summary>
        /// <param name="storeLocation">The store location.</param>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Location where the <paramref name="message" /> is saved.
        /// </returns>
        Task<string> SaveAS4MessageAsync(string storeLocation, AS4Message message, CancellationToken cancellation);

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