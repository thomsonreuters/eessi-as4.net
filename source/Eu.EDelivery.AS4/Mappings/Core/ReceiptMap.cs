using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Xml;
using NonRepudiationInformation = Eu.EDelivery.AS4.Model.Core.NonRepudiationInformation;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    internal class ReceiptMap
    {
        private static readonly XmlSerializer NonRepudiationSerializer = 
            new XmlSerializer(typeof(Xml.NonRepudiationInformation));

        /// <summary>
        /// Maps from a domain model representation to a XML representation of an AS4 receipt.
        /// </summary>
        /// <param name="model">The domain model to convert.</param>
        internal static Xml.SignalMessage Convert(Receipt model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return new Xml.SignalMessage
            {
                MessageInfo = new MessageInfo
                {
                    MessageId = model.MessageId,
                    RefToMessageId = model.RefToMessageId,
                    Timestamp = model.Timestamp.LocalDateTime
                },
                Receipt = new Xml.Receipt
                {
                    UserMessage = 
                        model.UserMessage != null 
                            ? UserMessageMap.Convert(model.UserMessage) 
                            : null,
                    NonRepudiationInformation = 
                        model.NonRepudiationInformation != null
                            ? MapNonRepudiationInformation(model.NonRepudiationInformation)
                            : null
                }
            };
        }

        private static Xml.NonRepudiationInformation MapNonRepudiationInformation(NonRepudiationInformation model)
        {
            MessagePartNRInformation MapPartNRInformation(Reference r)
            {
                return new MessagePartNRInformation
                {
                    Item = new ReferenceType
                    {
                        URI = r.URI,
                        DigestMethod = new DigestMethodType { Algorithm = r.DigestMethod.Algorithm },
                        DigestValue = r.DigestValue,
                        Transforms = r.Transforms.Select(t => new TransformType { Algorithm = t.Algorithm }).ToArray()
                    }
                };
            }

            return new Xml.NonRepudiationInformation
            {
                MessagePartNRInformation = model.MessagePartNRIReferences.Select(MapPartNRInformation).ToArray()
            };
        }

        /// <summary>
        /// Maps from a XML representation with an optional routing usermessage to a domain model representation of an AS4 receipt.
        /// </summary>
        /// <param name="xml">The XML representation to convert.</param>
        /// <param name="routingM">The optional routing usermessage element to include in the to be created receipt.</param>
        public static Receipt Convert(Xml.SignalMessage xml, Maybe<RoutingInputUserMessage> routingM)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            if (routingM == null)
            {
                throw new ArgumentNullException(nameof(routingM));
            }

            if (xml.Receipt == null)
            {
                throw new ArgumentException(
                    @"Cannot create Receipt domain model from a XML representation without a Receipt element",
                    nameof(xml.Receipt));
            }

            string messageId = xml.MessageInfo?.MessageId;
            string refToMessageId = xml.MessageInfo?.RefToMessageId;
            DateTimeOffset timestamp = xml.MessageInfo?.Timestamp.ToDateTimeOffset() ?? DateTimeOffset.Now;

            Maybe<NonRepudiationInformation> nriM = GetNonRepudiationFromXml(xml.Receipt);
            Maybe<UserMessage> userM = GetUserMessageFromXml(xml.Receipt);

            Maybe<Receipt> routingNriReceiptM =
                routingM.Zip(nriM, (routing, nri) => new Receipt(messageId, refToMessageId, timestamp, nri, routing));

            Maybe<Receipt> routingUserReceiptM =
                routingM.Zip(userM, (routing, user) => new Receipt(messageId, refToMessageId, timestamp, user, routing));

            Maybe<Receipt> routingReceipt = 
                routingM.Select(routing => new Receipt(messageId, refToMessageId, timestamp, includedUserMessage: null, routedUserMessage: routing));

            Maybe<Receipt> nriReceipt = 
                nriM.Select(nri => new Receipt(messageId, refToMessageId, timestamp, nri, routedUserMessage: null));

            Maybe<Receipt> userReceipt = 
                userM.Select(user => new Receipt(messageId, refToMessageId, timestamp, user, routedUserMessage: null));

            return routingNriReceiptM
                .OrElse(routingUserReceiptM)
                .OrElse(routingReceipt)
                .OrElse(nriReceipt)
                .OrElse(userReceipt)
                .GetOrElse(() => new Receipt(messageId, refToMessageId, timestamp, includedUserMessage: null, routedUserMessage: null));
        }

        private static Maybe<NonRepudiationInformation> GetNonRepudiationFromXml(Xml.Receipt r)
        {
            XmlElement firstNrrElement = r.Any?.FirstOrDefault();

            if (firstNrrElement != null
                && firstNrrElement.LocalName.IndexOf(
                    "NonRepudiationInformation",
                    StringComparison.OrdinalIgnoreCase) > -1)
            {
                object deserialize = NonRepudiationSerializer.Deserialize(new XmlNodeReader(firstNrrElement));
                return Maybe.Just(MapNonRepudiationInformation((Xml.NonRepudiationInformation) deserialize));
            }

            if (r.NonRepudiationInformation != null)
            {
                return Maybe.Just(MapNonRepudiationInformation(r.NonRepudiationInformation));
            }

            return Maybe<NonRepudiationInformation>.Nothing;
        }

        private static NonRepudiationInformation MapNonRepudiationInformation(Xml.NonRepudiationInformation xml)
        {
            if (xml.MessagePartNRInformation == null)
            {
                return new NonRepudiationInformation(new Reference[0]);
            }

            Reference MapReference(ReferenceType r)
            {
                return new Reference(
                    r.URI,
                    r.Transforms?.Select(t => new ReferenceTransform(t.Algorithm)).ToArray(),
                    new ReferenceDigestMethod(r.DigestMethod?.Algorithm),
                    r.DigestValue);
            }

            IEnumerable<Reference> references =
                xml.MessagePartNRInformation
                   .Select(p => p.Item)
                   .Where(i => i != null)
                   .Cast<ReferenceType>()
                   .Select(MapReference)
                   .ToArray();

            return new NonRepudiationInformation(references);
        }

        private static Maybe<UserMessage> GetUserMessageFromXml(Xml.Receipt r)
        {
            if (r.UserMessage == null)
            {
                return Maybe.Nothing<UserMessage>();
            }

            return Maybe.Just(UserMessageMap.Convert(r.UserMessage));
        }
    }
}
