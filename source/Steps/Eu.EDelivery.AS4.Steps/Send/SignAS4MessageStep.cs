using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;
using System.Security.Cryptography;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Signing;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the MSH signs the AS4 UserMessage
    /// </summary>
    [Info("Sign the AS4 Message if necessary")]
    [Description("This step signs the AS4 Message if signing is enabled in the Sending PMode")]
    public class SignAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IConfig _config;
        private readonly ICertificateRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class
        /// </summary>
        public SignAS4MessageStep() 
            : this(Config.Instance, Registry.Instance.CertificateRepository)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class. 
        /// Create Signing Step with a given Certificate Store Repository
        /// </summary>
        /// <param name="config"></param>
        /// <param name="repository">
        /// </param>
        public SignAS4MessageStep(
            IConfig config,
            ICertificateRepository repository)
        {
            _config = config;
            _repository = repository;
        }

        /// <summary>
        /// Sign the <see cref="AS4Message" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty)
            {
                Logger.Debug($"{messagingContext.LogTag} Incoming ");
                return await StepResult.SuccessAsync(messagingContext);
            }

            SendingProcessingMode pmode = 
                messagingContext.SendingPMode 
                ?? _config.GetReferencedSendingPMode(messagingContext.ReceivingPMode);

            if (pmode.Security.Signing.IsEnabled != true)
            {
                Logger.Debug(
                    $"{messagingContext.LogTag} No signing will be performend on the message " +
                    $"because the SendingPMode {pmode.Id} siging information is disabled");

                return await StepResult.SuccessAsync(messagingContext);
            }

            Logger.Info(
                $"{messagingContext.LogTag} Sign AS4Message with " + 
                $"given signing information of the SendingPMode {pmode.Id}");

            SignAS4Message(pmode, messagingContext.AS4Message);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private void SignAS4Message(SendingProcessingMode pmode, AS4Message message)
        {
            try
            {
                X509Certificate2 certificate = RetrieveCertificate(pmode);

                // Use GetRSAPrivateKey instead of HasPrivateKey to avoid 'Keyset does not exists' exception.
                if (certificate.GetRSAPrivateKey() == null)
                {
                    throw new CryptographicException(
                        "Cannot use certificate for signing: certificate does not have a private key. " +
                        "Please make sure that the private key is included in the certificate and is marked as 'Exportable'");
                }

                CalculateSignatureConfig settings = 
                    CreateSignConfig(certificate, pmode.Security.Signing);

                message.Sign(settings);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }
                Logger.Trace(exception.StackTrace);
                throw;
            }
        }

        private X509Certificate2 RetrieveCertificate(SendingProcessingMode pmode)
        {
            Signing signInfo = pmode.Security.Signing;

            if (signInfo.SigningCertificateInformation == null)
            {
                throw new ConfigurationErrorsException(
                    "No signing certificate information found " +
                    $"in Sending PMode {pmode.Id} to perform signing. " +
                    "Please provide either a <CertificateFindCriteria/> or <PrivateKeyCertificate/> tag to the Security.Signing element");
            }

            if (signInfo.SigningCertificateInformation is CertificateFindCriteria certFindCriteria)
            {
                return _repository.GetCertificate(
                    findType: certFindCriteria.CertificateFindType,
                    privateKeyReference: certFindCriteria.CertificateFindValue);
            }

            if (signInfo.SigningCertificateInformation is PrivateKeyCertificate embeddedCertInfo)
            {
                return new X509Certificate2(
                    rawData: Convert.FromBase64String(embeddedCertInfo.Certificate),
                    password: embeddedCertInfo.Password,
                    keyStorageFlags:
                        X509KeyStorageFlags.Exportable
                        | X509KeyStorageFlags.MachineKeySet
                        | X509KeyStorageFlags.PersistKeySet);
            }

            throw new NotSupportedException(
                "The signing certificate information specified in the PMode could not be used to retrieve the certificate. " +
                "Please provide either a <CertificateFindCriteria/> or <PrivateKeyCertificate/> tag to the Security.Signing element");
        }

        private static CalculateSignatureConfig CreateSignConfig(X509Certificate2 signCertificate, Signing settings)
        {
            return new CalculateSignatureConfig(
                signingCertificate: signCertificate,
                referenceTokenType: settings.KeyReferenceMethod,
                signingAlgorithm: settings.Algorithm,
                hashFunction: settings.HashFunction);
        }
    }
}