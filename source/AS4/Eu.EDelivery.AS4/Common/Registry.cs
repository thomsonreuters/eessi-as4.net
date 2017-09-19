using System;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;

namespace Eu.EDelivery.AS4.Common
{
    /// <summary>
    /// Global Registry to provide Strategies
    /// </summary>
    public sealed class Registry : IRegistry
    {
        public static readonly Registry Instance = new Registry();

        public Registry()
        {
            SerializerProvider = new SerializerProvider();

            RegisterPayloadStrategyProvider();
            RegisterDeliverSenderProvider();
            RegisterNotifySenderProvider();
            RegisterAttachmentUploaderProvider();
            RegisterAS4MessageBodyRetrieverProvider();
        }

        public Func<DatastoreContext> CreateDatastoreContext { get; set; }

        public IPayloadRetrieverProvider PayloadRetrieverProvider { get; private set; }

        public IDeliverSenderProvider DeliverSenderProvider { get; private set; }

        public INotifySenderProvider NotifySenderProvider { get; private set; }

        public ICertificateRepository CertificateRepository { get; set; }

        public ISerializerProvider SerializerProvider { get; }

        public IAttachmentUploaderProvider AttachmentUploader { get; private set; }

        public MessageBodyStore MessageBodyStore { get; private set; }

        private void RegisterPayloadStrategyProvider()
        {
            PayloadRetrieverProvider = new PayloadRetrieverProvider();
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith(FilePayloadRetriever.Key, StringComparison.OrdinalIgnoreCase), new FilePayloadRetriever());
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith(FtpPayloadRetriever.Key, StringComparison.OrdinalIgnoreCase), new FtpPayloadRetriever());
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith(HttpPayloadRetriever.Key, StringComparison.OrdinalIgnoreCase), new HttpPayloadRetriever());
        }

        private void RegisterDeliverSenderProvider()
        {
            DeliverSenderProvider = new DeliverSenderProvider();
            DeliverSenderProvider.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, FileSender.Key), () => new ReliableSender(deliverSender: new FileSender()));
            DeliverSenderProvider.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, HttpSender.Key), () => new ReliableSender(deliverSender: new HttpSender()));
        }

        private void RegisterNotifySenderProvider()
        {
            NotifySenderProvider = new NotifySenderProvider();

            NotifySenderProvider.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, FileSender.Key), () => new ReliableSender(notifySender: new FileSender()));
            NotifySenderProvider.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, HttpSender.Key), () => new ReliableSender(notifySender: new HttpSender()));
        }

        private void RegisterAttachmentUploaderProvider()
        {
            AttachmentUploader = new AttachmentUploaderProvider();

            var mimeTypeRepository = new MimeTypeRepository();

            AttachmentUploader.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, FileAttachmentUploader.Key), new FileAttachmentUploader(mimeTypeRepository));
            AttachmentUploader.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, EmailAttachmentUploader.Key), new EmailAttachmentUploader(mimeTypeRepository));
            AttachmentUploader.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, PayloadServiceAttachmentUploader.Key), new PayloadServiceAttachmentUploader());
        }

        private void RegisterAS4MessageBodyRetrieverProvider()
        {
            MessageBodyStore = new MessageBodyStore();
            MessageBodyStore.Accept(
                condition: l => l.StartsWith("file:///", StringComparison.OrdinalIgnoreCase),
                persister: new AS4MessageBodyFileStore(Serialization.SerializerProvider.Default));
        }
    }
}