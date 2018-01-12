using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AS4.ParserService.Models
{
    public class DecodeResult
    {
        /// <summary>
        /// The <see cref="EbmsMessageType"/> of the Received Message.
        /// </summary>
        public EbmsMessageType ReceivedMessageType { get; set; }

        /// <summary>
        /// The ebMS Id of the received AS4 Message
        /// </summary>
        public string ReceivedEbmsMessageId { get; set; }

        /// <summary>
        /// The ebMS Id of the message that has been created
        /// </summary>
        /// <remarks>This is typically Error or Receipt since the Decode service creates
        /// the signal-message that must be used to respond to a received UserMessage.</remarks>
        public string ResponseEbmsMessageId { get; set; }

        /// <summary>
        /// The Message that must be used as a Response
        /// </summary>
        public byte[] ResponseMessage { get; set; }

        /// <summary>
        /// The <see cref="EbmsMessageType"/> of the ResponseMessage that has been created.
        /// </summary>
        public EbmsMessageType ResponseMessageType { get; set; }

        /// <summary>
        /// The FileName that can be used to save the created response message
        /// </summary>
        public string ResponseMessageFileName { get; set; }

        /// <summary>
        /// The Attachments that were present as a payload in the received message.
        /// </summary>
        public PayloadInfo[] Payloads { get; set; }

        public static DecodeResult CreateForBadRequest()
        {
            return new DecodeResult
            {
                ReceivedMessageType = EbmsMessageType.Unknown,
                ResponseMessage = new byte[] { },
                ResponseMessageType = EbmsMessageType.Unknown,
                Payloads = new PayloadInfo[] { }
            };
        }

        public static DecodeResult CreateAccepted(EbmsMessageType messageType, string receivedEbmsMessageId)
        {
            return new DecodeResult
            {
                ResponseMessageType = EbmsMessageType.Unknown,
                ReceivedMessageType = messageType,
                ResponseMessage = new byte[] { },
                Payloads = new PayloadInfo[] { },
                ReceivedEbmsMessageId = receivedEbmsMessageId
            };
        }

        public static DecodeResult CreateWithReceipt(IEnumerable<PayloadInfo> receivedPayloads, byte[] responseMessage, string receivedEbmsMessageId, string receiptEbmsMessageId)
        {
            return new DecodeResult
            {
                ReceivedMessageType = EbmsMessageType.UserMessage,
                ReceivedEbmsMessageId = receivedEbmsMessageId,
                Payloads = receivedPayloads.ToArray(),
                ResponseMessage = responseMessage,
                ResponseEbmsMessageId = receiptEbmsMessageId,
                ResponseMessageType = EbmsMessageType.Receipt,
                ResponseMessageFileName = $"{receiptEbmsMessageId}.receipt"
            };
        }

        public static DecodeResult CreateWithError(byte[] responseMessage, string receivedEbmsMessageId, string errorEbmsMessageId)
        {
            return new DecodeResult()
            {
                ResponseMessage = responseMessage,
                ResponseEbmsMessageId = errorEbmsMessageId,
                ReceivedMessageType = EbmsMessageType.UserMessage,
                ResponseMessageType = EbmsMessageType.Error,
                ReceivedEbmsMessageId = receivedEbmsMessageId,
                ResponseMessageFileName = $"{errorEbmsMessageId}.error",
                Payloads = new PayloadInfo[] { }
            };
        }
    }
}