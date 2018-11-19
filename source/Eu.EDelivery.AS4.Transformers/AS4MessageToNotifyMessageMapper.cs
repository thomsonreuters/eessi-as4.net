using System;
using System.Linq;
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
            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

            if (receivedEntityType == null)
            {
                throw new ArgumentNullException(nameof(receivedEntityType));
            }

            SignalMessage firstNonPrSignalMessage =
                as4Message.SignalMessages.FirstOrDefault(s => !(s is PullRequest));

            Status status = 
                typeof(ExceptionEntity).IsAssignableFrom(receivedEntityType)
                    ? Status.Exception
                    : firstNonPrSignalMessage is Receipt
                        ? Status.Delivered
                        : Status.Error;

            return new NotifyMessage
            {
                MessageInfo =
                {
                    MessageId = firstNonPrSignalMessage?.MessageId,
                    RefToMessageId = firstNonPrSignalMessage?.RefToMessageId
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