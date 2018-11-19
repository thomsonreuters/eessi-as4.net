using System;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;

namespace Eu.EDelivery.AS4.Common
{
    /// <summary>
    /// Global Registry to provide Strategies
    /// </summary>
    public sealed class Registry : IRegistry
    {
        public static readonly Registry Instance = new Registry();
        private Func<DatastoreContext> _createDatastore;

        /// <summary>
        /// Initializes a new instance of the <see cref="Registry"/> class.
        /// </summary>
        private Registry()
        {
            CertificateRepository = new CertificateRepository();

            MessageBodyStore = new MessageBodyStore();
            MessageBodyStore.Accept(
                condition: l => l.StartsWith("file:///", StringComparison.OrdinalIgnoreCase),
                persister: new AS4MessageBodyFileStore());
        }

        /// <summary>
        /// Initializes the <see cref="Registry"/>
        /// </summary>
        /// <param name="config"></param>
        public void Initialize(IConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (IsInitialized)
            {
                return;
            }

            _createDatastore = () => new DatastoreContext(config);

            IsInitialized = true;
            PullRequestAuthorizationMapProvider = new FilePullAuthorizationMapProvider(config.AuthorizationMapPath);
            CertificateRepository = ResolveCertificateRepository(config.CertificateRepositoryType);
        }

        private static ICertificateRepository ResolveCertificateRepository(string typeString)
        {
            if (!GenericTypeBuilder.CanResolveTypeThatImplements<ICertificateRepository>(typeString))
            {
                throw new InvalidOperationException(
                    $"Cannot resolve a valid {nameof(ICertificateRepository)} implementation for the {typeString} fully-qualified assembly name");
            }

            return GenericTypeBuilder
                .FromType(typeString)
                .Build<ICertificateRepository>();
        }

        public bool IsInitialized { get; private set; }

        public Func<DatastoreContext> CreateDatastoreContext => OnlyAfterInitialized(() => _createDatastore);

        public ICertificateRepository CertificateRepository { get; private set; }

        public IPullAuthorizationMapProvider PullRequestAuthorizationMapProvider { get; private set; }

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