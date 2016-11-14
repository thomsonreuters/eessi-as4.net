using System.Web;
using Microsoft.Win32;

namespace Eu.EDelivery.AS4.Repositories
{
    /// <summary>
    /// Repository with Mime Type specific operations
    /// </summary>
    public class MimeTypeRepository : IMimeTypeRepository
    {
        private const string RegistryPath = @"MIME\Database\Content Type\";

        /// <summary>
        /// Retrieve the right Extension
        /// from a given MIME Content Type
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public string GetExtensionFromMimeType(string mimeType)
        {
            string emptyExtension = string.Empty;
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(RegistryPath + mimeType, writable: false);
            if (key == null) return emptyExtension;

            object value = key.GetValue("Extension", defaultValue: string.Empty);
            return value.ToString();
        }

        /// <summary>
        /// Retrieve the right MimeType
        /// from a given Extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string GetMimeTypeFromExtension(string extension)
        {
            return MimeMapping.GetMimeMapping(extension);
        }
    }

    public interface IMimeTypeRepository
    {
        /// <summary>
        /// Retrieve the right Extension
        /// from a given MIME Content Type
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        string GetExtensionFromMimeType(string mimeType);

        /// <summary>
        /// Retrieve the right MimeType
        /// from a given Extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        string GetMimeTypeFromExtension(string extension);
    }
}