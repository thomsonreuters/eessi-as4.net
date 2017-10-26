using System.IO;

namespace Eu.EDelivery.AS4.Utilities
{
    public static class FileUtils
    {
        private const int DefaultBufferSize = 4096;

        public static FileStream OpenAsync(string fileName, FileMode mode, FileAccess access)
        {
            return new FileStream(fileName, mode, access, FileShare.Read, DefaultBufferSize, useAsync: true);
        }

        public static FileStream OpenReadAsync(string fileName)
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, useAsync: true);
        }

        public static FileStream CreateAsync(string fileName)
        {
            return new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, DefaultBufferSize, useAsync: true);
        }
    }

}
