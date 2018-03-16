using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Encryption;
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
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (!messagingContext.SendingPMode.Security.Encryption.IsEnabled)
            {
                return await ReturnSameMessagingContext(messagingContext);
            }

            TryEncryptAS4Message(messagingContext);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private void TryEncryptAS4Message(MessagingContext messagingContext)
        {
            Logger.Info($"{messagingContext.EbmsMessageId} Encrypt AS4 Message with given Encryption Information");
            try
            {
                var encryptionSettings = messagingContext.SendingPMode.Security.Encryption;

                var keyEncryptionConfig = RetrieveKeyEncryptionConfig(encryptionSettings);
                var dataEncryptionConfig = new DataEncryptionConfiguration(encryptionSettings.Algorithm, 
                                                                           algorithmKeySize: encryptionSettings.AlgorithmKeySize);

                messagingContext.AS4Message.Encrypt(keyEncryptionConfig, dataEncryptionConfig);
            }
            catch (Exception exception)
            {
                string description = $"{messagingContext.EbmsMessageId} Problems with Encrypting AS4 Message: {exception.Message}";
                Logger.Error(description);

                throw new CryptographicException(description, exception);
            }
        }

        private KeyEncryptionConfiguration RetrieveKeyEncryptionConfig(Encryption encryptionSettings)
        {
            X509Certificate2 certificate = RetrieveCertificate(encryptionSettings);

            return new KeyEncryptionConfiguration(certificate, keyEncryption: encryptionSettings.KeyTransport);
        }

        private X509Certificate2 RetrieveCertificate(Encryption encryptionSettings)
        {
            if (encryptionSettings.EncryptionCertificateInformation == null)
            {
                throw new ConfigurationErrorsException("No encryption certificate information found in PMode to perform encryption");
            }

            var publicKeyFindCriteria = encryptionSettings.EncryptionCertificateInformation as CertificateFindCriteria;

            if (publicKeyFindCriteria != null)
            {
                return _certificateRepository.GetCertificate(publicKeyFindCriteria.CertificateFindType, publicKeyFindCriteria.CertificateFindValue);
            }

            var publicKeyCertificate = encryptionSettings.EncryptionCertificateInformation as PublicKeyCertificate;

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