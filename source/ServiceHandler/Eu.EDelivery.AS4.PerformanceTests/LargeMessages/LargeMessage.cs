using System.IO;

namespace Eu.EDelivery.AS4.PerformanceTests.LargeMessages
{
    public class LargeMessage
    {
        /// <summary>
        /// Create a large file for a given size.
        /// </summary>
        /// <param name="filePath">The file Path.</param>
        public static void CreateFile(string filePath)
        {
            using (FileStream fileStream = File.Create(filePath))
            {
                fileStream.Seek(2048L * 1024 * 1024, SeekOrigin.Begin);
                fileStream.WriteByte(0);
            }
        }
    }
}
