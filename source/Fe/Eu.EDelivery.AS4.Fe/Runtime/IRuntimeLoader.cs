using System.Collections.Generic;
using Eu.EDelivery.AS4.Fe.Modules;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    /// <summary>
    /// Interface to implement a runtime loader
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IModular" />
    public interface IRuntimeLoader : IModular
    {
        /// <summary>
        /// Gets the receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        IEnumerable<ItemType> Receivers { get; }
        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        IEnumerable<ItemType> Steps { get; }
        /// <summary>
        /// Gets the transformers.
        /// </summary>
        /// <value>
        /// The transformers.
        /// </value>
        IEnumerable<ItemType> Transformers { get; }
        /// <summary>
        /// Gets the certificate repositories.
        /// </summary>
        /// <value>
        /// The certificate repositories.
        /// </value>
        IEnumerable<ItemType> CertificateRepositories { get; }
        /// <summary>
        /// Gets the deliver senders.
        /// </summary>
        /// <value>
        /// The deliver senders.
        /// </value>
        IEnumerable<ItemType> DeliverSenders { get; }
        /// <summary>
        /// Gets the notify senders.
        /// </summary>
        /// <value>
        /// The notify senders.
        /// </value>
        IEnumerable<ItemType> NotifySenders { get; }
        /// <summary>
        /// Gets the attachment uploaders.
        /// </summary>
        /// <value>
        /// The attachment uploaders.
        /// </value>
        IEnumerable<ItemType> AttachmentUploaders { get; }
        /// <summary>
        /// Gets the dynamic discovery profiles.
        /// </summary>
        /// <value>
        /// The dynamic discovery profiles.
        /// </value>
        IEnumerable<ItemType> DynamicDiscoveryProfiles { get; }
        /// <summary>
        /// Gets the receiving pmode.
        /// </summary>
        /// <value>
        /// The receiving pmode.
        /// </value>
        IEnumerable<ItemType> MetaData { get; }        
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        IRuntimeLoader Initialize();
    }
}