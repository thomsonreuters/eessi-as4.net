using System;
using System.Linq;

namespace AS4.ParserService.Models
{

    /// <summary>
    /// Represents a payload / attachment that must be included in an AS4 Message.
    /// </summary>
    public class PayloadInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadInfo"/> class.
        /// </summary>
        public PayloadInfo()
        {
        }

        public PayloadInfo(string payloadId, string contentType, byte[] content)
        {
            PayloadName = payloadId;
            ContentType = contentType;
            Content = content;
        }

        /// <summary>
        /// The name of the payload. 
        /// </summary>
        public string PayloadName { get; set; }

        /// <summary>
        /// The filename that can be used to save the payload on a filesystem.
        /// </summary>
        public string PayloadFilename
        {
            get
            {
                if (String.IsNullOrWhiteSpace(ContentType))
                {
                    return PayloadName;
                }

                // TODO: refactor, put in another class and make sure that text/xml is also handled,
                // since the method below doesn't cover it.
                var extension = new MimeSharp.Mime().Extension(ContentType).FirstOrDefault();

                if (extension == null)
                {
                    return PayloadName;
                }

                return $"{PayloadName}{extension}";
            }

        }

        /// <summary>
        /// The content-type of the payload.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The content of the payload.
        /// </summary>
        public byte[] Content { get; set; }
    }
}