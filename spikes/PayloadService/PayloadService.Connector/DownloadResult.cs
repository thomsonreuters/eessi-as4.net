using System.IO;

namespace PayloadService.Connector
{
    public class DownloadResult
    {
        public bool Success { get; private set; }

        public string ErrorMessage { get; private set; }
        public string OriginalFilename { get; private set; }
        public Stream Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadResult"/> class.
        /// </summary>
        private DownloadResult(bool success, string errorMessage, string originalFilename, Stream content)
        {
            this.Success = success;
            this.ErrorMessage = errorMessage;
            this.OriginalFilename = originalFilename;
            this.Content = content;
        }

        public static DownloadResult CreateSuccess(string originalFilename, Stream content)
        {
            return new DownloadResult(true, string.Empty, originalFilename, content);
        }

        public static DownloadResult CreateFailed(string errorMessage)
        {
            return new DownloadResult(false, errorMessage, string.Empty, Stream.Null);
        }
    }
}