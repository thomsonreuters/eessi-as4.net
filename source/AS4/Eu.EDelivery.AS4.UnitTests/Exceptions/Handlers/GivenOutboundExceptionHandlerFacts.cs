using System;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Model.Deliver;
using Eu.EDelivery.AS4.UnitTests.Model.Notify;
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
                    Assert.True(ex.Exception.IndexOf(_expectedException.Message, StringComparison.CurrentCultureIgnoreCase) > -1, "Not equal message insterted");
                    Assert.True(_expectedBody == Encoding.UTF8.GetString(ex.MessageBody), "Not equal body inserted");
                });
        }

        [Theory]
        [InlineData(true, Operation.ToBeNotified)]
        [InlineData(false, default(Operation))]
        public async Task InsertOutException_IfStepExecutionException(bool notifyProducer, Operation expected)
        {
            await TestHandleExecutionException(
                expected,
                ContextWithAS4UserMessage(notifyProducer, _expectedId),
                sut => sut.HandleExecutionException);
        }

        [Theory]
        [InlineData(true, Operation.ToBeNotified)]
        [InlineData(false, default(Operation))]
        public async Task InsertOutException_IfErrorException(bool notifyProducer, Operation expected)
        {
            await TestHandleExecutionException(
                expected,
                ContextWithAS4UserMessage(notifyProducer, _expectedId),
                sut => sut.HandleErrorException);
        }

        private static MessagingContext ContextWithAS4UserMessage(bool notifyProducer, string id)
        {
            return new MessagingContext(AS4Message.Create(new FilledUserMessage(id)), default(MessagingContextMode))
            {
                SendingPMode = new SendingProcessingMode { ExceptionHandling = { NotifyMessageProducer = notifyProducer } }
            };
        }

        [Fact]
        public async Task InsertOutException_IfDeliverMessage()
        {
            var deliverEnvelope = new EmptyDeliverEnvelope(_expectedId);

            await TestHandleExecutionException(
                default(Operation),
                new MessagingContext(deliverEnvelope),
                sut => sut.HandleExecutionException);
        }

        [Fact]
        public async Task InsertOutException_IfNotifyMessage()
        {
            var notifyEnvelope = new EmptyNotifyEnvelope(_expectedId);

            await TestHandleExecutionException(
                default(Operation),
                new MessagingContext(notifyEnvelope),
                sut => sut.HandleExecutionException);
        }

        [Fact]
        public async Task InsertOutMessage_IfSubmitMessage()
        {
            // Arrange
            var sut = new OutboundExceptionHandler(GetDataStoreContext);

            // Act
            await sut.HandleExecutionException(new Exception(), new MessagingContext(new SubmitMessage()));

            // Assert
            GetDataStoreContext.AssertOutException(ex => Assert.NotNull(ex.MessageBody));
        }

        private async Task TestHandleExecutionException(
            Operation expected,
            MessagingContext context,
            Func<IAgentExceptionHandler, Func<Exception, MessagingContext, Task<MessagingContext>>> getExercise)
        {
            // Arrange
            GetDataStoreContext.InsertOutMessage(new OutMessage { EbmsMessageId = _expectedId, Status = OutStatus.Sent });

            var sut = new OutboundExceptionHandler(GetDataStoreContext);
            Func<Exception, MessagingContext, Task<MessagingContext>> exercise = getExercise(sut);

            // Act
            await exercise(_expectedException, context);

            // Assert
            GetDataStoreContext.AssertOutMessage(_expectedId, m => Assert.Equal(OutStatus.Exception, m.Status));
            GetDataStoreContext.AssertOutException(
                _expectedId,
                exception =>
                {
                    Assert.True(exception.Exception.IndexOf(_expectedException.Message, StringComparison.CurrentCultureIgnoreCase) > -1, "Message does not contain expected message");
                    Assert.True(expected == OperationUtils.Parse(exception.Operation), "Not equal 'Operation' inserted");
                    Assert.True(exception.MessageBody == null, "Inserted exception body is not empty");
                });
        }
    }
}
