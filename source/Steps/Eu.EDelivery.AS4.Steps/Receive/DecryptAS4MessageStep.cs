using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;
using Org.BouncyCastle.Crypto;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// The use case describes how a message gets decrypted.
    /// </summary>
    [Info("Decrypt received message")]
    [Description("Decrypts the received AS4 Message if necessary by using the specified Receiving PMode")]
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
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext context)
        {
            if (context.AS4Message.IsSignalMessage)
            {
                return StepResult.Success(context);
            }

            AS4Message as4Message = context.AS4Message;
            ReceivingProcessingMode pmode = context.ReceivingPMode;
            Decryption decryption = pmode.Security.Decryption;

            if (decryption.Encryption == Limit.Required && !as4Message.IsEncrypted)
            {
                return FailedDecryptResult(
                    $"AS4 Message is not encrypted but Receiving PMode {pmode.Id} requires it. " + 
                    "Please alter the PMode Decryption.Encryption element to Allowed or Ignored",
                    ErrorAlias.PolicyNonCompliance,
                    context);
            }

            if (decryption.Encryption == Limit.NotAllowed && as4Message.IsEncrypted)
            {
                return FailedDecryptResult(
                    $"AS4 Message is encrypted but Receiving PMode {pmode.Id} doesn't allow it. " + 
                    "Please alter the PMode Decryption.Encryption element to Required, Allowed or Ignored",
                    ErrorAlias.PolicyNonCompliance,
                    context);
            }

            if (IsEncryptionIgnored(context) || !context.AS4Message.IsEncrypted)
            {
                return StepResult.Success(context);
            }

            return await TryDecryptAS4MessageAsync(context).ConfigureAwait(false);
        }

        private static StepResult FailedDecryptResult(string description, ErrorAlias errorAlias, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult("Cannot decrypt incoming message: " + description, errorAlias);
            return StepResult.Failed(context);
        }

        private static bool IsEncryptionIgnored(MessagingContext messagingContext)
        {
            ReceivingProcessingMode pmode = messagingContext.ReceivingPMode;
            bool isIgnored = pmode.Security.Decryption.Encryption == Limit.Ignored;

            if (isIgnored)
            {
                Logger.Info(
                    $"{messagingContext.LogTag} Decryption is ignored " + 
                    $"in Receiving PMode {pmode.Id}, so decryption will take place");
            }

            return isIgnored;
        }

        private async Task<StepResult> TryDecryptAS4MessageAsync(MessagingContext messagingContext)
        {
            try
            {
                Logger.Trace($"{messagingContext} Start decrypting AS4 Message ...");

                messagingContext.AS4Message.Decrypt(GetCertificate(messagingContext));

                Logger.Info($"{messagingContext} AS4 Message is decrypted correctly");

                return await StepResult.SuccessAsync(messagingContext);
            }
            catch (Exception ex) when (ex is CryptoException || ex is CryptographicException)
            {
                messagingContext.ErrorResult = new ErrorResult(
                    description: $"Decryption failed: {ex.Message}",
                    alias: ErrorAlias.FailedDecryption);

                Logger.Error(messagingContext.ErrorResult.Description);
                return StepResult.Failed(messagingContext);
            }
        }

        private X509Certificate2 GetCertificate(MessagingContext messagingContext)
        {
            Decryption decryption = messagingContext.ReceivingPMode.Security.Decryption;

            if (decryption.DecryptCertificateInformation == null)
            {
                throw new ConfigurationErrorsException(
                    "Cannot start decrypting: no certificate information found " + 
                    $"in Receiving PMode {messagingContext.ReceivingPMode.Id} to decrypt the message. " +
                    "Please use either a <CertificateFindCriteria/> or <PrivateKeyCertificate/> to specify the certificate information");
            }

            if (decryption.DecryptCertificateInformation is CertificateFindCriteria certFindCriteria)
            {
                return _certificateRepository.GetCertificate(
                    certFindCriteria.CertificateFindType,
                    certFindCriteria.CertificateFindValue);
            }

            if (decryption.DecryptCertificateInformation is PrivateKeyCertificate embeddedCertInfo)
            {
                return new X509Certificate2(
                    rawData: Convert.FromBase64String(embeddedCertInfo.Certificate),
                    password: embeddedCertInfo.Password,
                    keyStorageFlags: X509KeyStorageFlags.Exportable
                                     | X509KeyStorageFlags.MachineKeySet
                                     | X509KeyStorageFlags.PersistKeySet);
            }

            throw new NotSupportedException(
                "The signing certificate information specified in the Receiving PMode " + 
                $"{messagingContext.ReceivingPMode.Id} could not be used to retrieve the certificate. " + 
                "Please use either a <CertificateFindCriteria/> or <PrivateKeyCertificate/> to specify the certificate information");
        }
    }
}