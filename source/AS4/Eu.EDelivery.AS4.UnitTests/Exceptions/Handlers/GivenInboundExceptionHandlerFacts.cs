using System;
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
    public class GivenInboundExceptionHandlerFacts : GivenDatastoreFacts
    {
        private readonly string _expectedId = Guid.NewGuid().ToString();

        [Property]
        public void Set_Retry_Info_When_ReceivingPMode_Is_Configured_For_Retry(
            bool enabled, 
            PositiveInt count, 
            TimeSpan interval)
        {
            // Arrange
            ClearInExceptions();
            var sut = new InboundExceptionHandler(GetDataStoreContext);
            var pmode = new ReceivingProcessingMode();
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
                new MessagingContext(new SubmitMessage()) { ReceivingPMode = pmode })
               .GetAwaiter()
               .GetResult();

            // Assert
            GetDataStoreContext.AssertInException(ex =>
            {
                Assert.NotEmpty(ex.MessageBody);
                GetDataStoreContext.AssertRetryRelatedInException(
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

        private void ClearInExceptions()
        {
            using (DatastoreContext ctx = GetDataStoreContext())
            {
                ctx.InExceptions.RemoveRange(ctx.InExceptions);
                ctx.SaveChanges();
            }
        }

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
                    Assert.True(ex.Exception.IndexOf(expectedMessage, StringComparison.CurrentCultureIgnoreCase) > -1);
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

        private async Task TestExecutionException(
            Operation expected,
            MessagingContext context,
            Func<IAgentExceptionHandler, Func<Exception, MessagingContext, Task<MessagingContext>>> getExercise)
        {
            // Arrange
            var inMessage = new InMessage(ebmsMessageId: _expectedId);
            inMessage.SetStatus(InStatus.Received);

            GetDataStoreContext.InsertInMessage(inMessage);

            var sut = new InboundExceptionHandler(GetDataStoreContext);
            var exercise = getExercise(sut);

            // Act
            await exercise(new Exception(), context);

            // Assert
            GetDataStoreContext.AssertInMessage(_expectedId, m => Assert.Equal(InStatus.Exception, m.Status.ToEnum<InStatus>()));
            GetDataStoreContext.AssertInException(
                _expectedId,
                ex =>
                {
                    Assert.Equal(expected, ex.Operation.ToEnum<Operation>());
                    Assert.Null(ex.MessageBody);
                });
        }

        private static MessagingContext ContextWithAS4UserMessage(string id, bool notifyConsumer)
        {
            return new MessagingContext(
                AS4Message.Create(new FilledUserMessage(id)),
                MessagingContextMode.Receive)
            {
                ReceivingPMode =
                    new ReceivingProcessingMode { ExceptionHandling = { NotifyMessageConsumer = notifyConsumer } }
            };
        }
    }
}
