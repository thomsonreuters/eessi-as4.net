using System;
using Eu.EDelivery.AS4.Builders;
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
    public sealed class Registry
    {
        public static readonly Registry Instance = new Registry();

        private ICertificateRepository _certificateRepository;
        private Func<DatastoreContext> _createDatastore;

        /// <summary>
        /// Initializes a new instance of the <see cref="Registry"/> class.
        /// </summary>
        private Registry()
        {
            SerializerProvider = new SerializerProvider();

            PayloadRetrieverProvider = new PayloadRetrieverProvider();
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith(FilePayloadRetriever.Key, StringComparison.OrdinalIgnoreCase), new FilePayloadRetriever());
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith(TempFilePayloadRetriever.Key, StringComparison.OrdinalIgnoreCase), new TempFilePayloadRetriever());
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith(FtpPayloadRetriever.Key, StringComparison.OrdinalIgnoreCase), new FtpPayloadRetriever());
            PayloadRetrieverProvider.Accept(p => p.Location.StartsWith(HttpPayloadRetriever.Key, StringComparison.OrdinalIgnoreCase), new HttpPayloadRetriever());

            DeliverSenderProvider = new DeliverSenderProvider();
            DeliverSenderProvider.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, FileSender.Key), () => new ReliableSender(deliverSender: new FileSender()));
            DeliverSenderProvider.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, HttpSender.Key), () => new ReliableSender(deliverSender: new HttpSender()));

            NotifySenderProvider = new NotifySenderProvider();
            NotifySenderProvider.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, FileSender.Key), () => new ReliableSender(notifySender: new FileSender()));
            NotifySenderProvider.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, HttpSender.Key), () => new ReliableSender(notifySender: new HttpSender()));

            AttachmentUploader = new AttachmentUploaderProvider();
            var mimeTypeRepository = new MimeTypeRepository();
            AttachmentUploader.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, FileAttachmentUploader.Key), new FileAttachmentUploader(mimeTypeRepository));
            AttachmentUploader.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, EmailAttachmentUploader.Key), new EmailAttachmentUploader(mimeTypeRepository));
            AttachmentUploader.Accept(s => StringComparer.OrdinalIgnoreCase.Equals(s, PayloadServiceAttachmentUploader.Key), new PayloadServiceAttachmentUploader());

            MessageBodyStore = new MessageBodyStore();
            MessageBodyStore.Accept(
                condition: l => l.StartsWith("file:///", StringComparison.OrdinalIgnoreCase),
                persister: new AS4MessageBodyFileStore(Serialization.SerializerProvider.Default));
        }

        /// <summary>
        /// Initializes the <see cref="Registry"/>
        /// </summary>
        /// <param name="config"></param>
        public void Initialize(IConfig config)
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;

            string certificateTypeRepository = config.GetSetting("CertificateRepository");
            _certificateRepository = 
                String.IsNullOrWhiteSpace(certificateTypeRepository) 
                    ? new CertificateRepository() 
                    : GenericTypeBuilder.FromType(certificateTypeRepository).Build<ICertificateRepository>();

            _createDatastore = () => new DatastoreContext(config);
        }

        public bool IsInitialized { get; private set; }

        public Func<DatastoreContext> CreateDatastoreContext => OnlyAfterInitialized(() => _createDatastore);

        public IPayloadRetrieverProvider PayloadRetrieverProvider { get; }

        public IDeliverSenderProvider DeliverSenderProvider { get; }

        public INotifySenderProvider NotifySenderProvider { get; }

        public ICertificateRepository CertificateRepository => OnlyAfterInitialized(() => _certificateRepository);

        public ISerializerProvider SerializerProvider { get; }

        public IAttachmentUploaderProvider AttachmentUploader { get; }

        public MessageBodyStore MessageBodyStore { get; }

        private T OnlyAfterInitialized<T>(Func<T> f)
        {
            if (IsInitialized)
            {
                return f();
            }

            throw new InvalidOperationException(
                "Cannot use this member before the configuration is initialized. " +
                $"Call the {nameof(Initialize)} method to initialize the registration");
        }
    }
}