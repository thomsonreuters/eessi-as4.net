using System.IO;

namespace Eu.EDelivery.AS4.Utilities
{
    public static class FileUtils
    {
        private const int DefaultBufferSize = 4096;

        public static FileStream OpenAsync(string fileName, FileMode mode, FileAccess access, FileOptions options = FileOptions.None)
        {
            options |= FileOptions.Asynchronous;

            return new FileStream(fileName, mode, access, FileShare.Read, DefaultBufferSize, options);
        }

        public static FileStream OpenReadAsync(string fileName, FileOptions options = FileOptions.None)
        {
            options |= FileOptions.Asynchronous;

            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, options);
        }

        public static FileStream CreateAsync(string fileName, FileOptions options = FileOptions.None)
        {
            options |= FileOptions.Asynchronous;

            return new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, DefaultBufferSize, options);
        }
    }

}
