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
    public class GivenOutboundExceptionHandlerFacts : GivenDatastoreFacts
    {
        private readonly string _expectedId = Guid.NewGuid().ToString();
        private readonly string _expectedBody = Guid.NewGuid().ToString();
        private readonly Exception _expectedException = new Exception(Guid.NewGuid().ToString());

        /// <summary>
        /// Inserts the out exception if transform exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertOutException_IfTransformException()
        {
            // Arrange
            var sut = new OutboundExceptionHandler(GetDataStoreContext);

            // Act
            MessagingContext context = 
                await sut.ExerciseTransformException(GetDataStoreContext, _expectedBody, _expectedException);

            // Assert
            Assert.Same(_expectedException, context.Exception);
            GetDataStoreContext.AssertOutException(
                ex =>
                {
                    Assert.True(_expectedException.Message == ex.Exception, "Not equal message insterted");
                    Assert.True(_expectedBody == Encoding.UTF8.GetString(ex.MessageBody), "Not equal body inserted");
                });
        }

        [Theory]
        [InlineData(true, Operation.ToBeNotified)]
        [InlineData(false, default(Operation))]
        public async Task InsertOutException_IfStepExecutionException(bool notifyProducer, Operation expected)
        {
            // Arrange
            MessagingContext context = ContextWithAS4UserMessage(notifyProducer, _expectedId);
            var sut = new OutboundExceptionHandler(GetDataStoreContext);

            // Act
            await sut.HandleExecutionException(_expectedException, context);

            // Assert
            GetDataStoreContext.AssertOutException(
                _expectedId,
                exception =>
                {
                    Assert.True(_expectedException.Message == exception.Exception, "Not equal message inserted");
                    Assert.True(expected == exception.Operation, "Not equal 'Operation' inserted");
                    Assert.True(exception.MessageBody == null, "Inserted exception body is not empty");
                });
        }

        private static MessagingContext ContextWithAS4UserMessage(bool notifyProducer, string id)
        {
            return new MessagingContext(AS4Message.Create(new FilledUserMessage(id)), default(MessagingContextMode))
            {
                SendingPMode = new SendingProcessingMode {ExceptionHandling = {NotifyMessageProducer = notifyProducer}}
            };
        }
    }
}
