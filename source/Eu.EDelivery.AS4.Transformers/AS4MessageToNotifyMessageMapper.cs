using System;
using System.Xml;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;

namespace Eu.EDelivery.AS4.Transformers
{
    internal static class AS4MessageToNotifyMessageMapper
    {
        internal static NotifyMessage Convert(
            SignalMessage tobeNotifiedSignal,
            Type receivedEntityType,
            XmlDocument soapEnvelope)
        {
            if (tobeNotifiedSignal == null)
            {
                throw new ArgumentNullException(
                    nameof(tobeNotifiedSignal), 
                    @"No SignalMessage found to create a NotifyMessage from");
            }

            if (soapEnvelope == null)
            {
                throw new ArgumentNullException(
                    nameof(soapEnvelope),
                    @"No SOAP envelope document found to include in the NotifyMessage");
            }

            Status status =
                typeof(ExceptionEntity).IsAssignableFrom(receivedEntityType)
                    ? Status.Exception
                    : tobeNotifiedSignal is Receipt
                        ? Status.Delivered
                        : Status.Error;

            string xpath = $"//eb:SignalMessage[eb:MessageInfo/eb:MessageId[text()='{tobeNotifiedSignal.MessageId}']]";
            var ns = new XmlNamespaceManager(soapEnvelope.NameTable);
            ns.AddNamespace("eb", Constants.Namespaces.EbmsXmlCore);

            return new NotifyMessage
            {
                MessageInfo =
                {
                    MessageId = tobeNotifiedSignal.MessageId,
                    RefToMessageId = tobeNotifiedSignal.RefToMessageId
                },
                StatusInfo =
                {
                    Status = status,
                    Any = new[] { (XmlElement) soapEnvelope.SelectSingleNode(xpath, ns) }
                }
            };
        }
    }
}