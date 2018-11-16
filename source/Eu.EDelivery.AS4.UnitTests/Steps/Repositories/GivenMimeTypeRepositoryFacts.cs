using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Repositories
{
    /// <summary>
    /// Testing <see cref="MimeTypeRepository" />
    /// </summary>
    public class GivenMimeTypeRepositoryFacts
    {
        public class GivenValidArguments : GivenMimeTypeRepositoryFacts
        {
            [Fact]
            public void ThenGetsExtensionSucceedsWithValidMimeContentType()
            {
                // Arrange
                const string mimeContentType = "image/jpeg";

                // Act
                string extension = MimeTypeRepository.Instance.GetExtensionFromMimeType(mimeContentType);

                // Assert
                Assert.Equal(".jpg", extension);
            }
        }

        public class GivenInvalidArguments : GivenMimeTypeRepositoryFacts
        {
            [Fact]
            public void ThenGetsExtensionFailsWithInvalidMimeContentType()
            {
                // Arrange
                const string mimeContentType = "invalid/type";

                // Act
                string extension = MimeTypeRepository.Instance.GetExtensionFromMimeType(mimeContentType);

                // Assert
                Assert.Empty(extension);
            }
        }
    }
}