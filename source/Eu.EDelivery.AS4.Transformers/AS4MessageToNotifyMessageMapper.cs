using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Transformers
{
    internal static class AS4MessageToNotifyMessageMapper
    {
        internal static NotifyMessage Convert(AS4Message as4Message)
        {
            var notifyMessage = AS4Mapper.Map<NotifyMessage>(as4Message.FirstSignalMessage);

            notifyMessage.StatusInfo.Any = GetOriginalSignalMessage(as4Message);

            return notifyMessage;
        }

        private static XmlElement[] GetOriginalSignalMessage(AS4Message as4Message)
        {
            const string xpath = "//*[local-name()='SignalMessage']";

            var messageEnvelope = as4Message.EnvelopeDocument;

            if (messageEnvelope == null)
            {
                messageEnvelope = AS4XmlSerializer.ToSoapEnvelopeDocument(as4Message, CancellationToken.None);
            }

            XmlNode nodeSignature = messageEnvelope.SelectSingleNode(xpath);

            return new[] { (XmlElement)nodeSignature };
        }
    }
}