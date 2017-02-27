using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;

namespace Eu.EDelivery.AS4.Common
{
    public interface IRegistry
    {
        ISerializerProvider SerializerProvider { get; }
        ICertificateRepository CertificateRepository { get; set; }
        INotifySenderProvider NotifySenderProvider { get;  }
        IDeliverSenderProvider DeliverSenderProvider { get; }
        IPayloadRetrieverProvider PayloadRetrieverProvider { get;  }        
        IAttachmentUploaderProvider AttachmentUploader { get;  }
    }
}