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
        /// Start uploading <see cref="Attachment"/>
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        Task<UploadResult> UploadAsync(Attachment attachment);
    }
}
