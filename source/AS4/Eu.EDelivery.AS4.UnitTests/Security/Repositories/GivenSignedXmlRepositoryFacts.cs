using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Security.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security.Repositories
{
    /// <summary>
    /// Testing <see cref="SignedXmlRepository"/>
    /// </summary>
    public class GivenSignedXmlRepositoryFacts
    {
        protected XmlDocument Document;

        public GivenSignedXmlRepositoryFacts()
        {
            this.Document = new XmlDocument();
            this.Document.LoadXml(Properties.Resources.as4_soap_signed_message);
        }

        public class GivenValidArguments : GivenSignedXmlRepositoryFacts
        {
            [Fact]
            public void ThenRespositoryGetsSignatureElement()
            {
                // Arrange
                var repository = new SignedXmlRepository(base.Document);
                // Act
                XmlElement signatureElement = repository.GetSignatureElement();
                // Assert
                Assert.NotNull(signatureElement);
            }

            [Theory]
            [InlineData("header-c8783d04-c792-46ac-a43e-17cbf160a778")]
            [InlineData("body-c9c493a9-30db-4730-959b-ca6a30d506d3")]
            public void ThenRepositoryGetsReferenceIdElements(string id)
            {
                // Arrange
                var repository = new SignedXmlRepository(base.Document);
                // Act
                XmlElement idElement = repository.GetReferenceIdElement(id);
                // Assert
                Assert.NotNull(idElement);
            }
        }

        public class GivenInvalidArguments : GivenSignedXmlRepositoryFacts
        {
            [Fact]
            public void ThenRespositoryFailsWhenGettingSignatureElement()
            {
                // Arrange
                var document = new XmlDocument();
                var repository = new SignedXmlRepository(document);
                // Act
                var as4Exception = Assert.Throws<AS4Exception>(
                    () => repository.GetSignatureElement());
                // Assert
                Assert.Equal(ErrorCode.Ebms0101, as4Exception.ErrorCode);
            }

            [Fact]
            public void ThenRepositoryFailsWhenGettingReferenceIdElements()
            {
                // Arrange
                var document = new XmlDocument();
                var repository = new SignedXmlRepository(document);
                const string notExistingId = "not-existing-id";
                // Act
                XmlElement idElement = repository.GetReferenceIdElement(notExistingId);
                // Assert
                Assert.Null(idElement);
            }
        }
    }
}
