using System;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Newtonsoft.Json;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// <see cref="IAttachmentUploader" /> implementation to upload <see cref="Attachment" /> models as Multipart Form data.
    /// </summary>
    public class PayloadServiceAttachmentUploader : IAttachmentUploader
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly Func<string, HttpContent, Task<HttpResponseMessage>> _postRequest;
        private string _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadServiceAttachmentUploader"/> class.
        /// </summary>
        public PayloadServiceAttachmentUploader() : this(HttpClient.PostAsync) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadServiceAttachmentUploader" /> class.
        /// </summary>
        /// <param name="postRequest"></param>
        public PayloadServiceAttachmentUploader(Func<string, HttpContent, Task<HttpResponseMessage>> postRequest)
        {
            _postRequest = postRequest;
        }

        /// <summary>
        /// Configure the <see cref="IAttachmentUploader" />
        /// with a given <paramref name="payloadReferenceMethod" />
        /// </summary>
        /// <param name="payloadReferenceMethod"></param>
        public void Configure(Method payloadReferenceMethod)
        {
            _location = payloadReferenceMethod["location"].Value;
        }

        /// <summary>
        /// Start uploading <see cref="Attachment" />
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public async Task<UploadResult> UploadAsync(Attachment attachment)
        {
            try
            {
                HttpResponseMessage response = await PostAttachmentAsMultipart(attachment);
                return await DeserializeResponseAsUploadResult(response);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                throw new AS4Exception(exception.Message);
            }
        }

        private async Task<HttpResponseMessage> PostAttachmentAsMultipart(Attachment attachment)
        {
            var form = new MultipartFormDataContent {{new StreamContent(attachment.Content), attachment.Id, attachment.Id}};

            HttpResponseMessage response = await _postRequest(_location, form);
            Logger.Info($"Upload Attachment returns HTTP Status Code: {response.StatusCode}");

            return response;
        }

        private static async Task<UploadResult> DeserializeResponseAsUploadResult(HttpResponseMessage response)
        {
            string serializedContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UploadResult>(serializedContent);
        }
    }
}