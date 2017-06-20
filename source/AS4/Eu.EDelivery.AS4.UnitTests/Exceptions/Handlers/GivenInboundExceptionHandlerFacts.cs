using System;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class GivenInboundExceptionHandlerFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task InsertInException_IfTransformException()
        {
            // Arrange
            string expectedBody = Guid.NewGuid().ToString(), 
                expectedMessage = Guid.NewGuid().ToString();

            var sut = new InboundExceptionHanlder(GetDataStoreContext);

            // Act
            await sut.ExerciseTransformException(GetDataStoreContext, expectedBody, new Exception(expectedMessage));

            // Assert
            GetDataStoreContext.AssertInException(
                ex =>
                {
                    Assert.Equal(expectedBody, Encoding.UTF8.GetString(ex.MessageBody));
                    Assert.Equal(expectedMessage, ex.Exception);
                });
        }

        [Theory]
        [InlineData(true, Operation.ToBeNotified)]
        [InlineData(false, default(Operation))]
        public async Task InsertInException_IfExecutionException(bool notifyConsumer, Operation expected)
        {
            // Act
            string id = Guid.NewGuid().ToString();
            MessagingContext context = ContextWithAS4UserMessage(id, notifyConsumer);

            var sut = new InboundExceptionHanlder(GetDataStoreContext);

            // Act
            await sut.HandleExecutionException(new Exception(), context);

            // Assert
            GetDataStoreContext.AssertInException(
                id,
                ex =>
                {
                    Assert.Equal(expected, ex.Operation);
                    Assert.Null(ex.MessageBody);
                });
        }

        private static MessagingContext ContextWithAS4UserMessage(string id, bool notifyConsumer)
        {
            return new MessagingContext(AS4Message.Create(new FilledUserMessage(id)), MessagingContextMode.Receive)
            {
                ReceivingPMode = new ReceivingProcessingMode {ExceptionHandling = {NotifyMessageConsumer = notifyConsumer}}
            };
        }
    }
}
