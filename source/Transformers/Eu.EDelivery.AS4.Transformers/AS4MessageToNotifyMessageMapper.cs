using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Singletons;

namespace Eu.EDelivery.AS4.Transformers
{
    internal static class AS4MessageToNotifyMessageMapper
    {
        internal static NotifyMessage Convert(AS4Message as4Message)
        {
            var notifyMessage = AS4Mapper.Map<NotifyMessage>(as4Message.PrimarySignalMessage);

            notifyMessage.StatusInfo.Any = GetOriginalSignalMessageSignature(as4Message);

            return notifyMessage;
        }

        private static XmlElement[] GetOriginalSignalMessageSignature(AS4Message as4Message)
        {
            const string xpath = "//*[local-name()='SignalMessage']";
            XmlNode nodeSignature = as4Message.EnvelopeDocument.SelectSingleNode(xpath);

            return new[] { (XmlElement)nodeSignature };
        }
    }
}