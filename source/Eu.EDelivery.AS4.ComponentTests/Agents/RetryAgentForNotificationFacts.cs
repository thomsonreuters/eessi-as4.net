using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using Exception = System.Exception;
using Parameter = Eu.EDelivery.AS4.Model.PMode.Parameter;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using RetryReliability = Eu.EDelivery.AS4.Model.PMode.RetryReliability;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class RetryAgentForNotificationFacts : ComponentTestTemplate
    {
        [Theory]
        [InlineData(200, Operation.Notified)]
        [InlineData(500, Operation.DeadLettered)]
        public async Task InMessage_Is_Set_To_Notified_When_Retry_Happen_Within_Allowed_MaxRetry(
            int secondAttemptStatusCode,
            Operation expected)
        {
            await TestComponentWithSettings(
                "inmessage_notify_reliability_settings.xml",
                async (settings, as4Msh) =>
                {
                    // Arrange
                    const string url = "http://localhost:7071/business/inmessage/";
                    string ebmsMessageId = $"receipt-{Guid.NewGuid()}";

                    var store = new AS4MessageBodyFileStore(SerializerProvider.Default);
                    var im = new InMessage(ebmsMessageId)
                    {
                        ContentType = Constants.ContentTypes.Soap,
                        MessageLocation = store.SaveAS4Message(
                            as4Msh.GetConfiguration().InMessageStoreLocation,
                            AS4Message.Create(new Receipt(ebmsMessageId, $"reftoid-{Guid.NewGuid()}")))
                    };

                    SendingProcessingMode pmode = NotifySendingPMode(url);
                    Entities.RetryReliability CreateRetry(long id)
                        => Entities.RetryReliability.CreateForInMessage(
                            refToInMessageId: id, 
                            maxRetryCount: pmode.ReceiptHandling.Reliability.RetryCount, 
                            retryInterval: pmode.ReceiptHandling.Reliability.RetryInterval.AsTimeSpan(), 
                            type: RetryType.Notification);

                    // Act
                    InsertMessageEntityWithRetry(im, as4Msh.GetConfiguration(), pmode, CreateRetry);

                    // Assert
                    SimulateNotifyFailureOnFirstAttempt(url, secondAttemptStatusCode);

                    var spy = new DatabaseSpy(as4Msh.GetConfiguration());
                    InMessage notified = await PollUntilPresent(
                        () => spy.GetInMessageFor(
                            m => m.Operation.ToEnum<Operation>() == expected),
                        timeout: TimeSpan.FromSeconds(10));

                    Assert.Equal(ebmsMessageId, notified.EbmsMessageId);

                    Entities.RetryReliability referenced = await PollUntilPresent(
                        () => spy.GetRetryReliabilityFor(r => r.RefToInMessageId == notified.Id),
                        timeout: TimeSpan.FromSeconds(5));
                    Assert.Equal(RetryStatus.Completed, referenced.Status.ToEnum<RetryStatus>());
                });
        }

        [Theory]
        [InlineData(200, Operation.Notified)]
        [InlineData(500, Operation.DeadLettered)]
        public async Task OutMessage_Is_To_Notified_When_Retry_Happen_Withing_Allowed_MaxRetry(
            int secondAttemptStatusCode,
            Operation expected)
        {
            await TestComponentWithSettings(
                "outmessage_notify_reliability_settings.xml",
                async (settings, as4Msh) =>
                {
                    // Arrange
                    const string url = "http://localhost:7071/business/outmessage/";
                    string ebmsMessageId = $"error-{Guid.NewGuid()}";

                    var store = new AS4MessageBodyFileStore(SerializerProvider.Default);
                    var om = new OutMessage(ebmsMessageId)
                    {
                        ContentType = Constants.ContentTypes.Soap,
                        MessageLocation = store.SaveAS4Message(
                            as4Msh.GetConfiguration().InMessageStoreLocation,
                            AS4Message.Create(new Error(ebmsMessageId)
                            {
                                Errors = new[] { new ErrorDetail { ShortDescription = "this is an error" } }
                            }))
                    };

                    SendingProcessingMode pmode = NotifySendingPMode(url);
                    Entities.RetryReliability CreateRetry(long id) 
                        => Entities.RetryReliability.CreateForOutMessage(
                            refToOutMessageId: id, 
                            maxRetryCount: pmode.ErrorHandling.Reliability.RetryCount, 
                            retryInterval: pmode.ErrorHandling.Reliability.RetryInterval.AsTimeSpan(), 
                            type: RetryType.Notification);

                    // Act
                    InsertMessageEntityWithRetry(om, as4Msh.GetConfiguration(), pmode, CreateRetry);

                    // Assert
                    SimulateNotifyFailureOnFirstAttempt(url, secondAttemptStatusCode);

                    var spy = new DatabaseSpy(as4Msh.GetConfiguration());
                    OutMessage notified = await PollUntilPresent(
                        () => spy.GetOutMessageFor(
                            m => m.Operation.ToEnum<Operation>() == expected),
                        timeout: TimeSpan.FromSeconds(10));
                    Assert.Equal(ebmsMessageId, notified.EbmsMessageId);

                    Entities.RetryReliability referenced = await PollUntilPresent(
                        () => spy.GetRetryReliabilityFor(r => r.RefToOutMessageId == notified.Id),
                        timeout: TimeSpan.FromSeconds(5));
                    Assert.Equal(RetryStatus.Completed, referenced.Status.ToEnum<RetryStatus>());
                });
        }

        private static void InsertMessageEntityWithRetry(
            MessageEntity im, 
            IConfig config, 
            SendingProcessingMode pmode,
            Func<long, Entities.RetryReliability> createRetry)
        {
            using (var ctx = new DatastoreContext(config))
            {
                im.SetPModeInformation(pmode);
                im.SetOperation(Operation.ToBeNotified);

                ctx.Add(im);
                ctx.SaveChanges();

                var r = createRetry(im.Id);

                ctx.RetryReliability.Add(r);
                ctx.SaveChanges();
            }
        }

        [Theory]
        [InlineData(200, Operation.Notified)]
        [InlineData(500, Operation.DeadLettered)]
        public async Task OutException_Is_Set_To_Notified_When_Retry_Happen_Within_Allowed_MaxRetry(
            int secondAttempt,
            Operation expected)
        {
            await TestComponentWithSettings(
                "outexception_notify_reliability_settings.xml",
                async (settings, as4Msh) =>
                {
                    // Arrange
                    var handler = new OutboundExceptionHandler(
                        () => new DatastoreContext(as4Msh.GetConfiguration()));

                    const string url = "http://localhost:7070/business/outexception/";

                    // Act
                    await handler.HandleExecutionException(
                        new Exception("This is an test exception"),
                        new MessagingContext(new SubmitMessage())
                        {
                            SendingPMode = NotifySendingPMode(url)
                        });

                    // Arrange
                    SimulateNotifyFailureOnFirstAttempt(url, secondAttempt);

                    var spy = new DatabaseSpy(as4Msh.GetConfiguration());
                    OutException notified =
                        await PollUntilPresent(
                            () => spy.GetOutExceptions(
                                ex => ex.Operation.ToEnum<Operation>() == expected).FirstOrDefault(),
                            timeout: TimeSpan.FromSeconds(10));

                    Entities.RetryReliability referenced = await PollUntilPresent(
                        () => spy.GetRetryReliabilityFor(r => r.RefToOutExceptionId == notified.Id),
                        timeout: TimeSpan.FromSeconds(5));
                    Assert.Equal(RetryStatus.Completed, referenced.Status.ToEnum<RetryStatus>());
                });
        }

        private static SendingProcessingMode NotifySendingPMode(string url)
        {
            var notifyMethod = new Method
            {
                Type = "HTTP",
                Parameters = new List<Parameter>
                {
                    new Parameter { Name = "location", Value = url }
                }
            };

            var reliability = new RetryReliability
            {
                IsEnabled = true,
                RetryCount = 1,
                RetryInterval = "00:00:01"
            };

            return new SendingProcessingMode
            {
                ReceiptHandling =
                {
                    NotifyMessageProducer = true,
                    NotifyMethod = notifyMethod,
                    Reliability = reliability
                },
                ErrorHandling =
                {
                    NotifyMessageProducer = true,
                    NotifyMethod = notifyMethod,
                    Reliability = reliability
                },
                ExceptionHandling =
                {
                    NotifyMessageProducer = true,
                    NotifyMethod = notifyMethod,
                    Reliability = reliability
                }
            };
        }

        [Theory]
        [InlineData(200, Operation.Notified)]
        [InlineData(500, Operation.DeadLettered)]
        public async Task InException_Is_Set_To_Notified_When_Retry_Happen_Within_Allowed_MaxRetry(
            int secondAttempt,
            Operation expected)
        {
            await TestComponentWithSettings(
                "inexception_notify_reliability_settings.xml",
                async (settings, as4Msh) =>
                {
                    // Arrange
                    var handler = new InboundExceptionHandler(
                        () => new DatastoreContext(as4Msh.GetConfiguration()));

                    const string url = "http://localhost:7071/business/inexception/";

                    // Act
                    await handler.HandleExecutionException(
                        new Exception("This is an test exception"),
                        new MessagingContext(new SubmitMessage())
                        {
                            ReceivingPMode = NotifyExceptionReceivePMode(url)
                        });

                    // Assert
                    SimulateNotifyFailureOnFirstAttempt(url, secondAttempt);

                    var spy = new DatabaseSpy(as4Msh.GetConfiguration());
                    InException notified = await PollUntilPresent(
                        () => spy.GetInExceptions(
                            ex => ex.Operation.ToEnum<Operation>() == expected).FirstOrDefault(),
                        timeout: TimeSpan.FromSeconds(10));

                    Entities.RetryReliability referenced = await PollUntilPresent(
                        () => spy.GetRetryReliabilityFor(r => r.RefToInExceptionId == notified.Id),
                        timeout: TimeSpan.FromSeconds(5));
                    Assert.Equal(RetryStatus.Completed, referenced.Status.ToEnum<RetryStatus>());
                });
        }

        private static ReceivingProcessingMode NotifyExceptionReceivePMode(string url)
        {
            return new ReceivingProcessingMode
            {
                ExceptionHandling =
                {
                    NotifyMessageConsumer = true,
                    NotifyMethod =
                    {
                        Type = "HTTP",
                        Parameters = new List<Parameter>
                        {
                            new Parameter { Name = "location", Value = url }
                        }
                    },
                    Reliability =
                    {
                        IsEnabled = true,
                        RetryCount = 1,
                        RetryInterval = "00:00:01"
                    }
                }
            };
        }

        private static void SimulateNotifyFailureOnFirstAttempt(string url, int secondAttempt)
        {
            var onSecondAttempt = new ManualResetEvent(initialState: false);
            var currentRetryCount = 0;
            StubHttpServer.StartServerLifetime(
                url,
                r =>
                {
                    if (++currentRetryCount >= 2)
                    {
                        r.StatusCode = secondAttempt;
                        r.OutputStream.WriteByte(0);
                        r.OutputStream.Dispose();
                        return ServerLifetime.Stop;
                    }

                    r.StatusCode = 500;
                    r.OutputStream.WriteByte(0);
                    r.OutputStream.Dispose();
                    return ServerLifetime.Continue;
                },
                onSecondAttempt);

            Assert.True(onSecondAttempt.WaitOne(TimeSpan.FromMinutes(1)));
        }
    }
}