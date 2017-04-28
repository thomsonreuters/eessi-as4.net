using System.IO;
using Xunit;

namespace Eu.EDelivery.AS4.PerformanceTests.LargeMessages
{
    public class LargeMessage
    {
        /// <summary>
        /// Create a large file for a given size.
        /// </summary>
        /// <param name="filePath">The file Path.</param>
        /// <param name="value"></param>
        /// <param name="metric">The metric.</param>
        public static void CreateFile(string filePath, int value, Size metric)
        {
            using (FileStream fileStream = File.Create(filePath))
            {
                fileStream.Seek(value * (long) metric, SeekOrigin.Begin);
                fileStream.WriteByte(0);
            }
        }

        public enum Size : long
        {
            GB = 1024 * 1024 * 1024,
            MB = 1024 * 1024
        }

        [Fact]
        public void CreateExpectedFile()
        {
            // Arrange
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "message.txt");

            // Act
            CreateFile(filePath, value: 1, metric: Size.MB);

            // Assert
            int actualSize = File.ReadAllBytes(filePath).Length;
            Assert.Equal((long) Size.MB, actualSize);
        }
    }
}
