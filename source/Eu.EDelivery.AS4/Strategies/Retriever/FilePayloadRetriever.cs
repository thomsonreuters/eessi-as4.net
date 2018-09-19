using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// File Retriever Implementation to retrieve the FileStream of a local file
    /// </summary>
    public class FilePayloadRetriever : IPayloadRetriever
    {
        public const string Key = "file:///";

        private readonly IConfig _config;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePayloadRetriever"/> class.
        /// </summary>
        public FilePayloadRetriever() : this(Config.Instance) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FilePayloadRetriever"/> class.
        /// </summary>
        public FilePayloadRetriever(IConfig configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _config = configuration;
        }

        /// <summary>
        /// Retrieve <see cref="Stream"/> contents from a given <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public Task<Stream> RetrievePayloadAsync(string location)
        {
            string relativePayloadPath = location.Replace(Key, string.Empty);
            string absolutePayloadPath = Path.GetFullPath(Path.Combine(Config.ApplicationPath, relativePayloadPath));

            var payload = new FileInfo(absolutePayloadPath);

            string relativeRetrievalPath = _config.PayloadRetrievalLocation.Replace(Key, string.Empty);
            string absoluteRetrievalPath = Path.GetFullPath(relativeRetrievalPath);

            if (payload.Directory?.FullName != absoluteRetrievalPath)
            {
                throw new NotSupportedException(
                    $"Only files from the '{_config.PayloadRetrievalLocation}' folder are allowed to be retrieved: {payload.Directory?.FullName} <> {absoluteRetrievalPath}");
            }

            var uri = new Uri(absolutePayloadPath);
            Stream payloadStream = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            Logger.Debug($"Payload is successfully retrieved at location \"{location}\"");
            return Task.FromResult(payloadStream);
        }
    }
}