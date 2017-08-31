using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Streaming;
using MimeKit;
using MimeKit.IO;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Serialize <see cref="AS4Message" /> to a <see cref="Stream" />
    /// </summary>
    public class MimeMessageSerializer : ISerializer
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
            _soapSerializer = serializer;
        }

        public Task SerializeAsync(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            return Task.Run(() => this.Serialize(message, stream, cancellationToken), cancellationToken);
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
                throw new FormatException("An error occured while serializing the MIME message", exception);
            }
        }

        private void SerializeToMimeStream(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            using (var bodyPartStream = new MemoryStream())
            {
                _soapSerializer.Serialize(message, bodyPartStream, cancellationToken);

                MimeMessage mimeMessage = CreateMimeMessage(message, bodyPartStream);
                FormatOptions formatOptions = GetFormatOptions();

                mimeMessage.WriteTo(formatOptions, stream, cancellationToken);
            }
        }

        private static MimeMessage CreateMimeMessage(AS4Message message, Stream bodyPartStream)
        {
            MimePart bodyPart = GetBodyPartFromStream(bodyPartStream);
            Multipart bodyMultipart = CreateMultiPartFromBodyPart(bodyPart);

            var mimeMessage = new MimeMessage { Body = bodyMultipart };
            ReassignContentType(bodyMultipart, message.ContentType);
            AddAttachmentsToBodyMultiPart(message, bodyMultipart);

            return mimeMessage;
        }

        private static void AddAttachmentsToBodyMultiPart(AS4Message message, Multipart bodyMultipart)
        {
            foreach (Attachment attachment in message.Attachments)
            {
                AddAttachmentToMultipart(bodyMultipart, attachment);
            }
        }

        private static MimePart GetBodyPartFromStream(Stream stream)
        {
            var bodyPart = new MimePart("application", "soap+xml");
            bodyPart.ContentType.Parameters["charset"] = Encoding.UTF8.HeaderName.ToLowerInvariant();
            bodyPart.ContentObject = new ContentObject(stream);

            return bodyPart;
        }

        private static Multipart CreateMultiPartFromBodyPart(MimeEntity bodyPart)
        {
            var bodyMultipart = new Multipart("related") { bodyPart };
            bodyMultipart.ContentType.Parameters["type"] = bodyPart.ContentType.MimeType;

            return bodyMultipart;
        }

        private static void ReassignContentType(MimeEntity bodyMultipart, string type)
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

        private static void AddHeaderParametersToBodyMultiPart(MimeEntity bodyMultipart, ContentType contentType)
        {
            foreach (Parameter item in contentType.Parameters)
            {
                bodyMultipart.ContentType.Parameters.Add(item);
            }
        }

        private static void AddAttachmentToMultipart(Multipart bodyMultipart, Attachment attachment)
        {
            // A stream that is passed to a ContentObject must be seekable.  If this is not the case,
            // we'll have to create a new stream which is seekable and assign it to the Attachment.Content.

            if (attachment.Content.CanSeek == false)
            {
                var tempStream = new VirtualStream();
                attachment.Content.CopyTo(tempStream);
                tempStream.Position = 0;
                attachment.Content = tempStream;
            }

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

        // ReSharper disable once InconsistentNaming : double underscore to indicate that this field should not be used directly.
        private static FormatOptions __formatOptions;

        private static FormatOptions GetFormatOptions()
        {
            if (__formatOptions == null)
            {
                __formatOptions = new FormatOptions();
                foreach (HeaderId headerId in Enum.GetValues(typeof(HeaderId)).Cast<HeaderId>())
                {
                    __formatOptions.HiddenHeaders.Add(headerId);
                }
            }

            return __formatOptions;
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

            var memoryStream = new MemoryStream(
                Encoding.UTF8.GetBytes($"Content-Type: {contentType}\r\n\r\n"));

            var chainedStream = new ChainedStream();
            chainedStream.Add(memoryStream, leaveOpen: true);
            chainedStream.Add(inputStream, leaveOpen: true);

            return await ParseStreamToAS4MessageAsync(chainedStream, contentType, cancellationToken).ConfigureAwait(false);

        }

        private void PreConditions(Stream inputStream, string contentType)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }
        }

        private async Task<AS4Message> ParseStreamToAS4MessageAsync(
            Stream inputStream,
            string contentType,
            CancellationToken cancellationToken)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            List<MimePart> bodyParts = TryParseBodyParts(inputStream, cancellationToken);
            Stream envelopeStream = bodyParts.First().ContentObject.Open();

            AS4Message message = await _soapSerializer
                .DeserializeAsync(envelopeStream, contentType, cancellationToken).ConfigureAwait(false);

            AddBodyPartsAsAttachmentsToMessage(bodyParts, message);

            return message;
        }

        private static List<MimePart> TryParseBodyParts(Stream inputStream, CancellationToken cancellationToken)
        {
            try
            {
                MimeMessage mimeMessage = new MimeParser(inputStream, persistent: true).ParseMessage(cancellationToken);
                List<MimePart> bodyParts = mimeMessage.BodyParts.OfType<MimePart>().ToList();
                if (bodyParts.Count <= 0)
                {
                    throw new FormatException("MIME Body Parts are empty");
                }

                return bodyParts;
            }
            catch (Exception exception)
            {
                throw CreateAS4MimeInconsistencyException(exception);
            }
        }

        private static FormatException CreateAS4MimeInconsistencyException(Exception exception)
        {
            return new FormatException(
                "The use of MIME is not consistent with the required usage in this specification", exception);
        }

        private static void AddBodyPartsAsAttachmentsToMessage(IReadOnlyList<MimePart> bodyParts, AS4Message message)
        {
            const int startAfterSoapHeader = 1;
            for (int i = startAfterSoapHeader; i < bodyParts.Count; i++)
            {
                MimePart bodyPart = bodyParts[i];
                Attachment attachment = CreateAttachment(bodyPart);

                (bool hasValue, PartInfo value) partInfo = SelectReferencedPartInfo(attachment, message);

                if (partInfo.hasValue)
                {
                    attachment.Properties = partInfo.value.Properties;
                    message.AddAttachment(attachment);
                }
            }
        }

        private static Attachment CreateAttachment(MimePart bodyPart)
        {
            return new Attachment(id: bodyPart.ContentId)
            {
                Content = bodyPart.ContentObject.Open(),
                ContentType = bodyPart.ContentType.MimeType,
            };
        }

        private static (bool, PartInfo) SelectReferencedPartInfo(Attachment attachment, AS4Message message)
        {
            PartInfo partInfo = message.PrimaryUserMessage?.PayloadInfo
                            .FirstOrDefault(i => i.Href?.Contains(attachment.Id) == true);

            return (partInfo != null, partInfo);
        }
    }
}