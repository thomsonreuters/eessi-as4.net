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
    /// Testing <see cref="InExceptionService"/>
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
            this._mockedRepository = new Mock<IDatastoreRepository>();
            this._service = new InExceptionService(this._mockedRepository.Object);
        }

        public class GivenValidArguments : GivenInExceptionServiceFacts
        {
            [Theory, InlineData("shared-id")]
            public async Task ThenInsertAS4ExceptionSucceedsAsync(string sharedId)
            {
                // Arrange
                base._mockedRepository
                    .Setup(r => r.InsertInExceptionAsync(It.IsAny<InException>()))
                    .Callback((InException intException) =>
                    {
                        // Assert
                        Assert.Equal(sharedId, intException.EbmsRefToMessageId);
                    });
                base.ResetDatastoreService();

                AS4Exception as4Exception = AS4ExceptionBuilder
                    .WithDescription("Test Exception").WithMessageIds(sharedId).Build();

                // Act
                await base._service.InsertAS4ExceptionAsync(exception: as4Exception, as4Message: new AS4Message());
                // Assert
                base._mockedRepository.Verify(r
                    => r.InsertInExceptionAsync(It.IsAny<InException>()), Times.Once);
            }
        }
    }
}
