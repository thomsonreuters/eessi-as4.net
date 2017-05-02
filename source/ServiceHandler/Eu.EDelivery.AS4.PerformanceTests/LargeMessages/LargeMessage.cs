using System.IO;
using Xunit;

namespace Eu.EDelivery.AS4.PerformanceTests.LargeMessages
{
    public class LargeMessage
    {
        /// <summary>
        /// Create a large file for a given size.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="value"></param>
        /// <param name="metric">The metric.</param>
        public static string CreateFile(DirectoryInfo directory, int value, Size metric)
        {
            string filePath = Path.Combine(directory.FullName, "image.jpg");
            using (FileStream fileStream = File.Create(filePath))
            {
                fileStream.Seek(value * (long) metric, SeekOrigin.Begin);
                fileStream.WriteByte(0);
            }

            return $"file:///{filePath}";
        }

        [Fact]
        public void CreateExpectedFile()
        {
            // Arrange
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            // Act
            string filePath = CreateFile(currentDirectory, value: 1, metric: Size.MB);

            // Assert
            int expectedSize = Floor((int)Size.MB);
            int actualSize = Floor(ReadLargeFileLength(filePath));
            Assert.Equal(expectedSize, actualSize);
        }

        private static int Floor(int value)
        {
            return value - (value % 10);
        }

        private static int ReadLargeFileLength(string filePath)
        {
            return File.ReadAllBytes(filePath.Replace("file:///", string.Empty)).Length;
        }
    }

    public enum Size : long
    {
        GB = 1024 * 1024 * 1024,
        MB = 1024 * 1024
    }
}
