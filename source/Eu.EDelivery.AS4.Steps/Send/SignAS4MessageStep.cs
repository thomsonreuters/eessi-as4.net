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
        public SignAS4MessageStep() : this(Registry.Instance.CertificateRepository) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class. 
        /// Create Signing Step with a given Certificate Store Repository
        /// </summary>
        /// <param name="repository">
        /// </param>
        public SignAS4MessageStep(ICertificateRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        /// <summary>
        /// Sign the <see cref="AS4Message" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty)
            {
                Logger.Debug("No signing will be performed on the message because it's empty");
                return await StepResult.SuccessAsync(messagingContext);
            }

            SendingProcessingMode pmode = messagingContext.SendingPMode;
            if (pmode == null)
            {
                Logger.Debug(
                    "No signing will be performend on the message " +
                    "because no SendingPMode is found to get the signing configuration from");

                return await StepResult.FailedAsync(messagingContext);
            }

            if (pmode.Security.Signing.IsEnabled != true)
            {
                Logger.Debug(
                    "No signing will be performend on the message " +
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
                    $"in SendingPMode {pmode.Id} to perform signing. " +
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
                $"The signing certificate information specified in the SendingPMode {pmode.Id} could not be used to retrieve the certificate. " +
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