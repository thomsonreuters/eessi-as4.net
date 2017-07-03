using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Repositories
{
    internal class AS4MessageBodyFileStore : IAS4MessageBodyStore
    {
        private readonly ISerializerProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4MessageBodyFileStore" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public AS4MessageBodyFileStore(ISerializerProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Saves a given <see cref="AS4Message" /> to a given location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message to save.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns>
        /// Location where the <paramref name="message" /> is saved.
        /// </returns>
        /// <exception cref="ArgumentNullException">message</exception>
        public async Task<string> SaveAS4MessageAsync(
            string location,
            AS4Message message,
            CancellationToken cancellation)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string storeLocation = EnsureStoreLocation(location);
            string fileName = AssembleUniqueMessageLocation(storeLocation);

            if (!File.Exists(fileName))
            {
                await SaveMessageToFile(message, fileName, cancellation);
            }

            return $"file:///{fileName}";
        }

        public async Task<string> SaveAS4MessageStreamAsync(string location, Stream as4MessageStream, CancellationToken cancellation)
        {
            if (as4MessageStream == null)
            {
                throw new ArgumentNullException(nameof(as4MessageStream));
            }

            string storeLocation = EnsureStoreLocation(location);
            string fileName = AssembleUniqueMessageLocation(storeLocation);

            if (!File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    await as4MessageStream.CopyToAsync(fs);
                }
            }

            return $"file:///{fileName}";
        }

        private static string EnsureStoreLocation(string storeLocation)
        {
            string location = SubstringWithoutFileUri(storeLocation);

            if (Directory.Exists(location) == false)
            {
                Directory.CreateDirectory(location);
            }

            return location;
        }

        private static string AssembleUniqueMessageLocation(string storeLocation)
        {
            string fileName = Guid.NewGuid().ToString();

            return Path.Combine(storeLocation, $"{fileName}.as4");
        }

        /// <summary>
        /// Updates a s4 message asynchronous.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">message</exception>
        /// <exception cref="FileNotFoundException">The messagebody that must be updated could not be found.</exception>
        public async Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellation)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string fileLocation = SubstringWithoutFileUri(location);

            if (!File.Exists(fileLocation))
            {
                throw new FileNotFoundException(
                    $"The messagebody that must be updated could not be found at: {fileLocation}.");
            }

            await SaveMessageToFile(message, fileLocation, cancellation);
        }

        private async Task SaveMessageToFile(AS4Message message, string fileName, CancellationToken cancellationToken)
        {
            using (FileStream content = File.Create(fileName))
            {
                ISerializer serializer = _provider.Get(message.ContentType);
                await serializer.SerializeAsync(message, content, cancellationToken);
            }
        }

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public async Task<Stream> LoadMessageBodyAsync(string location)
        {
            string fileLocation = SubstringWithoutFileUri(location);

            if (string.IsNullOrEmpty(fileLocation))
            {
                return null;
            }

            if (File.Exists(fileLocation))
            {
                using (FileStream fileStream = File.OpenRead(fileLocation))
                {
                    VirtualStream virtualStream =
                        VirtualStream.CreateVirtualStream(
                            fileStream.CanSeek ? fileStream.Length : VirtualStream.ThresholdMax);

                    await fileStream.CopyToAsync(virtualStream);
                    virtualStream.Position = 0;

                    return virtualStream;
                }
            }

            return null;
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
}