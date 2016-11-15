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
        private static readonly Registry Signalton = new Registry();

        public static Registry Instance => Signalton;

        public IPayloadRetrieverProvider PayloadRetrieverProvider { get; set; }
        public IDeliverSenderProvider DeliverSenderProvider { get; private set; }
        public INotifySenderProvider NotifySenderProvider { get; set; }
        public ICertificateRepository CertificateRepository { get; set; }
        public ISerializerProvider SerializerProvider => new SerializerProvider();
        public IAttachmentUploaderProvider AttachmentUploader { get; set; }
        public IDatastoreRepository DatastoreRepository { get; set; }

        public Registry()
        {
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
            this.PayloadRetrieverProvider.Accept(p => p.Location.StartsWith("http"), new WebPayloadRetriever());
        }

        private void RegisterDeliverSenderProvider()
        {
            this.DeliverSenderProvider = new DeliverSenderProvider();
            this.DeliverSenderProvider.Accept(s
                => s.Equals("FILE", StringComparison.CurrentCultureIgnoreCase), new FileDeliverXmlSender());
        }

        private void RegisterNotifySenderProvider()
        {
            this.NotifySenderProvider = new NotifySenderProvider();
            this.NotifySenderProvider.Accept(s
                => s.Equals("FILE", StringComparison.CurrentCultureIgnoreCase), new FileNotifyXmlSender());
        }

        private void RegisterAttachmentUploaderProvider()
        {
            this.AttachmentUploader = new AttachmentUploaderProvider();

            var ignoreCase = StringComparison.CurrentCultureIgnoreCase;
            var mimeTypeRepository = new MimeTypeRepository();
            this.AttachmentUploader.Accept(s => s.Equals("FILE", ignoreCase), new FileAttachmentUploader(mimeTypeRepository));
            this.AttachmentUploader.Accept(s => s.Equals("EMAIL", ignoreCase), new EmailAttachmentUploader(mimeTypeRepository));
        }
    }
}