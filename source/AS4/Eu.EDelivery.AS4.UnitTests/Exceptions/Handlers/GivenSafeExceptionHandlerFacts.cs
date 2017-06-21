using System;
using System.Linq.Expressions;
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
            await TestSafeHandler(
                saboteur => saboteur.HandleTransformationException(null, null),
                sut => sut.HandleTransformationException(null, null));
        }

        [Fact]
        public async Task CatchesExecutionHandling()
        {
            await TestSafeHandler(
               saboteur => saboteur.HandleExecutionException(null, null),
               sut => sut.HandleExecutionException(null, null));
        }

        [Fact]
        public async Task CatchesErrorHandling()
        {
            await TestSafeHandler(
                saboteur => saboteur.HandleErrorException(null, null), 
                sut => sut.HandleErrorException(null, null));
        }

        private static async Task TestSafeHandler(
            Expression<Action<IAgentExceptionHandler>> expression,
            Func<IAgentExceptionHandler, Task<MessagingContext>> exercise)
        {
            var saboteur = new Mock<IAgentExceptionHandler>();
            saboteur.Setup(expression).Throws<Exception>();

            var sut = new SafeExceptionHandler(saboteur.Object);

            // Act
            MessagingContext context = await exercise(sut);

            // Assert
            Assert.NotNull(context);
            saboteur.Verify(expression, Times.Once);
        }
    }
}
