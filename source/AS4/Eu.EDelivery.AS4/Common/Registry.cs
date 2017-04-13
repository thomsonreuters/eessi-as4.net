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
        }

        public Func<DatastoreContext> CreateDatastoreContext { get; set; }

        public IPayloadRetrieverProvider PayloadRetrieverProvider { get; private set; }

        public IDeliverSenderProvider DeliverSenderProvider { get; private set; }

        public INotifySenderProvider NotifySenderProvider { get; private set; }

        public ICertificateRepository CertificateRepository { get; set; }

        public ISerializerProvider SerializerProvider { get; }

        public IAttachmentUploaderProvider AttachmentUploader { get; private set; }

        private void RegisterPayloadStrategyProvider()
        {
            PayloadRetrieverProvider = new PayloadRetrieverProvider();
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith("file://"), new FilePayloadRetriever());
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith("ftp://"), new FtpPayloadRetriever());
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith("http"), new HttpPayloadRetriever());
        }

        private void RegisterDeliverSenderProvider()
        {
            DeliverSenderProvider = new DeliverSenderProvider();
            DeliverSenderProvider.Accept(s => s.Equals("FILE", StringComparison.OrdinalIgnoreCase), new ReliableSender(new FileDeliverySender()));
            DeliverSenderProvider.Accept(s => s.Equals("HTTP", StringComparison.OrdinalIgnoreCase), new ReliableSender(new HttpDeliverySender()));
        }

        private void RegisterNotifySenderProvider()
        {
            NotifySenderProvider = new NotifySenderProvider();

            NotifySenderProvider.Accept(s => s.Equals("FILE", StringComparison.OrdinalIgnoreCase), () => new ReliableSender(new FileNotifySender()));
            NotifySenderProvider.Accept(s => s.Equals("HTTP", StringComparison.OrdinalIgnoreCase), () => new ReliableSender(new HttpNotifySender()));
        }

        private void RegisterAttachmentUploaderProvider()
        {
            AttachmentUploader = new AttachmentUploaderProvider();

            var mimeTypeRepository = new MimeTypeRepository();

            AttachmentUploader.Accept(s => s.Equals("FILE", StringComparison.OrdinalIgnoreCase), new FileAttachmentUploader(mimeTypeRepository));
            AttachmentUploader.Accept(s => s.Equals("EMAIL", StringComparison.OrdinalIgnoreCase), new EmailAttachmentUploader(mimeTypeRepository));
            AttachmentUploader.Accept(s => s.Equals("PAYLOAD-SERVICE", StringComparison.OrdinalIgnoreCase), new PayloadServiceAttachmentUploader());
        }
    }
}