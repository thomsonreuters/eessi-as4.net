using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Eu.EDelivery.AS4.PayloadService.Infrastructure
{
    internal class MultipartPayloadReader : IDisposable
    {
        private readonly Stream _contentStream;
        private readonly string _contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartPayloadReader"/> class.
        /// </summary>
        private MultipartPayloadReader(Stream content, string contentType)
        {
            _contentStream = content;
            _contentType = contentType;
        }

        /// <summary>
        /// Try to create a <see cref="MultipartPayloadReader"/> instance.
        /// </summary>
        /// <param name="contentStream"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static (bool success, MultipartPayloadReader reader) TryCreate(Stream contentStream, string contentType)
        {
            if (!IsMultipartContentType(contentType))
            {
                return (false, null);
            }

            return (true, new MultipartPayloadReader(contentStream, contentType));
        }

        private static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Start reading the HTTP Request
        /// </summary>
        /// <param name="onNextSection"></param>
        /// <returns></returns>
        public async Task StartReading(Func<Payload, Task> onNextSection)
        {
            var multipartReader = new MultipartReader(boundary: GetBoundary(_contentType), stream: _contentStream);
            MultipartSection section = await multipartReader.ReadNextSectionAsync();

            while (section != null)
            {
                bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (HasFileContentDisposition(contentDisposition))
                    {
                        await onNextSection(new Payload(section.Body, new PayloadMeta(contentDisposition.FileName.Value)));
                    }
                }

                section = await multipartReader.ReadNextSectionAsync();
            }
        }

        private static string GetBoundary(string contentType)
        {
            StringSegment boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(contentType).Boundary);

            if (string.IsNullOrWhiteSpace(boundary.Value))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > 100)
            {
                throw new InvalidDataException($"Multipart boundary length limit {100} exceeded.");
            }

            return boundary.Value;
        }

        private static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                       || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _contentStream?.Dispose();
        }
    }
}