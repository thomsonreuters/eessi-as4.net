using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Common;
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
    [Description("This step encrypts the AS4 Message and its attachments if encryption is enabled in the sending PMode")]
    [Info("Encrypt AS4 Message")]
    public class EncryptAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICertificateRepository _certificateRepository;

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
                return await ReturnSameMessagingContext(messagingContext);
            }

            var sw = new Stopwatch();
            sw.Start();

            TryEncryptAS4Message(messagingContext);

            sw.Stop();
            Logger.Trace($"Encrypt took {sw.ElapsedMilliseconds} milliseconds");

            return await StepResult.SuccessAsync(messagingContext);
        }

        private void TryEncryptAS4Message(MessagingContext messagingContext)
        {
            Logger.Info($"{messagingContext.EbmsMessageId} Encrypt AS4 Message with given Encryption Information");
            try
            {
                IEncryptionStrategy strategy = CreateEncryptStrategy(messagingContext);
                messagingContext.AS4Message.SecurityHeader.Encrypt(strategy);
            }
            catch (Exception exception)
            {
                string description = $"{messagingContext.EbmsMessageId} Problems with Encrypting AS4 Message: {exception.Message}";
                Logger.Error(description);

                throw new CryptographicException(description, exception);
            }
        }

        private IEncryptionStrategy CreateEncryptStrategy(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;
            Encryption encryption = messagingContext.SendingPMode.Security.Encryption;

            X509Certificate2 certificate = RetrieveCertificate(messagingContext);

            EncryptionStrategyBuilder builder = EncryptionStrategyBuilder.Create(messagingContext.AS4Message);

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

            if (encryption.EncryptionCertificateInformation == null)
            {
                throw new ConfigurationErrorsException("No encryption certificate information found in PMode to perform encryption");
            }

            var publicKeyFindCriteria = encryption.EncryptionCertificateInformation as CertificateFindCriteria;

            if (publicKeyFindCriteria != null)
            {
                return _certificateRepository.GetCertificate(publicKeyFindCriteria.CertificateFindType, publicKeyFindCriteria.CertificateFindValue);
            }

            var publicKeyCertificate = encryption.EncryptionCertificateInformation as PublicKeyCertificate;

            if (publicKeyCertificate != null)
            {
                return new X509Certificate2(Convert.FromBase64String(publicKeyCertificate.Certificate), string.Empty);
            }

            throw new NotSupportedException("The encryption certificate information specified in the PMode could not be used to retrieve the certificate");            
        }

        private static Task<StepResult> ReturnSameMessagingContext(MessagingContext messagingContext)
        {
            Logger.Debug($"Sending PMode {messagingContext.SendingPMode.Id} Encryption is disabled");
            return StepResult.SuccessAsync(messagingContext);
        }
    }
}