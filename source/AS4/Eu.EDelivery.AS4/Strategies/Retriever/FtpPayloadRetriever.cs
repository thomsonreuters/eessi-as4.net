using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// FTP Retriever Implementation to retrieve the FileStream of a FTP Server
    /// </summary>
    public class FtpPayloadRetriever : IPayloadRetriever
    {
        private readonly IConfig _config;
        private readonly ILogger _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpPayloadRetriever"/> class
        /// </summary>
        public FtpPayloadRetriever()
        {
            this._config = Config.Instance;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Retriee 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Stream RetrievePayload(string location)
        {
            return TryGetFtpFile(location);
        }

        private Stream TryGetFtpFile(string location)
        {
            try
            {
                FtpWebRequest ftpRequest = CreateFtpRequest(location);
                return Task.Run(() => GetFtpFile(ftpRequest)).Result;
            }
            catch (Exception exception)
            {
                throw ThrowAS4PayloadException(location, exception);
            }
        }

        private FtpWebRequest CreateFtpRequest(string location)
        {
            var ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(location));
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            ftpRequest.Credentials = new NetworkCredential(
                this._config.GetSetting("ftpusername"),
                this._config.GetSetting("ftppassword"));

            return ftpRequest;
        }

        private async Task<Stream> GetFtpFile(FtpWebRequest ftpRequest)
        {
            using (WebResponse ftpResponse = await ftpRequest.GetResponseAsync())
                return ftpResponse.GetResponseStream();
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
    }
}
