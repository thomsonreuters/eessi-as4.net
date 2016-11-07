namespace Eu.EDelivery.AS4.Utilities
{
    /// <summary>
    /// Wraps the available ContentTypes for the Application
    /// </summary>
    public static class ContentTypeSupporter
    {
        public static bool IsContentTypeSupported(string contentType)
        {
            return IsMimeContentType(contentType) || IsSoapContentType(contentType);
        }

        public static bool IsMimeContentType(string contentType)
        {
            return contentType.StartsWith(Constants.ContentTypes.Mime);
        }

        public static bool IsSoapContentType(string contentType)
        {
            return contentType.StartsWith(Constants.ContentTypes.Soap);
        }
    }
}