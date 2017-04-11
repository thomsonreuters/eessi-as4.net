using System;
using Eu.EDelivery.AS4.Strategies.Uploader;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Uploader
{
    /// <summary>
    /// <see cref="IAttachmentUploader"/> implementation to always return the same configured <see cref="IAttachmentUploader"/> implementation.
    /// </summary>
    internal class StubAttachmentUploaderProvider : IAttachmentUploaderProvider
    {
        private readonly IAttachmentUploader _configedUploader;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubAttachmentUploaderProvider"/> class.
        /// </summary>
        /// <param name="uploader"></param>
        public StubAttachmentUploaderProvider(IAttachmentUploader uploader)
        {
            _configedUploader = uploader;
        }

        /// <summary>
        /// Accept a <see cref="IAttachmentUploader"/> implementation in the <see cref="IAttachmentUploaderProvider"/>.
        /// </summary>
        /// <param name="condition">Condition for which the <see cref="IAttachmentUploader"/> must be used.</param>
        /// <param name="uploader"><see cref="IAttachmentUploader"/> implementation to be used.</param>
        public void Accept(Func<string, bool> condition, IAttachmentUploader uploader) {}

        /// <summary>
        /// Get a <see cref="IAttachmentUploader"/> implementation based on a given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type for which the <see cref="IAttachmentUploader"/> implementation is accepted.</param>
        /// <returns></returns>
        public IAttachmentUploader Get(string type)
        {
            return _configedUploader;
        }
    }
}