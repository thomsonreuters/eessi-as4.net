namespace Eu.EDelivery.AS4.PayloadService.Models
{
    public class UploadResult
    {
        public string PayloadId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadResult"/> class.
        /// </summary>
        public UploadResult(string payloadId)
        {
            this.PayloadId = payloadId;
        }
    }
}