using System;
using System.Linq;

namespace AS4.ParserService.Infrastructure
{
    internal class MimeTypeMapper
    {
        public static string GetExtensionFor(string mimeType)
        {
            if (String.IsNullOrWhiteSpace(mimeType))
            {
                return string.Empty;
            }

            if (mimeType.Equals("text/xml", StringComparison.OrdinalIgnoreCase))
            {
                return ".xml";
            }

            var extension = new MimeSharp.Mime().Extension(mimeType).FirstOrDefault();

            return String.IsNullOrWhiteSpace(extension) ? string.Empty : extension;
        }
    }
}