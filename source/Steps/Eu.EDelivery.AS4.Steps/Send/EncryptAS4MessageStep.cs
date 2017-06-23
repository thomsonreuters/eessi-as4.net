using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Strategies;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the MSH encrypts the ebMS UserMessage
    /// </summary>
    public class EncryptAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICertificateRepository _certificateRepository;

        ////private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptAS4MessageStep"/> class
        /// </summary>
        public EncryptAS4MessageStep() : this(Registry.Instance.CertificateRepository) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptAS4MessageStep"/> class
        /// </summary>
        /// <param name="certificateRepository"></param>
        public EncryptAS4MessageStep(ICertificateRepository certificateRepository)
        {
            _certificateRepository = certificateRepository;
        }

        /// <summary>
        /// Start Encrypting AS4 Message
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (!messagingContext.SendingPMode.Security.Encryption.IsEnabled)
            {
                return await ReturnSameInternalMessage(messagingContext);
            }

            TryEncryptAS4Message(messagingContext);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private void TryEncryptAS4Message(MessagingContext messagingContext)
        {
            Logger.Info($"{messagingContext.Prefix} Encrypt AS4 Message with given Encryption Information");
            try
            {
                IEncryptionStrategy strategy = CreateEncryptStrategy(messagingContext);
                messagingContext.AS4Message.SecurityHeader.Encrypt(strategy);
            }
            catch (Exception exception)
            {
                string description = $"{messagingContext.Prefix} Problems with Encrypting AS4 Message: {exception.Message}";
                Logger.Error(description);

                throw new ApplicationException(description, exception);
            }
        }

        private IEncryptionStrategy CreateEncryptStrategy(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;
            Encryption encryption = messagingContext.SendingPMode.Security.Encryption;

            X509Certificate2 certificate = RetrieveCertificate(messagingContext);

            EncryptionStrategyBuilder builder = EncryptionStrategyBuilder.Create(messagingContext);

            builder.WithDataEncryptionConfiguration(
                new DataEncryptionConfiguration(encryption.Algorithm, algorithmKeySize: encryption.AlgorithmKeySize));

            builder.WithKeyEncryptionConfiguration(
                new KeyEncryptionConfiguration(tokenReference: null, keyEncryption: encryption.KeyTransport));

            builder.WithCertificate(certificate);
            builder.WithAttachments(as4Message.Attachments);

            return builder.Build();
        }

        private X509Certificate2 RetrieveCertificate(MessagingContext messagingContext)
        {
            Encryption encryption = messagingContext.SendingPMode.Security.Encryption;

            return _certificateRepository.GetCertificate(encryption.PublicKeyFindType, encryption.PublicKeyFindValue);
        }

        private static Task<StepResult> ReturnSameInternalMessage(MessagingContext messagingContext)
        {
            Logger.Debug($"Sending PMode {messagingContext.SendingPMode.Id} Encryption is disabled");
            return StepResult.SuccessAsync(messagingContext);
        }
    }
}