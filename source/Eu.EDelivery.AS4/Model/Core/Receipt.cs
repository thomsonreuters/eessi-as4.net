using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Xml;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// ebMS signal message unit representing a successful response to an ebMS <see cref="UserMessage"/>
    /// </summary>
    public class Receipt : SignalMessage
    {
        /// <summary>
        /// The <see cref="Core.UserMessage"/> for which this is a receipt.
        /// </summary>
        /// <value>The user message.</value>
        /// <remarks>This property should only be populated when the NonRepudiationInformation is not filled out.</remarks>
        public UserMessage UserMessage { get; }

        /// <summary>
        /// Non-Repudiation information of the UserMessage for which this is a receipt.
        /// </summary>
        /// <value>The non repudiation information.</value>
        /// <remarks>This property is only populated when the UserMessage property is not filled out.</remarks>
        public NonRepudiationInformation NonRepudiationInformation { get; }

        /// <summary>
        /// Gets the multi-hop action value.
        /// </summary>
        public override string MultihopAction { get; } = Constants.Namespaces.EbmsOneWayReceipt;

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        public Receipt(string messageId, string refToMessageId)
            : base(messageId, refToMessageId, DateTimeOffset.Now) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="timestamp">The timestamp when this receipt is created.</param>
        /// <param name="includedUserMessage">The <see cref="Core.UserMessage"/> for which this receipt is created.</param>
        /// <param name="routedUserMessage">The <see cref="Core.UserMessage"/> to include in the receipt in the form of a RoutingInput element.</param>
        internal Receipt(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            UserMessage includedUserMessage,
            RoutingInputUserMessage routedUserMessage)
            : base(messageId, refToMessageId, timestamp, routedUserMessage)
        {
            UserMessage = includedUserMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="nonRepudiation">The non-repudiation information containing the signed references of the <see name="Core.UserMessage"/>.</param>
        internal Receipt(
            string messageId,
            string refToMessageId,
            NonRepudiationInformation nonRepudiation) 
            : this(messageId, refToMessageId, nonRepudiation, routedUserMessage: null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="nonRepudiation">The non-repudiation information containing the signed references of the <see name="Core.UserMessage"/>.</param>
        /// <param name="routedUserMessage">The <see cref="Core.UserMessage"/> to include in the receipt in the form of a RoutingInput element.</param>
        internal Receipt(
            string messageId,
            string refToMessageId,
            NonRepudiationInformation nonRepudiation,
            RoutingInputUserMessage routedUserMessage)
            : this(messageId, refToMessageId, DateTimeOffset.Now, nonRepudiation, routedUserMessage) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="timestamp">The timestamp when this receipt is created.</param>
        /// <param name="nonRepudiation">The non-repudiation information containing the signed references of the <see name="Core.UserMessage"/>.</param>
        /// <param name="routedUserMessage">The <see cref="Core.UserMessage"/> to include in the receipt in the form of a RoutingInput element.</param>
        internal Receipt(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            NonRepudiationInformation nonRepudiation,
            RoutingInputUserMessage routedUserMessage)
            : base(messageId, refToMessageId, timestamp, routedUserMessage)
        {
            NonRepudiationInformation = nonRepudiation;
        }

        /// <summary>
        /// Creates a non-repudiation AS4 receipt that references a given <paramref name="includedUserMessage"/>.
        /// </summary>
        /// <param name="receiptMessageId"></param>
        /// <param name="includedUserMessage">The <see cref="Core.UserMessage"/> for which this receipt is created.</param>
        /// <param name="userMessageSecurityHeader">The security header to retrieve the signed references from to include in the receipt.</param>
        /// <param name="userMessageSendViaMultiHop">
        /// Whether or not the user message was send in a multi-hop fashion or not.
        /// Setting this on <c>true</c> will result in a receipt with the referencing user message included in a RoutingInput element.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="includedUserMessage"/> should not be <c>null</c>.</exception>
        public static Receipt CreateFor(
            string receiptMessageId,
            UserMessage includedUserMessage,
            SecurityHeader userMessageSecurityHeader,
            bool userMessageSendViaMultiHop = false)
        {
            if (includedUserMessage == null)
            {
                throw new ArgumentNullException(nameof(includedUserMessage));
            }

            if (userMessageSecurityHeader != null)
            {
                IEnumerable<CryptoReference> signedReferences = userMessageSecurityHeader.GetReferences();

                if (signedReferences.Any())
                {
                    var nonRepudiation = 
                        new NonRepudiationInformation(
                            signedReferences.Select(Reference.CreateFromReferenceElement));

                    return userMessageSendViaMultiHop.ThenMaybe(UserMessageMap.ConvertToRouting(includedUserMessage))
                        .Select(routing => new Receipt(receiptMessageId, includedUserMessage?.MessageId, nonRepudiation, routing))
                        .GetOrElse(() => new Receipt(receiptMessageId, includedUserMessage?.MessageId, nonRepudiation, routedUserMessage: null)); 
                }
            }

            return CreateFor(receiptMessageId, includedUserMessage, userMessageSendViaMultiHop);
        }

        /// <summary>
        /// Creates an AS4 receipt that references a given <paramref name="includedUserMessage"/>.
        /// </summary>
        /// <param name="receiptMessageId"></param>
        /// <param name="includedUserMessage">The <see cref="Core.UserMessage"/> for which this receipt is created.</param>
        /// <param name="userMessageSendViaMultiHop">
        ///     Whether or not the user message was send in a multi-hop fashion or not.
        ///     Setting this on <c>true</c> will result in a receipt with the referencing user message included in a RoutingInput element.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="includedUserMessage"/> should not be <c>null</c>.</exception>
        public static Receipt CreateFor(
            string receiptMessageId,
            UserMessage includedUserMessage,
            bool userMessageSendViaMultiHop = false)
        {
            if (includedUserMessage == null)
            {
                throw new ArgumentNullException(nameof(includedUserMessage));
            }

            return userMessageSendViaMultiHop.ThenMaybe(UserMessageMap.ConvertToRouting(includedUserMessage))
                .Select(routing => new Receipt(receiptMessageId, includedUserMessage.MessageId, DateTimeOffset.Now, includedUserMessage, routing))
                .GetOrElse(() => new Receipt(receiptMessageId, includedUserMessage.MessageId, DateTimeOffset.Now, includedUserMessage, routedUserMessage: null));
        }

        /// <summary>
        /// Verifies the non-repudiation information of the <see cref="Receipt"/> against the NRI of the related <see cref="Core.UserMessage"/>.
        /// </summary>
        /// <param name="userMessage">The related <see cref="Core.UserMessage"/>.</param>
        /// <returns>
        ///     <c>true</c> when the non-repudiation information signed references matches with the given signed references of the user message.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="userMessage"/> should not be <c>null</c>.</exception>
        public bool VerifyNonRepudiationInfo(AS4Message userMessage)
        {
            if (userMessage == null)
            {
                throw new ArgumentNullException(nameof(userMessage));
            }

            byte[] GetNonRepudiationHashForUri(string userMessageReferenceUri)
            {
                return NonRepudiationInformation
                       .MessagePartNRIReferences
                       .Where(r => r.URI.Equals(userMessageReferenceUri))
                       .Select(p => p.DigestValue)
                       .FirstOrDefault();
            }

            bool IsNonRepudiationHashEqualToUserReferenceHash(CryptoReference r)
            {
                byte[] repudiationHash = GetNonRepudiationHashForUri(r.Uri);
                return repudiationHash != null && r.DigestValue?.SequenceEqual(repudiationHash) == true;
            }

            IEnumerable<CryptoReference> userReferences = 
                userMessage.SecurityHeader.GetReferences();

            return userReferences.Any()
                   && userReferences.Select(IsNonRepudiationHashEqualToUserReferenceHash).All(r => r);
        }
    }
}