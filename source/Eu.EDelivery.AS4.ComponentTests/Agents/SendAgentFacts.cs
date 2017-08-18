using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class SendAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;

        public SendAgentFacts()
        {
            OverrideSettings("sendagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }

        [Theory]
        [InlineData(true, OutStatus.Sent, Operation.ToBeForwarded)]
        [InlineData(false, OutStatus.Ack, Operation.ToBeNotified)]
        public async Task CorrectHandlingOnSynchronouslyReceivedMultiHopReceipt(bool actAsIntermediaryMsh, OutStatus expectedOutStatus, Operation expectedSignalOperation)
        {
            const string messageId = "multihop-message-id";
            string sendToUrl = $"http://localhost:9997/msh/";

            var pmode = CreateMultihopPMode(sendToUrl);

            var as4Message = CreateMultiHopMessage(messageId, pmode);

            ManualResetEvent signal = new ManualResetEvent(false);

            ResponseHandler r = new ResponseHandler(CreateMultiHopReceiptFor(as4Message));

            StubHttpServer.StartServer(sendToUrl, r.WriteResponse, signal);

            PutMessageToSend(as4Message, pmode, actAsIntermediaryMsh);

            signal.WaitOne();

            // Wait a bit to ensure that the AS4.NET MSH has finished handling the received response.
            await Task.Delay(TimeSpan.FromSeconds(1));

            var sentMessage = _databaseSpy.GetOutMessageFor(m => m.EbmsMessageId == messageId);
            var receivedMessage = _databaseSpy.GetInMessageFor(m => m.EbmsRefToMessageId == messageId);

            Assert.NotNull(sentMessage);
            Assert.NotNull(receivedMessage);

            Assert.Equal(expectedOutStatus, sentMessage.Status);
            Assert.Equal(MessageType.Receipt, receivedMessage.EbmsMessageType);
            Assert.Equal(expectedSignalOperation, receivedMessage.Operation);
        }

        private void PutMessageToSend(AS4Message as4Message, SendingProcessingMode pmode, bool actAsIntermediaryMsh)
        {
            string fileName = @".\database\as4messages\out\sendagent_test.as4";

            string directory = Path.GetDirectoryName(fileName);
            if (!String.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                SerializerProvider.Default.Get(as4Message.ContentType).Serialize(as4Message, fs, CancellationToken.None);
            }

            _databaseSpy.InsertOutMessage(new OutMessage()
            {
                ContentType = as4Message.ContentType,
                EbmsMessageId = as4Message.GetPrimaryMessageId(),
                EbmsMessageType = MessageType.UserMessage,
                PMode = AS4XmlSerializer.ToString(pmode),
                MessageLocation = $"FILE:///{fileName}",
                MEP = MessageExchangePattern.Push,
                Intermediary = actAsIntermediaryMsh,
                Operation = Operation.ToBeSent
            });
        }

        private static AS4Message CreateMultiHopMessage(string messageId, SendingProcessingMode sendingPMode)
        {
            var simpleUserMessage = UserMessageFactory.Instance.Create(sendingPMode);
            simpleUserMessage.MessageId = messageId;

            return AS4Message.Create(simpleUserMessage, sendingPMode);
        }

        private static AS4Message CreateMultiHopReceiptFor(AS4Message message)
        {
            using (MessagingContext context = new MessagingContext(message, MessagingContextMode.Receive))
            {
                var createReceipt = new CreateAS4ReceiptStep();
                var result = createReceipt.ExecuteAsync(context, CancellationToken.None).Result;

                Assert.True(result.Succeeded, "Unable to create Receipt");
                Assert.True(result.MessagingContext.AS4Message.IsMultiHopMessage, "Receipt is not created as a multihop receipt");

                return result.MessagingContext.AS4Message;
            }
        }

        private static SendingProcessingMode CreateMultihopPMode(string sendToUrl)
        {
            var pmode = new SendingProcessingMode()
            {
                Id = "PMode-Id",
                PushConfiguration = new PushConfiguration()
                {
                    Protocol = new Protocol()
                    {
                        Url = sendToUrl
                    }
                },
                ReceiptHandling = new SendHandling()
                {
                    NotifyMessageProducer = true,
                    NotifyMethod = new Method { Type = "FILE", Parameters = new List<Parameter> { new Parameter() { Name = "Location", Value = "." } } }
                },
                MepBinding = MessageExchangePatternBinding.Push,
                MessagePackaging = new SendMessagePackaging
                {
                    IsMultiHop = true,
                    PartyInfo = new PartyInfo()
                    {
                        FromParty = new Party()
                        {
                            PartyIds = new List<PartyId> { new PartyId("org:eu:europa:as4:example:accesspoint:B") },
                            Role = "Sender"
                        },
                        ToParty = new Party()
                        {
                            PartyIds = new List<PartyId> { new PartyId("org:eu:europa:as4:example:accesspoint:A") },
                            Role = "Receiver"
                        }
                    },
                    CollaborationInfo = new CollaborationInfo()
                    {
                        Action = "Receive_Agent_Forwarding_Action",
                        Service = new Service()
                        {
                            Type = "eu:europa:services",
                            Value = "Receive_Agent_Forwarding_Service"
                        }
                    }
                }

            };

            return pmode;
        }

        private sealed class ResponseHandler
        {
            private readonly AS4Message _responseMessage;

            /// <summary>
            /// Initializes a new instance of the <see cref="ResponseHandler"/> class.
            /// </summary>
            public ResponseHandler(AS4Message responseMessage)
            {
                _responseMessage = responseMessage;
            }

            public void WriteResponse(HttpListenerResponse response)
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = _responseMessage.ContentType;

                SerializerProvider.Default.Get(_responseMessage.ContentType).Serialize(_responseMessage, response.OutputStream, CancellationToken.None);
            }
        }
    }
}
