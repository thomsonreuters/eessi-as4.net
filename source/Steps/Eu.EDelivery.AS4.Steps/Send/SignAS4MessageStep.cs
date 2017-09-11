using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Strategies;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the MSH signs the AS4 UserMessage
    /// </summary>
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty)
            {
                return await StepResult.SuccessAsync(messagingContext);
            }

            if (messagingContext.SendingPMode?.Security.Signing.IsEnabled != true)
            {
                Logger.Info($"{messagingContext.EbmsMessageId} Sending PMode {messagingContext.SendingPMode?.Id} Signing is disabled");
                return await StepResult.SuccessAsync(messagingContext);
            }

            TrySignAS4Message(messagingContext, cancellationToken);
            ResetAttachmentContents(messagingContext.AS4Message);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private void TrySignAS4Message(MessagingContext message, CancellationToken cancellationToken)
        {
            try
            {
                Logger.Info($"{message.EbmsMessageId} Sign AS4 Message with given Signing Information");
                SignAS4Message(message, cancellationToken);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }
                Logger.Debug(exception.StackTrace);
                throw ThrowCommonSigningException(exception.Message, exception);
            }
        }

        private void SignAS4Message(MessagingContext message, CancellationToken cancellationToken)
        {
            X509Certificate2 certificate = RetrieveCertificate(message);

            if (!certificate.HasPrivateKey)
            {
                throw ThrowCommonSigningException($"{message.EbmsMessageId} Certificate hasn't a private key");
            }

            ISigningStrategy signingStrategy = CreateSignStrategy(message, certificate, cancellationToken);
            message.AS4Message.SecurityHeader.Sign(signingStrategy);
        }

        private X509Certificate2 RetrieveCertificate(MessagingContext message)
        {
            Model.PMode.Signing signing = message.SendingPMode.Security.Signing;

            X509Certificate2 certificate = _repository.GetCertificate(signing.PrivateKeyFindType, signing.PrivateKeyFindValue);

            return certificate;
        }

        private static ApplicationException ThrowCommonSigningException(string description, Exception innerException = null)
        {
            Logger.Error(description);
            return new ApplicationException(description, innerException);
        }

        private static ISigningStrategy CreateSignStrategy(MessagingContext messagingContext, X509Certificate2 certificate, CancellationToken cancellationToken)
        {
            AS4Message message = messagingContext.AS4Message;
            Model.PMode.Signing signing = messagingContext.SendingPMode.Security.Signing;

            SigningStrategyBuilder builder = new SigningStrategyBuilder(messagingContext.AS4Message, cancellationToken)
                .WithSecurityTokenReference(signing.KeyReferenceMethod)
                .WithSignatureAlgorithm(signing.Algorithm)
                .WithCertificate(certificate)
                .WithSigningId(message.SigningId, signing.HashFunction);

            foreach (Attachment attachment in message.Attachments)
            {
                builder.WithAttachment(attachment, signing.HashFunction);
            }

            return builder.Build();
        }

        private static void ResetAttachmentContents(AS4Message as4Message)
        {
            foreach (Attachment attachment in as4Message.Attachments)
            {
                attachment.ResetContentPosition();
            }
        }
    }
}