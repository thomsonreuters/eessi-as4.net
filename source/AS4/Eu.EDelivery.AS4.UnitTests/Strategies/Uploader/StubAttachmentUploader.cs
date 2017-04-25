using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Strategies.Uploader;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Uploader
{
    /// <summary>
    /// <see cref="IAttachmentUploader"/> implementation to return always a configured <see cref="UploadResult"/>.
    /// </summary>
    internal class StubAttachmentUploader : IAttachmentUploader
    {
        private readonly UploadResult _configuredResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubAttachmentUploader" /> class.
        /// </summary>
        /// <param name="downloadUrl"></param>
        public StubAttachmentUploader(string downloadUrl)
        {
            _configuredResult = new UploadResult {DownloadUrl = downloadUrl};
        }

        /// <summary>
        /// Configure the <see cref="IAttachmentUploader" />
        /// with a given <paramref name="payloadReferenceMethod" />
        /// </summary>
        /// <param name="payloadReferenceMethod"></param>
        public void Configure(AS4.Model.PMode.Method payloadReferenceMethod) {}

        /// <summary>
        /// Start uploading <see cref="Attachment" />
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public Task<UploadResult> UploadAsync(Attachment attachment)
        {
            return Task.FromResult(_configuredResult);
        }
    }
}