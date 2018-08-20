using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Forward;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Forward
{
    public class GivenDetermineForwardStepFacts
    {
        public class ValidMessagingContextFacts
        {
            [Fact]
            public async Task SendingPModeCorrectlyDetermined()
            {
                const string sendingPModeId = "Forward_SendingPMode_Id";

                var receivingPMode = new ReceivingProcessingMode()
                {
                    MessageHandling = new MessageHandling()
                    {
                        Item = new AS4.Model.PMode.Forward()
                        {
                            SendingPMode = sendingPModeId
                        }
                    }
                };

                var config = new StubConfig(sendingPModes: new Dictionary<string, SendingProcessingMode>()
                                            {
                                                {sendingPModeId,new SendingProcessingMode(){Id=sendingPModeId}}
                                            },
                                            receivingPModes: new Dictionary<string, ReceivingProcessingMode>());

                var context = new MessagingContext(new ReceivedMessage(Stream.Null), MessagingContextMode.Forward)
                {
                    ReceivingPMode = receivingPMode
                };

                var sut = new DetermineRoutingStep(config);
                var result = await sut.ExecuteAsync(context);

                Assert.True(result.Succeeded);
                Assert.NotNull(result.MessagingContext.SendingPMode);
            }
        }

        public class InvalidMessagingContextFacts
        {
            [Fact]
            public async Task ExceptionWhenNoReceivingPModeAvailable()
            {
                var messagingContext = new MessagingContext(new ReceivedMessage(Stream.Null), MessagingContextMode.Forward) { ReceivingPMode = null };

                var step = new DetermineRoutingStep(StubConfig.Default);

                await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(messagingContext));
            }


            [Fact]
            public async Task ExceptionWhenReceivingPModeIsInvalid()
            {
                var receivingPMode = new ReceivingProcessingMode()
                {
                    MessageHandling = new MessageHandling()
                    {
                        Item = new AS4.Model.PMode.Deliver()
                    }
                };

                var messagingContext = new MessagingContext(new ReceivedMessage(Stream.Null), MessagingContextMode.Forward)
                {
                    ReceivingPMode = receivingPMode
                };

                var step = new DetermineRoutingStep(StubConfig.Default);

                await Assert.ThrowsAsync<ConfigurationErrorsException>(() => step.ExecuteAsync(messagingContext));
            }

            [Fact]
            public async Task ExceptionWhenSendingPModeNotFound()
            {
                var receivingPMode = new ReceivingProcessingMode
                {
                    MessageHandling = new MessageHandling
                    {
                        Item = new AS4.Model.PMode.Forward
                        {
                            SendingPMode = "Forward_SendingPMode_Id"
                        }
                    }
                };

                var messagingContext = new MessagingContext(new ReceivedMessage(Stream.Null), MessagingContextMode.Forward)
                {
                    ReceivingPMode = receivingPMode
                };

                var step = new DetermineRoutingStep(StubConfig.Default);

                await Assert.ThrowsAsync<ConfigurationErrorsException>(() => step.ExecuteAsync(messagingContext));
            }
        }
    }
}

