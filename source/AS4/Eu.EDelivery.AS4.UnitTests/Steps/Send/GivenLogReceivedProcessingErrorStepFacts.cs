using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    public class GivenLogReceivedProcessingErrorStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task InExceptionGetsInserted_IfErrorResultAndAS4MessageArePresent()
        {
            // Arrange
            string id = Guid.NewGuid().ToString(), 
                expected = Guid.NewGuid().ToString();

            AS4Message as4Message = AS4Message.Create(new Error {RefToMessageId = id});
            var error = new ErrorResult(expected, default(ErrorCode), default(ErrorAlias));

            // Act
            await ExerciseLog(as4Message, error);

            // Assert
            GetDataStoreContext.AssertInException(id, ex => Assert.Equal(expected, ex.Exception));
        }

        [Fact]
        public async Task NoExceptionGetsLogged_IfNoErrorResultIsPresent()
        {
            // Arrange
            string id = Guid.NewGuid().ToString();
            AS4Message as4Message = AS4Message.Create(new Error {RefToMessageId = id});

            // Act
            await ExerciseLog(as4Message, error: null);

            // Assert
            GetDataStoreContext.AssertInException(id, Assert.Null);
        }
        
        private async Task ExerciseLog(AS4Message as4Message, ErrorResult error)
        {
            var sut = new LogReceivedProcessingErrorStep(GetDataStoreContext);

            await sut.ExecuteAsync(
                new MessagingContext(as4Message, MessagingContextMode.Send) {ErrorResult = error},
                default(CancellationToken));
        }
    }
}
