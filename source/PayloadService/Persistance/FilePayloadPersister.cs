using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Models;
using Microsoft.AspNetCore.Hosting;

namespace Eu.EDelivery.AS4.PayloadService.Persistance
{
    /// <summary>
    /// <see cref="IPayloadPersister"/> implementation to persist the <see cref="Payload"/> instances on the File System.
    /// </summary>
    internal sealed class FilePayloadPersister : IPayloadPersister
    {
        private const string OriginalFileNameKey = "originalfilename:";

        private readonly string _persistenceLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePayloadPersister"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public FilePayloadPersister(IHostingEnvironment environment)
        {
            _persistenceLocation = Path.Combine(environment.ContentRootPath, "Payloads");

            if (Directory.Exists(_persistenceLocation) == false)
            {
                Directory.CreateDirectory(_persistenceLocation);
            }
        }

        /// <summary>
        /// Save the given <paramref name="payload"/> with its Metadata
        /// to a specific implementation detail location.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task<string> SavePayload(Payload payload)
        {
            Guid id = Guid.NewGuid();

            WritePayloadMetadata(payload.Meta, id);
            await WritePayloadContent(payload.Content, id);

            return id.ToString();
        }

        private void WritePayloadMetadata(PayloadMeta payloadMeta, Guid id)
        {
            string metaDestinationPath = GetPersistanceFileLocationOf($"{id}.meta");

            using (FileStream metaFile = File.Create(metaDestinationPath))
            {
                using (var streamWriter = new StreamWriter(metaFile))
                {
                    streamWriter.WriteLine(OriginalFileNameKey + Path.GetFileName(payloadMeta.OriginalFileName.Trim('\"')));
                }
            }
        }

        private async Task WritePayloadContent(Stream payload, Guid id)
        {
            using (FileStream fileStream = File.Create(GetPersistanceFileLocationOf($"{id}")))
            {
                await payload.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Load a <see cref="Payload"/> with a given <paramref name="payloadId"/> 
        /// from the specific implementation detail location.
        /// </summary>
        /// <param name="payloadId"></param>
        /// <returns></returns>
        public Task<Payload> LoadPayload(string payloadId)
        {
            string filePath = GetPersistanceFileLocationOf($"{payloadId}");
            Payload payload;

            if (File.Exists(filePath))
            {
                payload = new Payload(File.OpenRead(filePath), LoadPayloadMeta(payloadId));
            }
            else
            {
                payload = Payload.Null;
            }

            return Task.FromResult(payload);
        }

        private PayloadMeta LoadPayloadMeta(string payloadId)
        {
            string filedownloadName = $"{payloadId}.download";
            string metaFilePath = GetPersistanceFileLocationOf($"{payloadId}.meta");

            if (File.Exists(metaFilePath))
            {
                return ParseMetadataFile(metaFilePath);
            }

            return new PayloadMeta(filedownloadName);
        }

        private string GetPersistanceFileLocationOf(string payloadId)
        {
            return Path.Combine(_persistenceLocation, payloadId);
        }

        private PayloadMeta ParseMetadataFile(string metaFile)
        {
            string[] lines = File.ReadAllLines(metaFile);
            string originalFilename = string.Empty;

            foreach (string line in lines)
            {
                Func<string, bool> lineContainsKey = 
                    key => line.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) > -1;

                if (lineContainsKey(OriginalFileNameKey))
                {
                    originalFilename = line.Substring(OriginalFileNameKey.Length);
                }
            }

            return new PayloadMeta(originalFilename);
        }
    }
}