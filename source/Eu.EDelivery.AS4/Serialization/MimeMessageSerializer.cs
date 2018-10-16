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
        private static readonly Lazy<FormatOptions> FormatOptions =
            new Lazy<FormatOptions>(() =>
            {
                var options = new FormatOptions();
                foreach (HeaderId headerId in Enum.GetValues(typeof(HeaderId)).Cast<HeaderId>())
                {
                    options.HiddenHeaders.Add(headerId);
                }

                return options;
            }, LazyThreadSafetyMode.PublicationOnly);

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

        /// <summary>
        /// Asynchronously serializes the given <see cref="AS4Message"/> to a given <paramref name="output"/> stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="output">The destination stream to where the message should be written.</param>
        /// <param name="cancellation">The token to control the cancellation of the serialization.</param>
        public async Task SerializeAsync(
            AS4Message message, 
            Stream output, 
            CancellationToken cancellation = default(CancellationToken))
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            try
            {
                await SerializeToMimeStreamAsync(message, output, cancellation);
            }
            catch (Exception exception)
            {
                throw new FormatException("An error occured while serializing the MIME message", exception);
            }
        }

        private async Task SerializeToMimeStreamAsync(AS4Message message, Stream stream, CancellationToken cancellationToken)
        {
            using (var bodyPartStream = new MemoryStream(4096))
            {
                _soapSerializer.Serialize(message, bodyPartStream, cancellationToken);

                MimeMessage mimeMessage = CreateMimeMessage(message, bodyPartStream);
                foreach (Attachment attachment in message.Attachments)
                {
                    AddAttachmentToMultipart((Multipart) mimeMessage.Body, attachment);
                }

                await mimeMessage.WriteToAsync(FormatOptions.Value, stream, cancellationToken);
            }
        }

        /// <summary>
        /// Synchronously serializes the given <see cref="AS4Message"/> to a given <paramref name="output"/> stream.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="output">The destination stream to where the message should be written.</param>
        /// <param name="cancellation">The token to control the cancellation of the serialization.</param>
        public void Serialize(
            AS4Message message, 
            Stream output, 
            CancellationToken cancellation = default(CancellationToken))
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            try
            {
                SerializeToMimeStream(message, output, cancellation);
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

                foreach (Attachment attachment in message.Attachments)
                {
                    AddAttachmentToMultipart((Multipart) mimeMessage.Body, attachment);
                }

                mimeMessage.WriteTo(FormatOptions.Value, stream, cancellationToken);
            }
        }

        private static MimeMessage CreateMimeMessage(AS4Message message ,Stream bodyPartStream)
        {
            var bodyPart = new MimePart("application", "soap+xml");
            bodyPart.ContentType.Parameters["charset"] = Encoding.UTF8.HeaderName.ToLowerInvariant();
            bodyPart.ContentObject = new ContentObject(bodyPartStream);

            var bodyMultipart = new Multipart("related") { bodyPart };
            bodyMultipart.ContentType.Parameters["type"] = bodyPart.ContentType.MimeType;

            ReassignContentType(bodyMultipart, message.ContentType);

            return new MimeMessage { Body = bodyMultipart };
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
                Logger.Error(ex);
                throw new NotSupportedException($"Attachment {attachment.Id} has a content-type that is not supported ({attachment.ContentType}).");
            }
        }

        /// <summary>
        /// Asynchronously deserializes the given <paramref name="input"/> stream to an <see cref="AS4Message"/> model.
        /// </summary>
        /// <param name="input">The source stream from where the message should be read.</param>
        /// <param name="contentType">The content type required to correctly deserialize the message into different MIME parts.</param>
        /// <param name="cancellation">The token to control the cancellation of the deserialization.</param>
        public async Task<AS4Message> DeserializeAsync(
            Stream input,
            string contentType,
            CancellationToken cancellation = default(CancellationToken))
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            var memoryStream = new MemoryStream(
                Encoding.UTF8.GetBytes($"Content-Type: {contentType}\r\n\r\n"));

            var chainedStream = new ChainedStream();
            chainedStream.Add(memoryStream, leaveOpen: false);
            chainedStream.Add(input, leaveOpen: true);

            try
            {
                return await ParseStreamToAS4MessageAsync(chainedStream, contentType, cancellation).ConfigureAwait(false);
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