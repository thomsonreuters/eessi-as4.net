using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Strategies.Retriever;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// This Transformer is responsible for creating a <see cref="MessagingContext"/> that contains a <see cref="SubmitMessage"/> for the payload it has received. 
    /// </summary>
    /// <seealso cref="ITransformer" />
    public class SubmitPayloadTransformer : ITransformer
    {
        private readonly IConfig _config;
        private static MimeTypeRepository _mimeTypeRepository;

        private string _pmodeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitPayloadTransformer"/> class.
        /// </summary>
        public SubmitPayloadTransformer() : this(Config.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitPayloadTransformer" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public SubmitPayloadTransformer(IConfig configuration)
        {
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }

            _config = configuration;
            _mimeTypeRepository = new MimeTypeRepository();
        }

        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties)
        {
            _pmodeId = properties.ReadMandatoryProperty("SendingPMode");
        }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            SendingProcessingMode sendingPMode = _config.GetSendingPMode(_pmodeId);

            (string payloadId, string payloadPath) = GetPayloadInfo(message);

            var submit = new SubmitMessage
            {
                MessageInfo = {MessageId = IdentifierFactory.Instance.Create()},
                Collaboration =
                {
                  AgreementRef  = {PModeId = sendingPMode.Id}
                },
                Payloads = new[]
                {
                    new Payload
                    {
                        Id = payloadId,
                        MimeType = message.ContentType,
                        Location = payloadPath
                    }
                }
            };

            return Task.FromResult(new MessagingContext(submit));
        }

        private static (string payloadId, string payloadPath) GetPayloadInfo(ReceivedMessage incoming)
        {
            if (incoming.UnderlyingStream is FileStream file)
            {
                string payloadPath = file.Name;
                string payloadId = Path.GetFileNameWithoutExtension(new FileInfo(payloadPath).Name);
                return (payloadId, FilePayloadRetriever.Key + payloadPath);
            }
            else
            {
                string ext = _mimeTypeRepository.GetExtensionFromMimeType(incoming.ContentType);

                string payloadId = Guid.NewGuid().ToString();
                string payloadPath = Path.Combine(Path.GetTempPath(), payloadId + ext);

                using (var tempStream = new FileStream(payloadPath, FileMode.Create, FileAccess.Write))
                {
                    incoming.UnderlyingStream.CopyTo(tempStream);
                }

                return (payloadId, TempFilePayloadRetriever.Key + payloadPath);
            }
        }
    }
}
