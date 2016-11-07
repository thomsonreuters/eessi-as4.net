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
        /// Retrieve the right File Extension
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
    }

    public interface IMimeTypeRepository
    {
        string GetExtensionFromMimeType(string mimeType);
    }
}