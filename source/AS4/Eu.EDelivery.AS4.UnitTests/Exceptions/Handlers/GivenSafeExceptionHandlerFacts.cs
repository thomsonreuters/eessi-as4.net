using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Internal;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class GivenSafeExceptionHandlerFacts
    {
        [Fact]
        public async Task CatchesTransformHandling()
        {
            // Arrange
            var saboteur = new Mock<IAgentExceptionHandler>();
            saboteur.Setup(h => h.HandleTransformationException(null, null)).Throws<Exception>();

            var sut = new SafeExceptionHandler(saboteur.Object);

            // Act
            MessagingContext context = await sut.HandleTransformationException(null, null);

            // Assert
            Assert.NotNull(context);
            saboteur.Verify(h => h.HandleTransformationException(null, null), Times.Once);
        }

        [Fact]
        public async Task CatchesExecutionHandling()
        {
            // Arrange
            var saboteur = new Mock<IAgentExceptionHandler>();
            saboteur.Setup(h => h.HandleExecutionException(null, null)).Throws<Exception>();

            var sut = new SafeExceptionHandler(saboteur.Object);

            // Act
            MessagingContext context = await sut.HandleExecutionException(null, null);

            // Assert
            Assert.NotNull(context);
            saboteur.Verify(h => h.HandleExecutionException(null, null), Times.Once);
        }

        [Fact]
        public async Task CatchesErrorHandling()
        {
            // Arrange
            var saboteur = new Mock<IAgentExceptionHandler>();
            saboteur.Setup(h => h.HandleErrorException(null, null)).Throws<Exception>();

            var sut = new SafeExceptionHandler(saboteur.Object);

            // Act
            MessagingContext context = await sut.HandleErrorException(null, null);

            // Assert
            Assert.NotNull(context);
            saboteur.Verify(h => h.HandleErrorException(null, null), Times.Once);
        }
    }
}
