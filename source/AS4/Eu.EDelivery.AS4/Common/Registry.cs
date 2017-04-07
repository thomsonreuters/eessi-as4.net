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

        public IPayloadRetrieverProvider PayloadRetrieverProvider { get; private set; }
        public IDeliverSenderProvider DeliverSenderProvider { get; private set; }
        public INotifySenderProvider NotifySenderProvider { get; private set; }
        public ICertificateRepository CertificateRepository { get; set; }
        public ISerializerProvider SerializerProvider { get; }
        public IAttachmentUploaderProvider AttachmentUploader { get; private set; }

        public Func<DatastoreContext> CreateDatastoreContext { get; set; }

        public Registry()
        {
            SerializerProvider = new SerializerProvider();

            RegisterPayloadStrategyProvider();
            RegisterDeliverSenderProvider();
            RegisterNotifySenderProvider();
            RegisterAttachmentUploaderProvider();
        }

        private void RegisterPayloadStrategyProvider()
        {
            this.PayloadRetrieverProvider = new PayloadRetrieverProvider();
            this.PayloadRetrieverProvider.Accept(p => p.Location.StartsWith("file://"), new FilePayloadRetriever());
            this.PayloadRetrieverProvider.Accept(p => p.Location.StartsWith("ftp://"), new FtpPayloadRetriever());
            this.PayloadRetrieverProvider.Accept(p => p.Location.StartsWith("http"), new HttpPayloadRetriever());
        }

        private void RegisterDeliverSenderProvider()
        {
            this.DeliverSenderProvider = new DeliverSenderProvider();
            this.DeliverSenderProvider.Accept(s => s.Equals("FILE", StringComparison.OrdinalIgnoreCase), new FileDeliverySender());
            this.DeliverSenderProvider.Accept(s => s.Equals("HTTP", StringComparison.OrdinalIgnoreCase), new HttpDeliverySender());
        }

        private void RegisterNotifySenderProvider()
        {
            this.NotifySenderProvider = new NotifySenderProvider();

            this.NotifySenderProvider.Accept(s => s.Equals("FILE", StringComparison.OrdinalIgnoreCase), (() => new FileNotifySender()));
            this.NotifySenderProvider.Accept(s => s.Equals("HTTP", StringComparison.OrdinalIgnoreCase), () => new HttpNotifySender());
        }

        private void RegisterAttachmentUploaderProvider()
        {
            this.AttachmentUploader = new AttachmentUploaderProvider();

            var mimeTypeRepository = new MimeTypeRepository();

            this.AttachmentUploader.Accept(s => s.Equals("FILE", StringComparison.OrdinalIgnoreCase), new FileAttachmentUploader(mimeTypeRepository));
            this.AttachmentUploader.Accept(s => s.Equals("EMAIL", StringComparison.OrdinalIgnoreCase), new EmailAttachmentUploader(mimeTypeRepository));
        }
    }
}