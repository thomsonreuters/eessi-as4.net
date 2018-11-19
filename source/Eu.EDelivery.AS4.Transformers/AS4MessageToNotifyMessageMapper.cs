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

            XmlElement[] originalSignalMessage = GetOriginalSignalMessage(as4Message);

            if (as4Message.IsPullRequest)
            {
                return new NotifyMessage
                {
                    MessageInfo = { RefToMessageId = as4Message.FirstSignalMessage.MessageId },
                    StatusInfo =
                    {
                        Status = typeof(ExceptionEntity).IsAssignableFrom(receivedEntityType)
                            ? Status.Exception
                            : Status.Error,
                        Any = originalSignalMessage
                    }
                };
            }

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