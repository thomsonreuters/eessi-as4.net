using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using MimeKit;
using MimeKit.IO;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Serialize <see cref="AS4Message" /> to a <see cref="Stream" />
    /// </summary>
    internal class MimeMessageSerializer : ISerializer
    {
        private readonly ISerializer _soapSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeMessageSerializer"/> class. 
        /// Create a MIME Serializer of the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="serializer">
        /// </param>
        public MimeMessageSerializer(ISerializer serializer)
        {
            this._soapSerializer = serializer;
        }

        /// <summary>
        /// Serialize <see cref="AS4Message" /> to a <see cref="Stream" />
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        public void Serialize(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                SerializeToMimeStream(message, stream, cancellationToken);
            }
            catch (Exception exception)
            {
                throw new AS4ExceptionBuilder()
                    .WithInnerException(exception).WithMessageIds(message.MessageIds).Build();
            }
        }

        private void SerializeToMimeStream(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                this._soapSerializer.Serialize(message, memoryStream, cancellationToken);

                MimeMessage mimeMessage = CreateMimeMessage(message, memoryStream);
                FormatOptions formatOptions = GetFormatOptions();

                mimeMessage.WriteTo(formatOptions, stream, cancellationToken);
            }
        }

        private MimeMessage CreateMimeMessage(AS4Message message, Stream stream)
        {
            MimePart bodyPart = GetBodyPartFromStream(stream);
            Multipart bodyMultipart = CreateMultiPartFromBodyPart(bodyPart);

            var mimeMessage = new MimeMessage { Body = bodyMultipart };
            ReassignContentType(bodyMultipart, message.ContentType);
            AddAttachmentsToBodyMultiPart(message, bodyMultipart);

            return mimeMessage;
        }

        private void AddAttachmentsToBodyMultiPart(AS4Message message, Multipart bodyMultipart)
        {
            foreach (Attachment attachment in message.Attachments)
                AddAttachmentToMultipart(bodyMultipart, attachment);
        }

        private MimePart GetBodyPartFromStream(Stream memoryStream)
        {
            var bodyPart = new MimePart("application", "soap+xml");
            bodyPart.ContentType.Parameters["charset"] = Encoding.UTF8.HeaderName.ToLowerInvariant();
            bodyPart.ContentObject = new ContentObject(memoryStream);

            return bodyPart;
        }

        private Multipart CreateMultiPartFromBodyPart(MimeEntity bodyPart)
        {
            var bodyMultipart = new Multipart("related") {bodyPart};
            bodyMultipart.ContentType.Parameters["type"] = bodyPart.ContentType.MimeType;

            return bodyMultipart;
        }

        private void ReassignContentType(MimeEntity bodyMultipart, string type)
        {
            ContentType contentType = ContentType.Parse(type);

            bodyMultipart.ContentType.Boundary = contentType.Boundary;
            bodyMultipart.ContentType.Charset = contentType.Boundary;
            bodyMultipart.ContentType.Format = contentType.Format;
            bodyMultipart.ContentType.MediaSubtype = contentType.MediaSubtype;
            bodyMultipart.ContentType.MediaType = contentType.MediaType;
            bodyMultipart.ContentType.Name = contentType.Name;
            bodyMultipart.ContentType.Parameters.Clear();

            AddHeaderParametersToBodyMultiPart(bodyMultipart, contentType);
        }

        private void AddHeaderParametersToBodyMultiPart(MimeEntity bodyMultipart, ContentType contentType)
        {
            foreach (Parameter item in contentType.Parameters)
                bodyMultipart.ContentType.Parameters.Add(item);
        }

        private void AddAttachmentToMultipart(Multipart bodyMultipart, Attachment attachment)
        {
            var attachmentMimePart = new MimePart(attachment.ContentType)
            {
                ContentId = attachment.Id,
                ContentObject = new ContentObject(attachment.Content),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Binary

                // We need to explicitly set this to binary, 
                // otherwise we can enounter issues with CRLFs & signing.
            };
            bodyMultipart.Add(attachmentMimePart);
        }

        private FormatOptions GetFormatOptions()
        {
            var formatOptions = new FormatOptions();
            foreach (HeaderId headerId in Enum.GetValues(typeof(HeaderId)).Cast<HeaderId>())
                formatOptions.HiddenHeaders.Add(headerId);

            return formatOptions;
        }

        /// <summary>
        /// Parse the MIME message to a <see cref="AS4Message" />
        /// </summary>
        /// <param name="inputStream">RequestStream that contains the MIME message</param>
        /// <param name="contentType">Multi-Part Content ExceptionType</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="AS4Message" /> that wraps the Envelope and Payloads Streams</returns>
        public async Task<AS4Message> DeserializeAsync(
            Stream inputStream,
            string contentType,
            CancellationToken cancellationToken)
        {
            PreConditions(inputStream, contentType);

            // I'm not 100% certain this doesn't cause a leak. 
            // It shouldn't because we only prefix it with a MemoryStream.
            var memoryStream = new MemoryStream(
                Encoding.UTF8.GetBytes($"Content-Type: {contentType}\r\n\r\n"));

            var chainedStream = new ChainedStream();
            chainedStream.Add(memoryStream, leaveOpen: false);
            chainedStream.Add(inputStream, leaveOpen: true);

            return await ParseStreamToAS4MessageAsync(chainedStream, contentType, cancellationToken);
        }

        private void PreConditions(Stream inputStream, string contentType)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));
        }

        private async Task<AS4Message> ParseStreamToAS4MessageAsync(
            Stream inputStream,
            string contentType,
            CancellationToken cancellationToken)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            List<MimePart> bodyParts = TryParseBodyParts(inputStream, cancellationToken);
            Stream envelopeStream = bodyParts.First().ContentObject.Open();

            AS4Message message = await this._soapSerializer
                .DeserializeAsync(envelopeStream, contentType, cancellationToken);

            AddBodyPartsAsAttachmentsToMessage(bodyParts, message);

            return message;
        }

        private List<MimePart> TryParseBodyParts(Stream inputStream, CancellationToken cancellationToken)
        {
            try
            {
                MimeMessage mimeMessage = new MimeParser(inputStream).ParseMessage(cancellationToken);
                List<MimePart> bodyParts = mimeMessage.BodyParts.OfType<MimePart>().ToList();
                if (bodyParts.Count <= 0) throw new AS4Exception("MIME Body Parts are empty");

                return bodyParts;
            }
            catch (Exception exception)
            {
                throw ThrowAS4MimeInconsistencyException(exception);
            }
        }

        private AS4Exception ThrowAS4MimeInconsistencyException(Exception exception)
        {
            return new AS4ExceptionBuilder()
                .WithInnerException(exception)
                .WithDescription("The use of MIME is not consistent with the required usage in this specification")
                .WithErrorCode(ErrorCode.Ebms0007)
                .WithExceptionType(ExceptionType.MimeInconsistency)
                .Build();
        }

        private void AddBodyPartsAsAttachmentsToMessage(IReadOnlyList<MimePart> bodyParts, AS4Message message)
        {
            const int startAfterSoapHeader = 1;
            for (int i = startAfterSoapHeader; i < bodyParts.Count; i++)
            {
                MimePart bodyPart = bodyParts[i];
                Attachment attachment = CreateAttachment(bodyPart, message);
                message.AddAttachment(attachment);
            }
        }

        private Attachment CreateAttachment(MimePart bodyPart, AS4Message message)
        {
            Attachment attachment = CreateDefaultAttachment(bodyPart);
            AssignPartProperties(attachment, message);

            return attachment;
        }

        private Attachment CreateDefaultAttachment(MimePart bodyPart)
        {
            return new Attachment(id: bodyPart.ContentId)
            {
                Content = bodyPart.ContentObject.Stream,
                ContentType = bodyPart.ContentType.MimeType,
            };
        }

        private void AssignPartProperties(Attachment attachment, AS4Message message)
        {
            PartInfo partInfo = message.PrimaryUserMessage?.PayloadInfo
                .FirstOrDefault(i => i.Href?.Contains(attachment.Id) == true);

            if (partInfo != null) attachment.Properties = partInfo.Properties;
        }
    }
}