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

        private AS4Message _as4Message;
        private InternalMessage _internalMessage;

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
                return ReturnSameInternalMessage(internalMessage);

            InitializeFields(internalMessage);
            TrySignAS4Message(cancellationToken);
            ResetAttachmentContents();

            return StepResult.SuccessAsync(internalMessage);
        }

        private Task<StepResult> ReturnSameInternalMessage(InternalMessage internalMessage)
        {
            this._logger.Info($"{internalMessage.Prefix} Sending PMode {internalMessage.AS4Message.SendingPMode?.Id} Signing is disabled");
            return StepResult.SuccessAsync(internalMessage);
        }

        private void InitializeFields(InternalMessage internalMessage)
        {
            this._internalMessage = internalMessage;
            this._as4Message = internalMessage.AS4Message;
        }

        private void TrySignAS4Message(CancellationToken cancellationToken)
        {
            try
            {
                this._logger.Info($"{this._internalMessage.Prefix} Sign AS4 Message with given Signing Information");
                SignAS4Message(cancellationToken);
            }
            catch (Exception exception)
            {
                this._logger.Error(exception.Message);
                throw ThrowCommonSigningException(exception.Message, exception);
            }
        }

        private void SignAS4Message(CancellationToken cancellationToken)
        {
            X509Certificate2 certificate = RetrieveCertificate();
            ISigningStrategy signingStrategy = CreateSignStrategy(certificate, cancellationToken);
            this._as4Message.SecurityHeader.Sign(signingStrategy);
        }

        private X509Certificate2 RetrieveCertificate()
        {
            Model.PMode.Signing signing = this._as4Message.SendingPMode.Security.Signing;
            X509Certificate2 certificate = this._repository
                .GetCertificate(signing.PrivateKeyFindType, signing.PrivateKeyFindValue);

            if (!certificate.HasPrivateKey)
                throw ThrowCommonSigningException($"{this._internalMessage.Prefix} Certificate hasn't a private key");

            return certificate;
        }

        private AS4Exception ThrowCommonSigningException(string description, Exception innerException = null)
        {
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(this._as4Message.MessageIds)
                .WithSendingPMode(this._internalMessage.AS4Message.SendingPMode)
                .Build();
        }

        private ISigningStrategy CreateSignStrategy(X509Certificate2 certificate, CancellationToken cancellationToken)
        {
            Model.PMode.Signing signing = this._as4Message.SendingPMode.Security.Signing;

            SigningStrategyBuilder builder = new SigningStrategyBuilder(this._as4Message, cancellationToken)
                .WithSecurityTokenReference(signing.KeyReferenceMethod)
                .WithSignatureAlgorithm(signing.Algorithm)
                .WithCertificate(certificate)
                .WithSigningId(this._as4Message.SigningId, signing.HashFunction);

            AddAttachmentsToBuilder(builder, signing);

            return builder.Build();
        }

        private void AddAttachmentsToBuilder(SigningStrategyBuilder builder, Model.PMode.Signing signing)
        {
            foreach (Attachment attachment in this._as4Message.Attachments)
                builder.WithAttachment(attachment, signing.HashFunction);
        }

        private void ResetAttachmentContents()
        {
            foreach (Attachment attachment in this._as4Message.Attachments)
                attachment.Content.Position = 0;
        }
    }
}