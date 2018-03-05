using System;
using System.IO;
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
            var context = SetupMessagingContext(_expectedId);

            context.ModifyContext(AS4Message.Create(new FilledUserMessage(_expectedId)));
            context.SendingPMode = new SendingProcessingMode { ExceptionHandling = { NotifyMessageProducer = notifyProducer } };

            await TestHandleExecutionException(
                expected,
                context,
                sut => sut.HandleExecutionException);
        }

        [Theory]
        [InlineData(true, Operation.ToBeNotified)]
        [InlineData(false, default(Operation))]
        public async Task InsertOutException_IfErrorException(bool notifyProducer, Operation expected)
        {
            var context = SetupMessagingContext(_expectedId);

            context.ModifyContext(AS4Message.Create(new FilledUserMessage(_expectedId)));
            context.SendingPMode = new SendingProcessingMode { ExceptionHandling = { NotifyMessageProducer = notifyProducer } };

            await TestHandleExecutionException(
                expected,
                context,
                sut => sut.HandleErrorException);
        }

        [Fact]
        public async Task InsertOutException_IfDeliverMessage()
        {
            var context = SetupMessagingContext(_expectedId);

            var deliverEnvelope = new EmptyDeliverEnvelope(_expectedId);

            context.ModifyContext(deliverEnvelope);

            await TestHandleExecutionException(
                default(Operation),
                context,
                sut => sut.HandleExecutionException);
        }

        [Fact]
        public async Task InsertOutException_IfNotifyMessage()
        {
            var context = SetupMessagingContext(_expectedId);

            var notifyEnvelope = new EmptyNotifyEnvelope(_expectedId);

            context.ModifyContext(notifyEnvelope);

            await TestHandleExecutionException(
                default(Operation),
                context,
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

        private MessagingContext SetupMessagingContext(string ebmsMessageId)
        {
            // Arrange
            var message = new OutMessage(ebmsMessageId: ebmsMessageId);
            message.SetStatus(OutStatus.Sent);

            GetDataStoreContext.InsertOutMessage(message, withReceptionAwareness: false);

            var receivedMessage = new ReceivedEntityMessage(message, Stream.Null, string.Empty);

            var context = new MessagingContext(receivedMessage, MessagingContextMode.Unknown);

            return context;
        }

        private async Task TestHandleExecutionException(
            Operation expected,
            MessagingContext context,
            Func<IAgentExceptionHandler, Func<Exception, MessagingContext, Task<MessagingContext>>> getExercise)
        {
            var sut = new OutboundExceptionHandler(GetDataStoreContext);
            Func<Exception, MessagingContext, Task<MessagingContext>> exercise = getExercise(sut);

            // Act
            await exercise(_expectedException, context);

            // Assert
            GetDataStoreContext.AssertOutMessage(_expectedId, m => Assert.Equal(OutStatus.Exception, OutStatusUtils.Parse(m.Status)));
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
