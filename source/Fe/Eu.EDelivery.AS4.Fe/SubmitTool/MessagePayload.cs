using System.Collections.Generic;
using HttpMultipartParser;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Class that holds the parameters for the submit tool
    /// </summary>
    public class MessagePayload
    {
        /// <summary>
        /// Gets or sets the sending pmode which is used to build the message.
        /// </summary>
        /// <value>
        /// The sending pmode.
        /// </value>
        public string SendingPmode { get; set; } = "8.1.1-pmode";
        /// <summary>
        /// Gets or sets the number of submit messages to submit.
        /// When more than 1 message is supplied then the other messages will just be a copy.
        /// </summary>
        /// <value>
        /// The number of submit messages.
        /// </value>
        public int NumberOfSubmitMessages { get; set; } = 10;
        /// <summary>
        /// Gets or sets the files to add as payload.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public IList<FilePart> Files { get; set; }
    }
}