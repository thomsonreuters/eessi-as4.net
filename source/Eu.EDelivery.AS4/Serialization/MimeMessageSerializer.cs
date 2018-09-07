using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Streaming;
using MimeKit;
using MimeKit.IO;
using NLog;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// Serialize <see cref="AS4Message" /> to a <see cref="Stream" />
    /// </summary>
    public class MimeMessageSerializer : ISerializer
    {
        private readonly ISerializer _soapSerializer;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeMessageSerializer"/> class. 
        /// Create a MIME Serializer of the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="serializer">
        /// </param>
        public MimeMessageSerializer(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            _soapSerializer = serializer;
        }

        public Task SerializeAsync(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return Task.Run(() => Serialize(message, stream, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Serialize <see cref="AS4Message" /> to a <see cref="Stream" />
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        public void Serialize(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

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
            using (var bodyPartStream = new MemoryStream(4096))
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
            bodyMultipart.ContentType.Charset = contentType.Charset;
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
                var tempStream = new VirtualStream(forAsync: true);
                attachment.Content.CopyTo(tempStream);
                tempStream.Position = 0;
                attachment.UpdateContent(tempStream, attachment.ContentType);
            }

            try
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
            catch (ArgumentException ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                throw new NotSupportedException($"Attachment {attachment.Id} has a content-type that is not supported ({attachment.ContentType}).");
            }
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
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            var memoryStream = new MemoryStream(
                Encoding.UTF8.GetBytes($"Content-Type: {contentType}\r\n\r\n"));

            var chainedStream = new ChainedStream();
            chainedStream.Add(memoryStream, leaveOpen: false);
            chainedStream.Add(inputStream, leaveOpen: true);

            try
            {
                return await ParseStreamToAS4MessageAsync(chainedStream, contentType, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                // Since the stream has been read, make sure that all
                // parts of the chained-stream are re-positioned to the
                // beginning of each stream.
                chainedStream.Position = 0;
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

            foreach (var userMessage in message.UserMessages)
            {
                IEnumerable<PartInfo> referencedPartInfos =
                    userMessage.PayloadInfo ?? Enumerable.Empty<PartInfo>();

                foreach (Attachment a in BodyPartsAsAttachments(bodyParts, referencedPartInfos))
                {
                    message.AddAttachment(a);
                }
            }

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
                throw new InvalidMessageException(
                    "The use of MIME is not consistent with the required usage in this specification", exception);
            }
        }

        private static IEnumerable<Attachment> BodyPartsAsAttachments(
            IReadOnlyList<MimePart> bodyParts, 
            IEnumerable<PartInfo> referencedPartInfos)
        {
            const int startAfterSoapHeader = 1;
            for (int i = startAfterSoapHeader; i < bodyParts.Count; i++)
            {
                MimePart bodyPart = bodyParts[i];

                (bool hasValue, PartInfo value) = SelectReferencedPartInfo(bodyPart.ContentId, referencedPartInfos);
                if (hasValue)
                {
                    yield return new Attachment(
                        id: bodyPart.ContentId,
                        content: bodyPart.ContentObject.Open(),
                        contentType: bodyPart.ContentType.MimeType,
                        props: value.Properties); ;
                }
                else
                {
                    Logger.Warn($"Attachment {bodyPart.ContentId} will be ignored because no matching <PartInfo /> is found");
                }
            }
        }

        private static (bool, PartInfo) SelectReferencedPartInfo(
            string attachmentId, 
            IEnumerable<PartInfo> referencedPartInfos)
        {
            PartInfo partInfo = referencedPartInfos.FirstOrDefault(i => i.Href?.Contains(attachmentId) == true);

            return (partInfo != null, partInfo);
        }
    }
}