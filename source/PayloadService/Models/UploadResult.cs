namespace Eu.EDelivery.AS4.PayloadService.Models
{
    public class UploadResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadResult"/> class.
        /// </summary>
        /// <param name="payloadId">The payload Id.</param>
        /// <param name="downloadUrl">The download Url.</param>
        public UploadResult(string payloadId, string downloadUrl)
        {
            PayloadId = payloadId;
            DownloadUrl = downloadUrl;
        }

        public string PayloadId { get; }

        public string DownloadUrl { get; }
    }
}