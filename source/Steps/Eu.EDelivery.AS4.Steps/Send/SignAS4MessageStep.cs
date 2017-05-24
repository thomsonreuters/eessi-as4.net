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
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Thrown when Signing Fails</exception>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (internalMessage.AS4Message.IsEmpty)
            {
                return await StepResult.SuccessAsync(internalMessage);
            }

            if (internalMessage.SendingPMode?.Security.Signing.IsEnabled != true)
            {
                Logger.Info($"{internalMessage.Prefix} Sending PMode {internalMessage.SendingPMode?.Id} Signing is disabled");
                return await StepResult.SuccessAsync(internalMessage);
            }

            TrySignAS4Message(internalMessage, cancellationToken);
            ResetAttachmentContents(internalMessage.AS4Message);

            return await StepResult.SuccessAsync(internalMessage);
        }

        private void TrySignAS4Message(InternalMessage message, CancellationToken cancellationToken)
        {
            try
            {
                Logger.Info($"{message.Prefix} Sign AS4 Message with given Signing Information");
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
                throw ThrowCommonSigningException(message, exception.Message, exception);
            }
        }

        private void SignAS4Message(InternalMessage message, CancellationToken cancellationToken)
        {
            X509Certificate2 certificate = RetrieveCertificate(message);

            if (!certificate.HasPrivateKey)
            {
                throw ThrowCommonSigningException(
                    message,
                    $"{message.Prefix} Certificate hasn't a private key");
            }

            ISigningStrategy signingStrategy = CreateSignStrategy(message, certificate, cancellationToken);
            message.AS4Message.SecurityHeader.Sign(signingStrategy);
        }

        private X509Certificate2 RetrieveCertificate(InternalMessage message)
        {
            Model.PMode.Signing signing = message.SendingPMode.Security.Signing;

            X509Certificate2 certificate = _repository.GetCertificate(signing.PrivateKeyFindType, signing.PrivateKeyFindValue);

            return certificate;
        }

        private static AS4Exception ThrowCommonSigningException(InternalMessage message, string description, Exception innerException = null)
        {
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(message.AS4Message.MessageIds)
                .WithSendingPMode(message.SendingPMode)
                .Build();
        }

        private static ISigningStrategy CreateSignStrategy(InternalMessage internalMessage, X509Certificate2 certificate, CancellationToken cancellationToken)
        {
            AS4Message message = internalMessage.AS4Message;
            Model.PMode.Signing signing = internalMessage.SendingPMode.Security.Signing;

            SigningStrategyBuilder builder = new SigningStrategyBuilder(message, cancellationToken)
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