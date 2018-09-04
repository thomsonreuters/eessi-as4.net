using System;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Newtonsoft.Json;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// <see cref="IAttachmentUploader" /> implementation to upload <see cref="Attachment" /> models as Multipart Form data.
    /// </summary>
    [Info(PayloadServiceAttachmentUploader.Key)]
    public class PayloadServiceAttachmentUploader : IAttachmentUploader
    {
        public const string Key = "PAYLOAD-SERVICE";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly Func<string, HttpContent, Task<HttpResponseMessage>> _postRequest;
        [Info("location")]
        private string Location { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadServiceAttachmentUploader"/> class.
        /// </summary>
        public PayloadServiceAttachmentUploader() : this(HttpClient.PostAsync) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadServiceAttachmentUploader" /> class.
        /// </summary>
        /// <param name="postRequest"></param>
        public PayloadServiceAttachmentUploader(Func<string, HttpContent, Task<HttpResponseMessage>> postRequest)
        {
            if (postRequest == null)
            {
                throw new ArgumentNullException(nameof(postRequest));
            }

            _postRequest = postRequest;
        }

        /// <summary>
        /// Configure the <see cref="IAttachmentUploader" />
        /// with a given <paramref name="payloadReferenceMethod" />
        /// </summary>
        /// <param name="payloadReferenceMethod"></param>
        public void Configure(Method payloadReferenceMethod)
        {
            if (payloadReferenceMethod == null)
            {
                throw new ArgumentNullException(nameof(payloadReferenceMethod));
            }

            string location = payloadReferenceMethod["location"]?.Value;
            if (String.IsNullOrWhiteSpace(location))
            {
                throw new InvalidOperationException(
                    $"{nameof(PayloadServiceAttachmentUploader)} requires a location to upload the attachments to, please add a "
                    + "<Parameter key=\"location\" value=\"your-payload-service-endpoint\"/> to the MessageHandling.Deliver.PayloadReferenceMethod in the ReceivingPMode");
            }

            Location = location;
        }

        /// <inheritdoc />
        public async Task<UploadResult> UploadAsync(Attachment attachment, UserMessage referringUserMessage)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (referringUserMessage == null)
            {
                throw new ArgumentNullException(nameof(referringUserMessage));
            }

            HttpResponseMessage response = await PostAttachmentAsMultipart(attachment).ConfigureAwait(false);
            return await DeserializeResponseAsUploadResult(response).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> PostAttachmentAsMultipart(Attachment attachment)
        {
            var form = new MultipartFormDataContent { { new StreamContent(attachment.Content), attachment.Id, attachment.Id } };

            HttpResponseMessage response = await _postRequest(Location, form).ConfigureAwait(false);
            Logger.Info($"(Deliver) Upload attachment returns HTTP StatusCode: {response.StatusCode}");

            return response;
        }

        private static async Task<UploadResult> DeserializeResponseAsUploadResult(HttpResponseMessage response)
        {
            string serializedContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<UploadResult>(serializedContent);
        }
    }
}