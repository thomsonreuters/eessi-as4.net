using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayloadService.Connector
{
    public class PayloadConnector
    {
        private readonly string _url;

        // Share the same HttpClient instance: https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadConnector"/> class.
        /// </summary>
        private PayloadConnector(string url)
        {
            _url = url;
            _client = new HttpClient();
        }

        public static PayloadConnector Connect(string url)
        {
            return new PayloadConnector(url);
        }

        public async Task<UploadResult> UploadFile(string filePath)
        {
            var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file), file.Name, file.Name);

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri(_url + "/api/Payload/Upload")
            };


            var result = await _client.SendAsync(message);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                // The ReadAsAsync method comes with the Microsoft.AspNet.WebApi.Client package.
                PayloadServiceResult response = await result.Content.ReadAsAsync<PayloadServiceResult>();
                return new UploadResult(true, response.PayloadId);
            }

            return new UploadResult(false, "");
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class PayloadServiceResult
        {
            public string PayloadId { get; set; }
        }

        public async Task<DownloadResult> DownloadFile(string payloadId)
        {
            var message = new HttpRequestMessage();

            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(_url + $"/api/Payload/{payloadId}");

            var result = await _client.SendAsync(message);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var str = await result.Content.ReadAsStreamAsync();

                return DownloadResult.CreateSuccess(EnsureValidFilename(result.Content.Headers.ContentDisposition.FileName), str);
            }

            return DownloadResult.CreateFailed("");
        }

        private static string EnsureValidFilename(string filename)
        {
            return String.Join("", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
