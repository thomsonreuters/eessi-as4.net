using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using DatabaseSpy = Eu.EDelivery.AS4.ComponentTests.Common.DatabaseSpy;
using RetryReliability = Eu.EDelivery.AS4.Entities.RetryReliability;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class RetryAgentForSendingFacts : ComponentTestTemplate
    {
        [Theory]
        [InlineData(HttpStatusCode.Accepted, Operation.Sent)]
        [InlineData(HttpStatusCode.InternalServerError, Operation.DeadLettered)]
        public async Task OutMessage_Is_Set_To_Sent_When_Retry_Happen_Within_Allowed_MaxRetry(
            HttpStatusCode secondAttempt,
            Operation expected)
        {
            await TestComponentWithSettings(
                "sendagent_settings.xml",
                async (settings, as4Msh) =>
                {
                    // Arrange
                    const string url = "http://localhost:7171/business/sending/";
                    SendingProcessingMode pmode = ReceptionAwarenessSendingPMode(url);
                    OutMessage im = CreateOutMessageRefStoredAS4Message(as4Msh);

                    InsertMessageEntityWithRetry(
                        entity: im,
                        config: as4Msh.GetConfiguration(),
                        pmode: pmode,
                        createRetry: id => RetryReliability.CreateForOutMessage(
                            id,
                            pmode.Reliability.ReceptionAwareness.RetryCount,
                            pmode.Reliability.ReceptionAwareness.RetryInterval.AsTimeSpan(),
                            RetryType.Send));

                    // Act
                    SimulateSendingFailureOnFirstAttempt(url, secondAttempt);

                    // Assert
                    var spy = new DatabaseSpy(as4Msh.GetConfiguration());
                    OutMessage sent = await PollUntilPresent(
                        () => spy.GetOutMessageFor(m => m.Operation == expected),
                        timeout: TimeSpan.FromSeconds(10));
                    Assert.Equal(im.EbmsMessageId, sent.EbmsMessageId);

                    RetryReliability referenced = await PollUntilPresent(
                        () => spy.GetRetryReliabilityFor(r => r.RefToOutMessageId == sent.Id),
                        timeout: TimeSpan.FromSeconds(5));
                    Assert.Equal(RetryStatus.Completed, referenced.Status);

                    InMessage deadLetteredError =
                        spy.GetInMessageFor(m => m.EbmsRefToMessageId == sent.EbmsMessageId);

                    bool storedDeadLetteredError =
                        deadLetteredError?.Operation == Operation.ToBeNotified
                        && deadLetteredError?.EbmsMessageType == MessageType.Error;

                    Assert.True(
                        storedDeadLetteredError == (expected == Operation.DeadLettered),
                        "Expected to have stored AS4 Error for DeadLettered message");
                });
        }

        private static OutMessage CreateOutMessageRefStoredAS4Message(AS4Component as4Msh)
        {
            string ebmsMessageId = $"receipt-{Guid.NewGuid()}";
            var store = new AS4MessageBodyFileStore();
            return new OutMessage(ebmsMessageId)
            {
                ContentType = Constants.ContentTypes.Soap,
                MessageLocation = store.SaveAS4Message(
                    as4Msh.GetConfiguration().InMessageStoreLocation,
                    AS4Message.Create(
                        new Receipt(
                            ebmsMessageId,
                            $"reftoid-{Guid.NewGuid()}",
                            DateTimeOffset.Now)))
            };
        }

        private static SendingProcessingMode ReceptionAwarenessSendingPMode(string url)
        {
            return new SendingProcessingMode
            {
                PushConfiguration = new PushConfiguration
                {
                    Protocol =
                    {
                        Url = url
                    }
                },
                ErrorHandling =
                {
                    NotifyMessageProducer = true
                },
                Reliability =
                {
                    ReceptionAwareness =
                    {
                        IsEnabled = true,
                        RetryCount = 1,
                        RetryInterval = "00:00:01"
                    }
                }
            };
        }

        private static void InsertMessageEntityWithRetry(
            OutMessage entity,
            IConfig config,
            SendingProcessingMode pmode,
            Func<long, RetryReliability> createRetry)
        {
            using (var ctx = new DatastoreContext(config))
            {
                entity.SetPModeInformation(pmode);
                entity.Operation = Operation.ToBeSent;

                ctx.OutMessages.Add(entity);
                ctx.SaveChanges();

                RetryReliability r = createRetry(entity.Id);
                ctx.RetryReliability.Add(r);
                ctx.SaveChanges();
            }
        }

        private static void SimulateSendingFailureOnFirstAttempt(string url, HttpStatusCode secondAttempt)
        {
            var onSecondAttempt = new ManualResetEvent(initialState: false);
            StubHttpServer.SimulateFailureOnFirstAttempt(url, secondAttempt, onSecondAttempt);
            Assert.True(onSecondAttempt.WaitOne(TimeSpan.FromMinutes(1)));
        }
    }
}
