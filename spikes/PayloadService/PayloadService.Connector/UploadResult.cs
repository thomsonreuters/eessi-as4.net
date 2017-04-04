namespace PayloadService.Connector
{
    public class UploadResult
    {
        public bool Success { get; }
        public string Location { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadResult"/> class.
        /// </summary>
        public UploadResult(bool success, string location)
        {
            Success = success;
            Location = location;
        }
    }
}