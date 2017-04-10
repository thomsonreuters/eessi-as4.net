using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using Eu.EDelivery.AS4.UnitTests.Common;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Services
{
    /// <summary>
    /// Testing <see cref="InExceptionService" />
    /// </summary>
    public class GivenInExceptionServiceFacts : GivenDatastoreFacts
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
            public void ThenInsertAS4ExceptionSucceedsAsync(string sharedId)
            {
                // Arrange
                _mockedRepository.Setup(r => r.InsertInException(It.IsAny<InException>()))
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
                _service.InsertAS4Exception(as4Exception, new AS4Message());

                // Assert
                _mockedRepository.Verify(r => r.InsertInException(It.IsAny<InException>()), Times.Once);
            }
        }
    }
}