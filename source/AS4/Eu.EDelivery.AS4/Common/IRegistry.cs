using System;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;

namespace Eu.EDelivery.AS4.Common
{
    public interface IRegistry
    {
        IAttachmentUploaderProvider AttachmentUploader { get; }
        ICertificateRepository CertificateRepository { get; }
        Func<DatastoreContext> CreateDatastoreContext { get; }
        IDeliverSenderProvider DeliverSenderProvider { get; }
        bool IsInitialized { get; }
        MessageBodyStore MessageBodyStore { get; }
        INotifySenderProvider NotifySenderProvider { get; }
        IPayloadRetrieverProvider PayloadRetrieverProvider { get; }
        ISerializerProvider SerializerProvider { get; }
    }
}