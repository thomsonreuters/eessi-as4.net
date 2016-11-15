using System;
using System.IO;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// File Retriever Implementation to retrieve the FileStream of a local file
    /// </summary>
    public class FilePayloadRetriever : IPayloadRetriever
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePayloadRetriever"/> class
        /// Create a new <see cref="IPayloadRetriever"/> implementation
        /// to retrieve payloads from the file system
        /// </summary>
        public FilePayloadRetriever()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Retrieve the payload from the given location
        /// </summary>
        /// <param name="location"> The location</param>
        /// <exception cref="Exception"> </exception>
        /// <returns> </returns>
        public Stream RetrievePayload(string location)
        {
            Stream payloadStream = TryRetrievePayload(location);
            this._logger.Info($"Payload is successfully retrieved at location: {location}");
            return payloadStream;
        }

        private Stream TryRetrievePayload(string location)
        {
            try
            {
                return RetrievePayloadAtlocation(location);
            }
            catch (UriFormatException exception)
            {
                throw ThrowAS4PayloadException(location, exception);
            }
            catch (IOException exception)
            {
                throw ThrowAS4PayloadException(location, exception);
            }
        }

        private AS4Exception ThrowAS4PayloadException(string location, Exception exception)
        {
            string description = $"Unable to retrieve Payload at location: {location}";
            this._logger.Error(description);
            
            return new AS4ExceptionBuilder()
                .WithInnerException(exception)
                .WithDescription(description)
                .WithErrorCode(ErrorCode.Ebms0011)
                .WithExceptionType(ExceptionType.ExternalPayloadError)
                .Build();
        }

        private Stream RetrievePayloadAtlocation(string location)
        {
            string relativePath = location.Replace("file:///", string.Empty);
            string absolutePath = Path.GetFullPath(relativePath);
            var uri = new Uri(absolutePath);

            return new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read);
        }
    }
}