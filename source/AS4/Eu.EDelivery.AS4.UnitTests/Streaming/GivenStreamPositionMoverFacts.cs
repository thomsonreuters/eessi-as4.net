using System;
using System.IO;
using System.Text;
using Eu.EDelivery.AS4.Streaming;
using MimeKit.IO;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Streaming
{
    /// <summary>
    /// Testing <see cref="StreamPositionMover"/>
    /// </summary>
    public class GivenStreamPositionMoverFacts
    {
        [Fact]
        public void FailsToMovePositionOnNullStream()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                () => StreamPositionMover.MovePositionToStreamStart(stream: null));
        }

        [Fact]
        public void ThenStreamGetsSetToZero_IfStreamIsSeekable()
        {
            // Arrange
            using (MemoryStream stubStream = GetNotZeroPositionStream())
            {
                // Act
                StreamPositionMover.MovePositionToStreamStart(stubStream);

                // Assert
                AssertEqualsZero(stubStream);
            }
        }

        [Fact]
        public void ThenStreamGetsSetToZero_IfStreamIsNonClosableStream()
        {
            // Arrange
            using (var stubStream = new NonCloseableStream(GetNotZeroPositionStream()))
            {
                // Act
                StreamPositionMover.MovePositionToStreamStart(stubStream);

                // Assert
                AssertEqualsZero(stubStream.InnerStream);
            }
        }

        [Fact]
        public void ThenStreamGetsSetToZero_IfStreamIsFilteredStream()
        {
            // Arrange
            using (var stubStream = new FilteredStream(GetNotZeroPositionStream()))
            {
                // Act
                StreamPositionMover.MovePositionToStreamStart(stubStream);

                // Assert
                AssertEqualsZero(stubStream.Source);
            }
        }

        private static void AssertEqualsZero(Stream stream) => Assert.Equal(0, stream.Position);

        [Fact]
        public void TestNonZeroPositionStreamFixture()
        {
            // Act
            using (Stream actualStream = GetNotZeroPositionStream())
            {
                // Assert
                Assert.NotEqual(0, actualStream.Position);
            }
        }

        private static MemoryStream GetNotZeroPositionStream()
        {
            var stubStream = new MemoryStream(Encoding.UTF8.GetBytes("ignored string"));
            stubStream.ReadByte();

            return stubStream;
        }
    }
}
