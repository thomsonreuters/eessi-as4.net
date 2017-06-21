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
using MessageInfo = Eu.EDelivery.AS4.Model.Common.MessageInfo;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class GivenInboundExceptionHandlerFacts : GivenDatastoreFacts
    {
        private readonly string _expectedId = Guid.NewGuid().ToString();

        [Fact]
        public async Task InsertInException_IfTransformException()
        {
            // Arrange
            string expectedBody = Guid.NewGuid().ToString(), 
                expectedMessage = Guid.NewGuid().ToString();

            var sut = new InboundExceptionHandler(GetDataStoreContext);

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
        public async Task InsertInException_IfErrorException(bool notifyConsumer, Operation expected)
        {
            await TestExecutionException(
                expected, 
                ContextWithAS4UserMessage(_expectedId, notifyConsumer),
                sut => sut.HandleErrorException);
        }

        [Theory]
        [InlineData(true, Operation.ToBeNotified)]
        [InlineData(false, default(Operation))]
        public async Task InsertInException_IfExecutionException(bool notifyConsumer, Operation expected)
        {
            await TestExecutionException(
                expected, 
                ContextWithAS4UserMessage(_expectedId, notifyConsumer),
                sut => sut.HandleExecutionException);
        }

        [Fact]
        public async Task InsertInException_WithDeliverMessage()
        {
            var envelope = new EmptyDeliverEnvelope(_expectedId);

            await TestExecutionException(
                default(Operation),
                new MessagingContext(envelope),
                sut => sut.HandleExecutionException);
        }

        [Fact]
        public async Task InsertInException_WithNotifyMessage()
        {
            var envelope = new EmptyNotifyEnvelope(_expectedId);

            await TestExecutionException(
                default(Operation),
                new MessagingContext(envelope),
                sut => sut.HandleErrorException);
        }

        [Fact]
        public async Task InsertInException_WithSubmitMessage()
        {
            var submitMessage = new SubmitMessage {MessageInfo = new MessageInfo(_expectedId, mpc: null)};

            await TestExecutionException(
                default(Operation),
                new MessagingContext(submitMessage),
                sut => sut.HandleExecutionException);
        }

        private async Task TestExecutionException(
            Operation expected,
            MessagingContext context,
            Func<IAgentExceptionHandler, Func<Exception, MessagingContext, Task<MessagingContext>>> getExercise)
        {
            // Arrange
            GetDataStoreContext.InsertInMessage(new InMessage {EbmsMessageId = _expectedId, Status = InStatus.Received});

            var sut = new InboundExceptionHandler(GetDataStoreContext);
            var exercise = getExercise(sut);

            // Act
            await exercise(new Exception(), context);

            // Assert
            GetDataStoreContext.AssertInMessage(_expectedId, m => Assert.Equal(InStatus.Exception, m.Status));
            GetDataStoreContext.AssertInException(
                _expectedId,
                ex =>
                {
                    Assert.Equal(expected, ex.Operation);
                    Assert.Null(ex.MessageBody);
                });
        }

        private static MessagingContext ContextWithReceivingPMode(Func<MessagingContext> createContext, bool notifyConsumer)
        {
            MessagingContext context = createContext();
            context.ReceivingPMode = new ReceivingProcessingMode
            {
                ExceptionHandling = {NotifyMessageConsumer = notifyConsumer}
            };

            return context;
        }

        private static MessagingContext ContextWithAS4UserMessage(string id, bool notifyConsumer)
        {
            var message = new MessagingContext(AS4Message.Create(new FilledUserMessage(id)), MessagingContextMode.Receive);
            message.ReceivingPMode = new ReceivingProcessingMode {ExceptionHandling = {NotifyMessageConsumer = notifyConsumer}};

            return message;
        }
    }
}
