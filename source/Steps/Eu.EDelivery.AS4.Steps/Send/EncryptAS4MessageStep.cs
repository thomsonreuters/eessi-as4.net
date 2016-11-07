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
using Eu.EDelivery.AS4.Security.Strategies;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the MSH encrypts the ebMS UserMessage
    /// </summary>
    public class EncryptAS4MessageStep : IStep
    {
        private readonly ILogger _logger;
        private readonly ICertificateRepository _certificateRepository;

        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TryEncryptAS4Message"/> class
        /// </summary>
        public EncryptAS4MessageStep() : this(Registry.Instance.CertificateRepository) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="TryEncryptAS4Message"/> class
        /// </summary>
        /// <param name="certificateRepository"></param>
        public EncryptAS4MessageStep(ICertificateRepository certificateRepository)
        {
            this._certificateRepository = certificateRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start Encrypting AS4 Message
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            _internalMessage = internalMessage;

            if (!internalMessage.AS4Message.SendingPMode.Security.Encryption.IsEnabled)
                return ReturnSameInternalMessage(internalMessage);

            TryEncryptAS4Message();

            return StepResult.SuccessAsync(internalMessage);
        }

        private void TryEncryptAS4Message()
        {
            this._logger.Info($"{this._internalMessage.Prefix} Encrypt AS4 Message with given Encryption Information");
            try
            {
                IEncryptionStrategy strategy = CreateEncryptStrategy();
                this._internalMessage.AS4Message.SecurityHeader.Encrypt(strategy);
            }
            catch (Exception exception)
            {
                string description = $"{this._internalMessage.Prefix} Problems with Encrypting AS4 Message";
                throw ThrowCommonEncryptionException(description, exception);
            }
        }

        private IEncryptionStrategy CreateEncryptStrategy()
        {
            AS4Message as4Message = this._internalMessage.AS4Message;
            Encryption encryption = as4Message.SendingPMode.Security.Encryption;
            X509Certificate2 certificate = RetrieveCertificate(encryption);

            var builder = new EncryptionStrategyBuilder(as4Message);

            builder.WithEncryptionAlgorithm(encryption.Algorithm);
            builder.WithCertificate(certificate);
            builder.WithAttachments(as4Message.Attachments);

            return builder.Build();
        }

        private X509Certificate2 RetrieveCertificate(Encryption encryption)
        {
            X509Certificate2 certificate = this._certificateRepository
                .GetCertificate(encryption.PublicKeyFindType, encryption.PublicKeyFindValue);

            if (!certificate.HasPrivateKey)
                throw ThrowCommonEncryptionException($"{this._internalMessage.Prefix} Failed Authentication");

            return certificate;
        }

        private Task<StepResult> ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            this._logger.Debug($"Sending PMode {internalMessage.AS4Message.SendingPMode.Id} Encryption is disabled");
            return StepResult.SuccessAsync(internalMessage);
        }

        private AS4Exception ThrowCommonEncryptionException(string description, Exception innerException = null)
        {
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(this._internalMessage.AS4Message.MessageIds)
                .WithSendingPMode(this._internalMessage.AS4Message.SendingPMode)
                .Build();
        }
    }
}