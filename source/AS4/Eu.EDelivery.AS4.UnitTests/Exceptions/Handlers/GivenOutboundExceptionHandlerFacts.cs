using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Model.Deliver;
using Eu.EDelivery.AS4.UnitTests.Model.Notify;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using RetryReliability = Eu.EDelivery.AS4.Model.PMode.RetryReliability;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class GivenOutboundExceptionHandlerFacts : GivenDatastoreFacts
    {
        private readonly string _expectedId = Guid.NewGuid().ToString();

        private readonly string _expectedBody = Guid.NewGuid().ToString();

        private readonly Exception _expectedException = new Exception(Guid.NewGuid().ToString());

        [Property]
        public void Set_Retry_Info_When_SendingPMode_Is_Configured_For_Retry(
            bool enabled, 
            PositiveInt count, 
            TimeSpan interval)
        {
            // Arrange
            ClearOutExceptions();
            var sut = new OutboundExceptionHandler(GetDataStoreContext);
            var pmode = new SendingProcessingMode();
            string intervalStr = interval.ToString("G");
            pmode.ExceptionHandling.Reliability =
                new RetryReliability
                {
                    IsEnabled = enabled,
                    RetryCount = count.Get,
                    RetryInterval = intervalStr
                };

            // Act
            sut.HandleExecutionException(
                new Exception(),
                new MessagingContext(new SubmitMessage()) { SendingPMode = pmode })
               .GetAwaiter()
               .GetResult();

            // Assert
            GetDataStoreContext.AssertOutException(ex =>
            {
                Assert.NotNull(ex.MessageBody);
                GetDataStoreContext.AssertRetryRelatedOutException(
                    ex.Id,
                    rr =>
                    {
                        Assert.True(
                            enabled == (0 == rr?.CurrentRetryCount),
                            "CurrentRetryCount != 0 when RetryReliability is enabled");
                        Assert.True(
                            enabled == (count.Get == rr?.MaxRetryCount),
                            enabled
                                ? $"Max retry count failed on enabled: {count.Get} != {rr?.MaxRetryCount}"
                                : $"Max retry count should be 0 on disabled but is {rr?.MaxRetryCount}");
                        Assert.True(
                            enabled == (intervalStr == rr?.RetryInterval),
                            enabled
                                ? $"Retry interval failed on enabled: {interval:G} != {rr?.RetryInterval}"
                                : $"Retry interval should be 0:00:00 on disabled but is {rr?.RetryInterval}");
                    });
            });
        }

        private void ClearOutExceptions()
        {
            using (DatastoreContext ctx = GetDataStoreContext())
            {
                ctx.OutExceptions.RemoveRange(ctx.OutExceptions);
                ctx.SaveChanges();
            }
        }

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
            var context = SetupMessagingContextForOutMessage(_expectedId);

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
            var context = SetupMessagingContextForOutMessage(_expectedId);

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
            var context = SetupMessagingContextForOutMessage(_expectedId);

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
            var context = SetupMessagingContextForOutMessage(_expectedId);

            var notifyEnvelope = new EmptyNotifyEnvelope(_expectedId);

            context.ModifyContext(notifyEnvelope);

            await TestHandleExecutionException(
                default(Operation),
                context,
                sut => sut.HandleExecutionException);
        }

        private MessagingContext SetupMessagingContextForOutMessage(string ebmsMessageId)
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
            GetDataStoreContext.AssertOutMessage(_expectedId, m => Assert.Equal(OutStatus.Exception, m.Status.ToEnum<OutStatus>()));
            GetDataStoreContext.AssertOutException(
                _expectedId,
                exception =>
                {
                    Assert.True(exception.Exception.IndexOf(_expectedException.Message, StringComparison.CurrentCultureIgnoreCase) > -1, "Message does not contain expected message");
                    Assert.True(expected == exception.Operation.ToEnum<Operation>(), "Not equal 'Operation' inserted");
                    Assert.True(exception.MessageBody == null, "Inserted exception body is not empty");
                });
        }
    }
}
