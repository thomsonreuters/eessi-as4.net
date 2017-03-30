using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Services
{
    /// <summary>
    /// Testing <see cref="InExceptionService" />
    /// </summary>
    public class GivenInExceptionServiceFacts
    {
        private InExceptionService _service;
        private Mock<IDatastoreRepository> _mockedRepository;

        public GivenInExceptionServiceFacts()
        {
            ResetDatastoreService();
        }

        protected void ResetDatastoreService()
        {
            _mockedRepository = new Mock<IDatastoreRepository>();
            _service = new InExceptionService(_mockedRepository.Object);
        }

        public class GivenValidArguments : GivenInExceptionServiceFacts
        {
            [Theory]
            [InlineData("shared-id")]
            public async Task ThenInsertAS4ExceptionSucceedsAsync(string sharedId)
            {
                // Arrange
                _mockedRepository.Setup(r => r.InsertInExceptionAsync(It.IsAny<InException>()))
                                 .Callback(
                                     (InException intException) =>
                                     {
                                         // Assert
                                         Assert.Equal(sharedId, intException.EbmsRefToMessageId);
                                     });
                ResetDatastoreService();

                AS4Exception as4Exception =
                    AS4ExceptionBuilder.WithDescription("Test Exception").WithMessageIds(sharedId).Build();

                // Act
                await _service.InsertAS4ExceptionAsync(as4Exception, new AS4Message());

                // Assert
                _mockedRepository.Verify(r => r.InsertInExceptionAsync(It.IsAny<InException>()), Times.Once);
            }
        }
    }
}