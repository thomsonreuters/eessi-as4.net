using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Repositories
{
    /// <summary>
    /// Testing <see cref="MimeTypeRepository" />
    /// </summary>
    public class GivenMimeTypeRepositoryFacts
    {
        private readonly MimeTypeRepository _repository;

        public GivenMimeTypeRepositoryFacts()
        {
            _repository = new MimeTypeRepository();
        }

        public class GivenValidArguments : GivenMimeTypeRepositoryFacts
        {
            [Fact]
            public void ThenGetsExtensionSucceedsWithValidMimeContentType()
            {
                // Arrange
                const string mimeContentType = "image/jpeg";

                // Act
                string extenstion = _repository.GetExtensionFromMimeType(mimeContentType);

                // Assert
                Assert.Equal(".jpg", extenstion);
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
                string extension = _repository.GetExtensionFromMimeType(mimeContentType);

                // Assert
                Assert.Empty(extension);
            }
        }
    }
}