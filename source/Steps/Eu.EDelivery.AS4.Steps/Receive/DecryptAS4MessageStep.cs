
using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Strategies;
using NLog;
using Org.BouncyCastle.Crypto;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// The use case describes how a message gets decrypted.
    /// </summary>
    [Description("Decrypts the received AS4 Message if necessary")]
    [Info("Decrypt received message")]
    public class DecryptAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICertificateRepository _certificateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptAS4MessageStep" /> class
        /// Create a <see cref="IStep" /> implementation
        /// to decrypt a <see cref="AS4Message" />
        /// </summary>
        public DecryptAS4MessageStep() : this(Registry.Instance.CertificateRepository) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptAS4MessageStep"/> class.
        /// </summary>
        /// <param name="certificateRepository">The certificate repository.</param>
        public DecryptAS4MessageStep(ICertificateRepository certificateRepository)
        {
            _certificateRepository = certificateRepository;
        }

        /// <summary>
        /// Start Decrypting <see cref="AS4Message"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext context, CancellationToken cancellationToken)
        {
            if (context.AS4Message.IsSignalMessage)
            {
                return await StepResult.SuccessAsync(context);
            }

            AS4Message as4Message = context.AS4Message;
            ReceivingProcessingMode pmode = context.ReceivingPMode;
            Decryption decryption = pmode.Security.Decryption;

            if (decryption.Encryption == Limit.Required && !as4Message.IsEncrypted)
            {
                return FailedDecryptResult($"AS4 Message is not encrypted but Receiving PMode {pmode.Id} requires it", ErrorAlias.PolicyNonCompliance, context);
            }

            if (decryption.Encryption == Limit.NotAllowed && as4Message.IsEncrypted)
            {
                return FailedDecryptResult($"AS4 Message is encrypted but Receiving PMode {pmode.Id} doesn't allow it", ErrorAlias.PolicyNonCompliance, context);
            }

            if (IsEncryptedIgnored(context) || !context.AS4Message.IsEncrypted)
            {
                return await StepResult.SuccessAsync(context);
            }

            return await TryDecryptAS4Message(context);
        }

        private static StepResult FailedDecryptResult(string description, ErrorAlias errorAlias, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, errorAlias);
            return StepResult.Failed(context);
        }

        private static bool IsEncryptedIgnored(MessagingContext messagingContext)
        {
            ReceivingProcessingMode pmode = messagingContext.ReceivingPMode;
            bool isIgnored = pmode.Security.Decryption.Encryption == Limit.Ignored;

            if (isIgnored)
            {
                Logger.Info($"{messagingContext.EbmsMessageId} Decryption Receiving PMode {pmode.Id} is ignored");
            }

            return isIgnored;
        }

        private async Task<StepResult> TryDecryptAS4Message(MessagingContext messagingContext)
        {
            try
            {
                Logger.Info($"{messagingContext.EbmsMessageId} Start decrypting AS4 Message ...");

                IEncryptionStrategy strategy = CreateDecryptStrategy(messagingContext);
                messagingContext.AS4Message.SecurityHeader.Decrypt(strategy);

                Logger.Info($"{messagingContext.EbmsMessageId} AS4 Message is decrypted correctly");

                return await StepResult.SuccessAsync(messagingContext);
            }
            catch (Exception exception) when (exception is CryptoException || exception is CryptographicException)
            {
                messagingContext.ErrorResult = new ErrorResult(
                    description: $"Decryption failed: {exception.Message}",
                    alias: ErrorAlias.FailedDecryption);

                Logger.Error(messagingContext.ErrorResult.Description);

                return StepResult.Failed(messagingContext);
            }
        }

        private IEncryptionStrategy CreateDecryptStrategy(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;
            X509Certificate2 certificate = GetCertificate(messagingContext);

            if (!certificate.HasPrivateKey)
            {
                throw new CryptoException("Decryption failed - No private key found.");
            }

            return EncryptionStrategyBuilder
                .Create(as4Message.EnvelopeDocument)
                .WithCertificate(certificate)
                .WithAttachments(as4Message.Attachments)
                .Build();
        }

        private X509Certificate2 GetCertificate(MessagingContext messagingContext)
        {
            Decryption decryption = messagingContext.ReceivingPMode.Security.Decryption;

            if (decryption.DecryptCertificateInformation == null)
            {
                throw new ConfigurationErrorsException("No certificate information found in PMode to decrypt the message.");
            }

            var certFindCriteria = decryption.DecryptCertificateInformation as CertificateFindCriteria;

            if (certFindCriteria != null)
            {
                return _certificateRepository.GetCertificate(certFindCriteria.CertificateFindType, certFindCriteria.CertificateFindValue);
            }

            var embeddedCertInfo = decryption.DecryptCertificateInformation as PrivateKeyCertificate;

            if (embeddedCertInfo != null)
            {
                return new X509Certificate2(Convert.FromBase64String(embeddedCertInfo.Certificate), embeddedCertInfo.Password, X509KeyStorageFlags.Exportable);
            }

            throw new NotSupportedException("The signing certificate information specified in the PMode could not be used to retrieve the certificate");
        }
    }
}