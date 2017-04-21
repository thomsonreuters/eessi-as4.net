using System;
using System.IO;
using System.Threading.Tasks;
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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Retrieve <see cref="Stream"/> contents from a given <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public Task<Stream> RetrievePayloadAsync(string location)
        {
            return Task.FromResult(RetrievePayload(location));
        }

        private static Stream RetrievePayload(string location)
        {
            Stream payloadStream = TryRetrievePayload(location);

            Logger.Info($"Payload is successfully retrieved at location: {location}");

            return payloadStream;
        }

        private static Stream TryRetrievePayload(string location)
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

        private static AS4Exception ThrowAS4PayloadException(string location, Exception exception)
        {
            string description = $"Unable to retrieve Payload at location: {location}";

            Logger.Error(description);
            
            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(exception)                
                .WithErrorCode(ErrorCode.Ebms0011)
                .WithErrorAlias(ErrorAlias.ExternalPayloadError)
                .Build();
        }

        private static Stream RetrievePayloadAtlocation(string location)
        {
            string relativePath = location.Replace("file:///", string.Empty);
            string absolutePath = Path.GetFullPath(relativePath);
            var uri = new Uri(absolutePath);            
            
            return new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}