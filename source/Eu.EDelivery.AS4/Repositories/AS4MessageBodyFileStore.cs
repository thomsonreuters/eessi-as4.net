using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;

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
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _provider = provider;
        }

        /// <summary>
        /// Saves an AS4 Message instance to the filesystem.
        /// </summary>
        /// <remarks>The AS4 Message is being serialized to file.</remarks>
        /// <param name="location"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string SaveAS4Message(string location, AS4Message message)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string storeLocation = EnsureStoreLocation(location);
            string fileName = AssembleUniqueMessageLocation(storeLocation);

            if (!File.Exists(fileName))
            {
                SaveMessageToFile(message, fileName);
            }

            return $"file:///{fileName}";
        }

        /// <summary>
        /// Saves an AS4 Message Stream to the filesystem.
        /// </summary>
        /// <param name="location">The location where the AS4 message must be saved</param>
        /// <param name="as4MessageStream">A stream representing the AS4 message</param>
        /// <returns></returns>
        public async Task<string> SaveAS4MessageStreamAsync(string location, Stream as4MessageStream)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (as4MessageStream == null)
            {
                throw new ArgumentNullException(nameof(as4MessageStream));
            }

            string storeLocation = EnsureStoreLocation(location);
            string fileName = AssembleUniqueMessageLocation(storeLocation);

            if (!File.Exists(fileName))
            {
                var sourceFile = GetFileStreamForSourceStream(as4MessageStream);

                if (sourceFile != null)
                {
                    File.Copy(sourceFile.Name, fileName);
                }
                else
                {
                    using (FileStream fs = FileUtils.OpenAsync(fileName, FileMode.Create, FileAccess.Write, FileOptions.SequentialScan))
                    {
                        File.SetAttributes(fs.Name, FileAttributes.NotContentIndexed);

                        await as4MessageStream.CopyToFastAsync(fs).ConfigureAwait(false);
                    }
                }
            }

            return $"file:///{fileName}";
        }

        private static FileStream GetFileStreamForSourceStream(Stream s)
        {
            if (s is FileStream fs)
            {
                return fs;
            }

            if (s is VirtualStream vs && vs.UnderlyingStream is FileStream ufs)
            {
                return ufs;
            }

            return null;
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
        /// Updates an existing file on the file-system with an updated version
        /// of the given AS4 Message instance.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="message"></param>
        public void UpdateAS4Message(string location, AS4Message message)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

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

            SaveMessageToFile(message, fileLocation);
        }

        private void SaveMessageToFile(AS4Message message, string fileName)
        {
            using (FileStream content = File.Create(fileName))
            {
                File.SetAttributes(fileName, FileAttributes.NotContentIndexed);

                ISerializer serializer = _provider.Get(message.ContentType);
                serializer.Serialize(message, content, CancellationToken.None);
            }
        }

        /// <summary>
        /// Loads a <see cref="Stream" /> at a given stored <paramref name="location" />.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public async Task<Stream> LoadMessageBodyAsync(string location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            string fileLocation = SubstringWithoutFileUri(location);

            if (string.IsNullOrEmpty(fileLocation))
            {
                return null;
            }

            if (File.Exists(fileLocation))
            {
                using (FileStream fileStream = FileUtils.OpenReadAsync(fileLocation, options: FileOptions.SequentialScan))
                {
                    VirtualStream virtualStream =
                        VirtualStream.Create(
                            fileStream.CanSeek ? fileStream.Length : VirtualStream.ThresholdMax,
                            forAsync: true);

                    await fileStream.CopyToFastAsync(virtualStream).ConfigureAwait(false);
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