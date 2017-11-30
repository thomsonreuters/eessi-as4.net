namespace AS4.ParserService.Models
{
    public class EncodeResult
    {
        /// <summary>
        /// The URL to where the AS4 Message must be sent to.
        /// </summary>
        public string SendToUrl { get; set; }

        /// <summary>
        /// The AS4Message that must be sent.
        /// </summary>
        public byte[] AS4Message { get; set; }

        /// <summary>
        /// The ContentType of the AS4Message.
        /// </summary>
        /// <remarks>This value must be used as the value of the Content-Type HTTP header when sending the AS4 Message</remarks>
        public string ContentType { get; set; }
    }
}