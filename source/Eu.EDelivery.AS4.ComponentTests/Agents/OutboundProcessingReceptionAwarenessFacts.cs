using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class OutboundProcessingReceptionAwarenessFacts : ComponentTestTemplate
    {
        [Fact]
        public async Task Stores_Correctly_Retry_Information_In_Sql_Server()
        {
            await TestComponentWithSettings(
                "outboundprocessingagent_settings_sqlserver.xml",
                async (settings, msh) =>
                {
                    // Arrange
                    var userMessage = new UserMessage($"user-{Guid.NewGuid()}");
                    var sendingPMode = new SendingProcessingMode
                    {
                        Id = "outboundprocessing-reliability",
                        Reliability =
                        {
                            ReceptionAwareness =
                            {
                                IsEnabled = true,
                                RetryCount = 3,
                                RetryInterval = "00:00:05"
                            }
                        }
                    };
                    var as4Message = AS4Message.Create(userMessage, sendingPMode);

                    var mshConfig = TestConfig.Create(settings);

                    var spy = DatabaseSpy.Create(mshConfig);

                    var toBeProcessed = new OutMessage(userMessage.MessageId)
                    {
                        ContentType = as4Message.ContentType,
                        EbmsMessageType = MessageType.UserMessage,
                        Operation = Operation.ToBeProcessed,
                        MessageLocation =
                            Registry.Instance
                                    .MessageBodyStore
                                    .SaveAS4Message(msh.GetConfiguration().OutMessageStoreLocation, as4Message)
                    };
                    toBeProcessed.SetPModeInformation(sendingPMode);

                    // Act
                    spy.InsertOutMessage(toBeProcessed);

                    // Assert
                    OutMessage stored = await PollUntilPresent(
                        () => spy.GetOutMessageFor(m => m.EbmsMessageId == userMessage.MessageId),
                        timeout: TimeSpan.FromSeconds(40));

                    await PollUntilPresent(
                        () => spy.GetRetryReliabilityFor(r => r.RefToOutMessageId == stored.Id),
                        timeout: TimeSpan.FromSeconds(10));
                });
        }

        [Fact]
        public async Task SendRetry_IsEnabled_For_ProcessedMessage_With_Multihop_Information()
        {
            await TestComponentWithSettings(
                   "outboundprocessingagent_settings_sqlserver.xml",
                   async (settings, msh) =>
                   {
                       // Arrange
                       var userMessage = new UserMessage($"user-{Guid.NewGuid()}");
                       var sendingPMode = new SendingProcessingMode
                       {
                           Id = "outboundprocessing-reliability",
                           Reliability =
                           {
                                ReceptionAwareness =
                                {
                                    IsEnabled = true,
                                    RetryCount = 3,
                                    RetryInterval = "00:00:05"
                                }
                           },
                           MessagePackaging =
                           {
                               IsMultiHop =  true
                           }
                       };
                       var as4Message = AS4Message.Create(userMessage, sendingPMode);

                       var mshConfig = TestConfig.Create(settings);

                       var spy = DatabaseSpy.Create(mshConfig);

                       var toBeProcessed = new OutMessage(userMessage.MessageId)
                       {
                           ContentType = as4Message.ContentType,
                           EbmsMessageType = MessageType.UserMessage,
                           Operation = Operation.ToBeProcessed,
                           MessageLocation =
                               Registry.Instance
                                       .MessageBodyStore
                                       .SaveAS4Message(msh.GetConfiguration().OutMessageStoreLocation, as4Message)
                       };
                       toBeProcessed.SetPModeInformation(sendingPMode);

                       // Act
                       spy.InsertOutMessage(toBeProcessed);

                       // Assert
                       Assert.True(toBeProcessed.Id != default(long));

                       await PollUntilSatisfied(
                              () => spy.GetOutMessageFor(m => m.EbmsMessageId == userMessage.MessageId)?.Operation == Operation.ToBeSent,
                              timeout: TimeSpan.FromSeconds(30));

                       Assert.True(spy.GetRetryReliabilityFor(r => r.RefToOutMessageId == toBeProcessed.Id) != null,
                                   "RetryReliability is not found in the datastore");
                   });
        }

        [Fact]
        public async Task SendRetry_IsDisabled_When_MSH_Is_In_Intermediary_Mode()
        {
            await TestComponentWithSettings(
                   "outboundprocessingagent_settings_sqlserver.xml",
                   async (settings, msh) =>
                   {
                       // Arrange
                       var userMessage = new UserMessage($"user-{Guid.NewGuid()}");
                       var sendingPMode = new SendingProcessingMode
                       {
                           Id = "outboundprocessing-reliability",
                           Reliability =
                           {
                                ReceptionAwareness =
                                {
                                    IsEnabled = true,
                                    RetryCount = 3,
                                    RetryInterval = "00:00:05"
                                }
                           },
                           MessagePackaging =
                           {
                               IsMultiHop =  true
                           }
                       };

                       var as4Message = AS4Message.Create(userMessage, sendingPMode);

                       var mshConfig = TestConfig.Create(settings);

                       var spy = DatabaseSpy.Create(mshConfig);

                       var toBeProcessed = new OutMessage(userMessage.MessageId)
                       {
                           ContentType = as4Message.ContentType,
                           EbmsMessageType = MessageType.UserMessage,
                           Intermediary = true,
                           Operation = Operation.ToBeProcessed,
                           MessageLocation =
                               Registry.Instance
                                       .MessageBodyStore
                                       .SaveAS4Message(msh.GetConfiguration().OutMessageStoreLocation, as4Message)
                       };
                       toBeProcessed.SetPModeInformation(sendingPMode);

                       // Act
                       spy.InsertOutMessage(toBeProcessed);

                       // Assert
                       Assert.True(toBeProcessed.Id != default(long));

                       await PollUntilSatisfied(
                             () => spy.GetOutMessageFor(m => m.EbmsMessageId == userMessage.MessageId)?.Operation == Operation.ToBeSent,
                             timeout: TimeSpan.FromSeconds(20));

                       Assert.True(spy.GetRetryReliabilityFor(r => r.RefToOutMessageId == toBeProcessed.Id) == null,
                                   "RetryReliability should not be present since the MSH is in a forwarding role");
                   });
        }
    }
}
