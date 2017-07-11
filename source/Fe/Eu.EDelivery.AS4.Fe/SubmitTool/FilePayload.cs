using System;
using Eu.EDelivery.AS4.Model.Common;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Class to hold the message payloads
    /// </summary>
    public class FilePayload
    {
        /// <summary>
        /// Gets or sets the type of the MIME.
        /// </summary>
        /// <value>
        /// The type of the MIME.
        /// </value>
        public string MimeType { get; set; }
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location { get; set; }
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Convert this class to a payload class
        /// </summary>
        /// <param name="payloadId">The payload identifier.</param>
        /// <returns></returns>
        public Payload ToPayload(string payloadId)
        {
            if (payloadId == null) throw new ArgumentNullException(nameof(payloadId), "Cannot be empty!");

            return new Payload
            {
                MimeType = MimeType,
                Location = Location,
                Id = payloadId
            };
        }
    }
}