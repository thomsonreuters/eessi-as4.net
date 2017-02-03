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
        private readonly ICertificateRepository _repository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4Message"/> class
        /// </summary>
        public SignAS4MessageStep()
        {
            this._repository = Registry.Instance.CertificateRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class. 
        /// Create Signing Step with a given Certificate Store Repository
        /// </summary>
        /// <param name="repository">
        /// </param>
        public SignAS4MessageStep(ICertificateRepository repository)
        {
            this._repository = repository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Sign the <see cref="AS4Message" />
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Thrown when Signing Fails</exception>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (internalMessage.AS4Message.SendingPMode?.Security.Signing.IsEnabled != true)
            {
                return ReturnSameInternalMessage(internalMessage);
            }

            TrySignAS4Message(internalMessage, cancellationToken);
            ResetAttachmentContents(internalMessage.AS4Message);

            return StepResult.SuccessAsync(internalMessage);
        }

        private Task<StepResult> ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            this._logger.Info($"{internalMessage.Prefix} Sending PMode {internalMessage.AS4Message.SendingPMode?.Id} Signing is disabled");
            return StepResult.SuccessAsync(internalMessage);
        }

        private void TrySignAS4Message(InternalMessage message, CancellationToken cancellationToken)
        {
            try
            {
                this._logger.Info($"{message.Prefix} Sign AS4 Message with given Signing Information");
                SignAS4Message(message, cancellationToken);
            }
            catch (Exception exception)
            {
                this._logger.Error(exception.Message);
                throw ThrowCommonSigningException(message.AS4Message, exception.Message, exception);
            }
        }

        private void SignAS4Message(InternalMessage message, CancellationToken cancellationToken)
        {
            X509Certificate2 certificate = RetrieveCertificate(message.AS4Message);

            if (!certificate.HasPrivateKey)
            {
                throw ThrowCommonSigningException(message.AS4Message, $"{message.Prefix} Certificate hasn't a private key");
            }

            ISigningStrategy signingStrategy = CreateSignStrategy(message.AS4Message, certificate, cancellationToken);
            message.AS4Message.SecurityHeader.Sign(signingStrategy);
        }

        private X509Certificate2 RetrieveCertificate(AS4Message as4Message)
        {
            var certCriteria = GetCertificateFindValue(as4Message);

            X509Certificate2 certificate = this._repository.GetCertificate(certCriteria.Item1, certCriteria.Item2);

            return certificate;
        }

        protected virtual Tuple<X509FindType, string> GetCertificateFindValue(AS4Message as4Message)
        {
            Model.PMode.Signing signing = as4Message.SendingPMode.Security.Signing;

            return new Tuple<X509FindType, string>(signing.PrivateKeyFindType, signing.PrivateKeyFindValue);
        }

        private AS4Exception ThrowCommonSigningException(AS4Message as4Message, string description, Exception innerException = null)
        {
            this._logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(as4Message.MessageIds)
                .WithSendingPMode(as4Message.SendingPMode)
                .Build();
        }

        private static ISigningStrategy CreateSignStrategy(AS4Message message, X509Certificate2 certificate, CancellationToken cancellationToken)
        {
            Model.PMode.Signing signing = message.SendingPMode.Security.Signing;

            SigningStrategyBuilder builder = new SigningStrategyBuilder(message, cancellationToken)
                .WithSecurityTokenReference(signing.KeyReferenceMethod)
                .WithSignatureAlgorithm(signing.Algorithm)
                .WithCertificate(certificate)
                .WithSigningId(message.SigningId, signing.HashFunction);

            foreach (var attachment in message.Attachments)
            {
                builder.WithAttachment(attachment, signing.HashFunction);
            }

            return builder.Build();
        }

        private static void ResetAttachmentContents(AS4Message as4Message)
        {
            foreach (Attachment attachment in as4Message.Attachments)
            {
                attachment.Content.Position = 0;
            }
        }
    }
}