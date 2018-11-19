using System;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Transformers
{
    internal static class AS4MessageToNotifyMessageMapper
    {
        internal static NotifyMessage Convert(AS4Message as4Message, Type receivedEntityType)
        {
            Status status = 
                typeof(ExceptionEntity).IsAssignableFrom(receivedEntityType)
                    ? Status.Exception
                    : as4Message.FirstSignalMessage is Receipt
                        ? Status.Delivered
                        : Status.Error;

            return new NotifyMessage
            {
                MessageInfo =
                {
                    MessageId = as4Message.FirstSignalMessage.MessageId,
                    RefToMessageId = as4Message.FirstSignalMessage.RefToMessageId
                },
                StatusInfo =
                {
                    Status = status,
                    Any = GetOriginalSignalMessage(as4Message)
                }
            };
        }

        private static XmlElement[] GetOriginalSignalMessage(AS4Message as4Message)
        {
            // TODO: more strict XPath selection?
            const string xpath = "//*[local-name()='SignalMessage']";

            XmlDocument messageEnvelope = 
                as4Message.EnvelopeDocument 
                ?? AS4XmlSerializer.ToSoapEnvelopeDocument(as4Message, CancellationToken.None);

            return new[] { (XmlElement) messageEnvelope.SelectSingleNode(xpath) };
        }
    }
}