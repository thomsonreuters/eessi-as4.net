using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.UnitTests.Common;
using Moq;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Extensions.AS4MessageExtensions;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Services
{
    /// <summary>
    /// Testing <see cref="InExceptionService" />
    /// </summary>
    public class GivenInExceptionServiceFacts : GivenDatastoreFacts
    {
        [Fact]
        public void ThenInsertAS4ExceptionSucceedsAsync()
        {
            // Arrange
            const string sharedId = "message-id";
            var mockedRepository = new Mock<IDatastoreRepository>();
            mockedRepository.Setup(r => r.InsertInException(It.IsAny<InException>()))
                            .Callback((InException exception) => Assert.Equal(sharedId, exception.EbmsRefToMessageId));

            AS4Exception as4Exception = CreateExceptionWithMessageId(sharedId);

            var service = new InExceptionService(mockedRepository.Object);

            // Act
            service.InsertAS4Exception(as4Exception, AS4Message.Empty);

            // Assert
            mockedRepository.Verify(r => r.InsertInException(It.IsAny<InException>()), Times.Once);
        }

        private static AS4Exception CreateExceptionWithMessageId(string sharedId)
        {
            return AS4ExceptionBuilder.WithDescription("Test Exception").WithMessageIds(sharedId).Build();
        }
    }
}