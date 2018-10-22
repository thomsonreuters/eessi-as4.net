using System;
using Eu.EDelivery.AS4.Repositories;
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
        INotifySenderProvider NotifySenderProvider { get; }
        IPayloadRetrieverProvider PayloadRetrieverProvider { get; }
        MessageBodyStore MessageBodyStore { get; }
    }
}