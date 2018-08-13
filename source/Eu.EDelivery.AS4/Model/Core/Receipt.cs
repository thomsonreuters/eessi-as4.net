using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Xml;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Receipt : SignalMessage
    {
        /// <summary>
        /// The UserMessage for which this is a receipt.
        /// </summary>
        /// <value>The user message.</value>
        /// <remarks>This property should only be populated when the NonRepudiationInformation is not filled out.</remarks>
        public UserMessage UserMessage { get; }

        /// <summary>
        /// NonRepudiation information of the UserMessage for which this is a receipt.
        /// </summary>
        /// <value>The non repudiation information.</value>
        /// <remarks>This property is only populated when the UserMessage property is not filled out.</remarks>
        public NonRepudiationInformation NonRepudiationInformation { get; }

        /// <summary>
        /// Gets the multihop action value.
        /// </summary>
        public override string MultihopAction { get; } = Constants.Namespaces.EbmsOneWayReceipt;

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="refToMessageId">The reference to a <see cref="Core.UserMessage"/></param>
        public Receipt(string refToMessageId) 
            : base(IdentifierFactory.Instance.Create(), refToMessageId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        public Receipt(
            string messageId, 
            string refToMessageId, 
            DateTimeOffset timestamp) 
            : base(messageId, refToMessageId, timestamp) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="includedUserMessage"></param>
        public Receipt(
            string refToMessageId,
            UserMessage includedUserMessage)
            : base(IdentifierFactory.Instance.Create(), refToMessageId)
        {
            if (includedUserMessage == null)
            {
                throw new ArgumentNullException(nameof(includedUserMessage));
            }

            UserMessage = includedUserMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="includedUserMessage"></param>
        /// <param name="routedUserMessage"></param>
        public Receipt(
            string refToMessageId,
            UserMessage includedUserMessage,
            RoutingInputUserMessage routedUserMessage)
            : base(IdentifierFactory.Instance.Create(), refToMessageId, routedUserMessage)
        {
            if (includedUserMessage == null)
            {
                throw new ArgumentNullException(nameof(includedUserMessage));
            }

            UserMessage = includedUserMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="includedUserMessage"></param>
        public Receipt(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            UserMessage includedUserMessage)
            : base(messageId, refToMessageId, timestamp)
        {
            if (includedUserMessage == null)
            {
                throw new ArgumentNullException(nameof(includedUserMessage));
            }

            UserMessage = includedUserMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="routing"></param>
        public Receipt(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            RoutingInputUserMessage routing)
            : base(messageId, refToMessageId, timestamp, routing) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="includedUserMessage"></param>
        /// <param name="routing"></param>
        public Receipt(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            UserMessage includedUserMessage,
            RoutingInputUserMessage routing)
            : base(messageId, refToMessageId, timestamp, routing)
        {
            if (includedUserMessage == null)
            {
                throw new ArgumentNullException(nameof(includedUserMessage));
            }

            UserMessage = includedUserMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="nonRepudiation"></param>
        public Receipt(
            string refToMessageId,
            NonRepudiationInformation nonRepudiation)
            : base(IdentifierFactory.Instance.Create(), refToMessageId)
        {
            if (nonRepudiation == null)
            {
                throw new ArgumentNullException(nameof(nonRepudiation));
            }

            NonRepudiationInformation = nonRepudiation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonRepudiation"></param>
        public Receipt(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            NonRepudiationInformation nonRepudiation)
            : base(messageId, refToMessageId, timestamp)
        {
            if (nonRepudiation == null)
            {
                throw new ArgumentNullException(nameof(nonRepudiation));
            }

            NonRepudiationInformation = nonRepudiation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonRepudiation"></param>
        /// <param name="routing"></param>
        public Receipt(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            NonRepudiationInformation nonRepudiation,
            RoutingInputUserMessage routing)
            : base(messageId, refToMessageId, timestamp, routing)
        {
            if (nonRepudiation == null)
            {
                throw new ArgumentNullException(nameof(nonRepudiation));
            }

            NonRepudiationInformation = nonRepudiation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="nonRepudiation"></param>
        /// <param name="routedUserMessage"></param>
        public Receipt(
            string refToMessageId,
            NonRepudiationInformation nonRepudiation,
            RoutingInputUserMessage routedUserMessage)
            : base(IdentifierFactory.Instance.Create(), refToMessageId, routedUserMessage)
        {
            if (nonRepudiation == null)
            {
                throw new ArgumentNullException(nameof(nonRepudiation));
            }

            NonRepudiationInformation = nonRepudiation;
        }

        /// <summary>
        /// Verifies the Non-Repudiation Information of the <see cref="Receipt"/> against the NRI of the related <see cref="Core.UserMessage"/>.
        /// </summary>
        /// <param name="userMessage">The related <see cref="Core.UserMessage"/>.</param>
        /// <returns></returns>
        public bool VerifyNonRepudiationInfo(AS4Message userMessage)
        {
            IEnumerable<CryptoReference> userReferences = 
                userMessage.SecurityHeader.GetReferences();

            return userReferences.Any()
                   && userReferences.Select(IsNonRepudiationHashEqualToUserReferenceHash).All(r => r);
        }

        private bool IsNonRepudiationHashEqualToUserReferenceHash(CryptoReference r)
        {
            byte[] repudiationHash = GetNonRepudiationHashForUri(r.Uri);
            return repudiationHash != null && r.DigestValue?.SequenceEqual(repudiationHash) == true;
        }

        private byte[] GetNonRepudiationHashForUri(string userMessageReferenceUri)
        {
            return NonRepudiationInformation.MessagePartNRIReferences
                .Where(r => r.URI.Equals(userMessageReferenceUri))
                .Select(r => r.DigestValue)
                .FirstOrDefault();
        }
    }
}