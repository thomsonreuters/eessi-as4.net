using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;

namespace AS4.ParserService.Models
{
    public class DecodeResult
    {
        /// <summary>
        /// The <see cref="EbmsMessageType"/> of the Received Message.
        /// </summary>
        public EbmsMessageType ReceivedMessageType { get; set; }

        /// <summary>
        /// When the decoded message is an Error signalmessage, this property
        /// contains the Error-details that are present in the ErrorMessage
        /// </summary>
        public string ErrorInformation { get; set; }

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

        public static DecodeResult CreateAccepted(EbmsMessageType messageType, string receivedEbmsMessageId, IEnumerable<ErrorLine> receivedErrorDetails)
        {
            string receivedErrorInformation = null;
            if (receivedErrorDetails != null && receivedErrorDetails.Any())
            {
                receivedErrorInformation = CreateErrorInformationString(receivedErrorDetails);
            }

            return new DecodeResult
            {
                ResponseMessageType = EbmsMessageType.Unknown,
                ErrorInformation = receivedErrorInformation,
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

        public static DecodeResult CreateWithError(byte[] responseMessage, IEnumerable<ErrorLine> errorDetails, string receivedEbmsMessageId, string errorEbmsMessageId)
        {
            return new DecodeResult()
            {
                ResponseMessage = responseMessage,
                ErrorInformation = CreateErrorInformationString(errorDetails),
                ResponseEbmsMessageId = errorEbmsMessageId,
                ReceivedMessageType = EbmsMessageType.UserMessage,
                ResponseMessageType = EbmsMessageType.Error,
                ReceivedEbmsMessageId = receivedEbmsMessageId,
                ResponseMessageFileName = $"{errorEbmsMessageId}.error",
                Payloads = new PayloadInfo[] { }
            };
        }

        private static string CreateErrorInformationString(IEnumerable<ErrorLine> receivedErrorDetails)
        {
            if (receivedErrorDetails == null || receivedErrorDetails.Any() == false)
            {
                return null;
            }

            var errors = new List<Eu.EDelivery.AS4.Xml.Error>();

            foreach (var errorDetail in receivedErrorDetails)
            {
                errors.Add(AS4Mapper.Map<Eu.EDelivery.AS4.Xml.Error>(errorDetail));
            }

            var serialized = AS4XmlSerializer.ToString(errors);

            if (!String.IsNullOrWhiteSpace(serialized))
            {
                var errorDocument = new XmlDocument();
                errorDocument.LoadXml(serialized);
                return errorDocument.SelectSingleNode("ArrayOfError")?.InnerXml;
            }

            return null;
        }
    }
}