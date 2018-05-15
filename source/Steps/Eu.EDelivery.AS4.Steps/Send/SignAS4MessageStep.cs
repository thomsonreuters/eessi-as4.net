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
        private readonly ICertificateRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class
        /// </summary>
        public SignAS4MessageStep()
        {
            _repository = Registry.Instance.CertificateRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class. 
        /// Create Signing Step with a given Certificate Store Repository
        /// </summary>
        /// <param name="repository">
        /// </param>
        public SignAS4MessageStep(ICertificateRepository repository)
        {
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

            if (messagingContext.SendingPMode?.Security.Signing.IsEnabled != true)
            {
                Logger.Debug(
                    $"{messagingContext.LogTag} No signing will be performend on the message " + 
                    $"because the SendingPMode {messagingContext.SendingPMode?.Id} siging information is disabled");

                return await StepResult.SuccessAsync(messagingContext);
            }

            TrySignAS4Message(messagingContext);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private void TrySignAS4Message(MessagingContext context)
        {
            try
            {
                Logger.Info(
                    $"{context.LogTag} Sign AS4Message with given signing information of the SendingPMode {context.SendingPMode.Id}");

                SignAS4Message(context);
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

        private void SignAS4Message(MessagingContext context)
        {
            X509Certificate2 certificate = RetrieveCertificate(context);

            if (!certificate.HasPrivateKey)
            {
                throw new CryptographicException(
                    $"{context.LogTag} Cannot use certificate for signing: certificate does not have a private key. " +
                    "Please make sure that the private key is included in the certificate and is marked as Exportable");
            }

            CalculateSignatureConfig calculateSignatureConfig = CreateSignConfig(certificate, context.SendingPMode);

            context.AS4Message.Sign(calculateSignatureConfig);
        }

        private X509Certificate2 RetrieveCertificate(MessagingContext messagingContext)
        {
            Signing signInfo = messagingContext.SendingPMode.Security.Signing;

            if (signInfo.SigningCertificateInformation == null)
            {
                throw new ConfigurationErrorsException(
                    $"{messagingContext.LogTag} No signing certificate information found " + 
                    $"in Sending PMode {messagingContext.SendingPMode.Id} to perform signing. " +
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

        private static CalculateSignatureConfig CreateSignConfig(X509Certificate2 signCertificate, SendingProcessingMode pmode)
        {
            Signing signing = pmode.Security.Signing;

            return new CalculateSignatureConfig(
                signingCertificate: signCertificate,
                referenceTokenType: signing.KeyReferenceMethod,
                signingAlgorithm: signing.Algorithm,
                hashFunction: signing.HashFunction);
        }
    }
}