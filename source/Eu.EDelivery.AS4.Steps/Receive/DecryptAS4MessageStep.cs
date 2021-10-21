using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services.Journal;
using log4net;
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
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

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
            if (certificateRepository == null)
            {
                throw new ArgumentNullException(nameof(certificateRepository));
            }

            _certificateRepository = certificateRepository;
        }

        /// <summary>
        /// Start Decrypting <see cref="AS4Message"/>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DecryptAS4MessageStep)} requires a AS4Message to decrypt but no AS4Message is present in the MessagingContext");
            }

            if (context.AS4Message.IsSignalMessage)
            {
                Logger.Debug("AS4Message is SignalMessage so will skip decryption since AS4.NET Component only supports encryption of payloads");
                return StepResult.Success(context);
            }

            if (context.ReceivingPMode?.Security?.Decryption == null)
            {
                Logger.Debug("AS4Message will not be decrypted sicne ReceivingPMode hasn't got a Security.Decryption element");
                return StepResult.Success(context);
            }

            ReceivingProcessingMode receivePMode = context.ReceivingPMode;
            if (receivePMode.Security.Decryption.Encryption == Limit.Required && !context.AS4Message.IsEncrypted)
            {
                Logger.Error(
                    $"AS4Message is not encrypted but ReceivingPMode {Config.Encode(receivePMode.Id)} requires it. "
                    + $"{Environment.NewLine} Please alter the PMode Decryption.Encryption element to Allowed or Ignored");

                context.ErrorResult = new ErrorResult("AS4Message is not encrypted but ReceivingPMode requires it", ErrorAlias.PolicyNonCompliance);
                return StepResult.Failed(context);
            }

            if (receivePMode.Security.Decryption.Encryption == Limit.NotAllowed && context.AS4Message.IsEncrypted)
            {
                Logger.Error(
                    $"AS4Message is encrypted but ReceivingPMode {Config.Encode(receivePMode.Id)} doesn't allow it. " +
                    $"{Environment.NewLine} Please alter the PMode Decryption.Encryption element to Required, Allowed or Ignored");

                context.ErrorResult = new ErrorResult("AS4Message is encrypted but ReceivingPMode doesn't allow it", ErrorAlias.PolicyNonCompliance);
                return StepResult.Failed(context);
            }

            if (!context.AS4Message.IsEncrypted)
            {
                Logger.Debug("AS4Message is not encrypted so will skip decryption");
                return StepResult.Success(context);
            }

            if (context.ReceivingPMode?.Security?.Decryption?.Encryption == Limit.Ignored)
            {
                Logger.Debug($"Decryption is ignored in ReceivingPMode {Config.Encode(receivePMode.Id)}, so no decryption will take place");
                return StepResult.Success(context);
            }

            return await DecryptAS4MessageAsync(context).ConfigureAwait(false);
        }

        private async Task<StepResult> DecryptAS4MessageAsync(MessagingContext messagingContext)
        {
            try
            {
                Logger.Trace("Start decrypting AS4Message ...");
                X509Certificate2 decryptionCertificate = GetCertificate(messagingContext);
                messagingContext.AS4Message.Decrypt(decryptionCertificate);
                Logger.Info($"{Config.Encode(messagingContext.LogTag)} AS4Message is decrypted correctly");

                JournalLogEntry entry = 
                    JournalLogEntry.CreateFrom(
                        messagingContext.AS4Message, 
                        $"Decrypted using certificate {decryptionCertificate.FriendlyName}");

                return await StepResult
                    .Success(messagingContext)
                    .WithJournalAsync(entry);
            }
            catch (Exception ex) when (ex is CryptoException || ex is CryptographicException)
            {
                Logger.Error(Config.Encode(ex));

                messagingContext.ErrorResult = new ErrorResult(
                    description: "Decryption of message failed",
                    alias: ErrorAlias.FailedDecryption);

                Logger.Error(Config.Encode(messagingContext.ErrorResult.Description));
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
                    $"in ReceivingPMode {messagingContext.ReceivingPMode.Id} to decrypt the message. " +
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
                "The decrypt-certificate information specified in the ReceivingPMode " + 
                $"{messagingContext.ReceivingPMode.Id} could not be used to retrieve the certificate used for decryption. " + 
                "Please use either a <CertificateFindCriteria/> or <PrivateKeyCertificate/> to specify the certificate information");
        }
    }
}