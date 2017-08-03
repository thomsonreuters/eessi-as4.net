using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Monitor;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Eu.EDelivery.AS4.Fe.SubmitTool;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using HttpMultipartParser;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class SubmitMessageCreatorTests
    {
        protected readonly string Pmode = "8.1.1-pmode";
        protected SubmitMessageCreator SubmitMessageCreator;
        protected IPmodeService PmodeService;
        protected IOptions<SubmitToolOptions> Options;

        protected SubmitMessageCreatorTests Setup(IEnumerable<IPayloadHandler> payloadHandlers = null, IEnumerable<IMessageHandler> messageHandlers = null)
        {
            Options = Substitute.For<IOptions<SubmitToolOptions>>();
            Options.Value.Returns(new SubmitToolOptions
            {
                PayloadHttpAddress = "httpaddress",
                ToHttpAddress = "tohttpaddress"
            });

            PmodeService = Substitute.For<IPmodeService>();
            SubmitMessageCreator = new SubmitMessageCreator(PmodeService, payloadHandlers, messageHandlers, Options, Substitute.For<IClient>());

            PmodeService.GetSendingByName(Arg.Is(Pmode)).Returns(new SendingBasePmode()
            {
                Pmode = new SendingProcessingMode
                {
                    Id = "2143213",
                    MessagePackaging = new SendMessagePackaging
                    {
                        PartyInfo = new PartyInfo
                        {
                            FromParty = new Party
                            {
                                PartyIds = new List<PartyId> { new PartyId { Id = "fds" } }
                            },
                            ToParty = new Party
                            {
                                PartyIds = new List<PartyId> { new PartyId { Id = "fdsqfdsfd" } }
                            }
                        }
                    }
                }
            });

            return this;
        }

        public class CreateSubmitMessages : SubmitMessageCreatorTests
        {
            [Fact]
            public async Task Throws_Exception_When_Pmode_Doesnt_Exist()
            {
                var payload = new MessagePayload
                {
                    SendingPmode = "IDONTEXIST"
                };

                var error = await Assert.ThrowsAsync<BusinessException>(() => Setup().SubmitMessageCreator.CreateSubmitMessages(payload));
                Assert.True(error.Message.Contains("Could not find PMode"));
            }

            [Fact]
            public async Task Passed_Payloads_To_The_Correct_Payload_Handler()
            {
                var payloadHandler = Substitute.For<IPayloadHandler>();
                payloadHandler.CanHandle(Arg.Any<string>()).Returns(true);
                payloadHandler.Handle(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>()).Returns(Task.FromResult("DOWNLOADURL"));

                var dummyMessageHandler = Substitute.For<IMessageHandler>();
                dummyMessageHandler.CanHandle(Arg.Any<string>()).Returns(true);
                await dummyMessageHandler.Handle(Arg.Any<SubmitMessage>(), Arg.Any<string>());

                Setup(new[] { payloadHandler }, new[] { dummyMessageHandler });

                using (var memoryStream = new MemoryStream())
                {
                    var payload = new MessagePayload
                    {
                        Files = new List<FilePart>
                        {
                            new FilePart("test", "test", memoryStream)
                        },
                        SendingPmode = Pmode
                    };

                    await SubmitMessageCreator.CreateSubmitMessages(payload);

                    payloadHandler.Received().CanHandle(Arg.Any<string>());
                    await payloadHandler.Received().Handle(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>());
                }
            }

            [Fact]
            public async Task Passed_Message_To_The_Correct_MessageHandler()
            {
                var dummyMessageHandler = Substitute.For<IMessageHandler>();
                dummyMessageHandler.CanHandle(Arg.Any<string>()).Returns(true);
                await dummyMessageHandler.Handle(Arg.Any<SubmitMessage>(), Arg.Any<string>());

                Setup(Enumerable.Empty<IPayloadHandler>(), new[] { dummyMessageHandler });

                var payload = new MessagePayload
                {
                    Files = Enumerable.Empty<FilePart>().ToList(),
                    SendingPmode = Pmode
                };

                await SubmitMessageCreator.CreateSubmitMessages(payload);

                dummyMessageHandler.Received().CanHandle(Arg.Any<string>());
                await dummyMessageHandler.Received().Handle(Arg.Any<SubmitMessage>(), Arg.Any<string>());
            }
        }
    }
}