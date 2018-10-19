using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class GivenSafeExceptionHandlerFacts : IAgentExceptionHandler
    {
        [Fact]
        public async Task CatchesTransformHandling()
        {
            await TestSafeHandler(
                saboteur => saboteur.HandleTransformationException(null, null),
                sut => sut.HandleTransformationException(null, null));
        }

        public Task<MessagingContext> HandleTransformationException(Exception exception, ReceivedMessage messageToTransform)
        {
            throw new SaboteurException("Sabotage handling transformation exception");
        }

        [Fact]
        public async Task CatchesExecutionHandling()
        {
            await TestSafeHandler(
               saboteur => saboteur.HandleExecutionException(null, null),
               sut => sut.HandleExecutionException(null, null));
        }

        public Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            throw new SaboteurException("Sabotage handling execution exception");
        }

        [Fact]
        public async Task CatchesErrorHandling()
        {
            await TestSafeHandler(
                saboteur => saboteur.HandleErrorException(null, null), 
                sut => sut.HandleErrorException(null, null));
        }

        public Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            throw new SaboteurException("Sabotage handling error exception");
        }

        private async Task TestSafeHandler(
            Expression<Action<IAgentExceptionHandler>> expression,
            Func<IAgentExceptionHandler, Task<MessagingContext>> exercise)
        {
            var sut = new SafeExceptionHandler(this);

            // Act
            MessagingContext context = await exercise(sut);

            // Assert
            Assert.NotNull(context);
        }
    }
}
