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
        bool IsInitialized { get; }
        Func<DatastoreContext> CreateDatastoreContext { get; }
        IAttachmentUploaderProvider AttachmentUploader { get; }
        ICertificateRepository CertificateRepository { get; }
        IDeliverSenderProvider DeliverSenderProvider { get; }
        INotifySenderProvider NotifySenderProvider { get; }
        IPayloadRetrieverProvider PayloadRetrieverProvider { get; }
        MessageBodyStore MessageBodyStore { get; }
        SerializerProvider SerializerProvider { get; }
    }
}