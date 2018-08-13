using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// Interface to upload Payloads to a given Media
    /// </summary>
    public interface IAttachmentUploader
    {
        /// <summary>
        /// Configure the <see cref="IAttachmentUploader"/>
        /// with a given <paramref name="payloadReferenceMethod"/>
        /// </summary>
        /// <param name="payloadReferenceMethod"></param>
        void Configure(Method payloadReferenceMethod);

        /// <summary>
        /// Start uploading the <paramref name="attachment"/>
        /// </summary>
        /// <remarks>The <paramref name="referringUserMessage"/> parameter can be used
        /// by the IAttachmentUploader implementation when determining the name that must be
        /// given to the uploaded payload.</remarks>
        /// <param name="attachment">The <see cref="Attachment"/> that must be uploaded</param>
        /// <param name="referringUserMessage">The UserMessage to which the Attachment belongs to.</param>
        /// <returns>An UploadResult instance</returns>
        Task<UploadResult> UploadAsync(Attachment attachment, UserMessage referringUserMessage);
    }
}
