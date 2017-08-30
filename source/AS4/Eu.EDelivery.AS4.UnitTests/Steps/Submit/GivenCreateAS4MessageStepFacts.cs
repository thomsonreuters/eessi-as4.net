using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Xunit;


namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    public class GivenCreateAS4MessageStepFacts
    {

        [Fact]
        public async Task CanCreateMessageWithPModeWithoutToParty()
        {
            var sendingParty = CreatePModeParty("sender", "c2", "eu.edelivery.services");

            var pmode = CreateSendingPMode(fromParty: sendingParty, toParty: null);

            var receivingParty = CreateSubmitMessageParty("receiver", "", "c3");

            var submitMessage = CreateSubmitMessage(pmode, fromParty: null, toParty: receivingParty);

            var context = new MessagingContext(submitMessage);
            context.SendingPMode = pmode;

            var sut = new CreateAS4MessageStep();

            var result = await sut.ExecuteAsync(context, CancellationToken.None);

            Assert.True(result.Succeeded);

            var as4Message = result.MessagingContext.AS4Message;

            Assert.False(as4Message.IsEmpty);
            Assert.True(as4Message.IsUserMessage);
            Assert.Equal(receivingParty.Role, as4Message.PrimaryUserMessage.Receiver.Role);
            Assert.Equal(receivingParty.PartyIds.First().Id, as4Message.PrimaryUserMessage.Receiver.PartyIds.First().Id);
            Assert.Equal(receivingParty.PartyIds.First().Type, as4Message.PrimaryUserMessage.Receiver.PartyIds.First().Type);
        }

        [Fact]
        public async Task CanCreateMessageWithPModeWithoutFromParty()
        {
            var receivingParty = CreatePModeParty("receiver", "c3", "eu.edelivery.services");

            var pmode = CreateSendingPMode(fromParty: null, toParty: receivingParty);

            var fromParty = CreateSubmitMessageParty("sender", "", "c2");

            var submitMessage = CreateSubmitMessage(pmode, fromParty: fromParty, toParty: null);

            var context = new MessagingContext(submitMessage);
            context.SendingPMode = pmode;

            var sut = new CreateAS4MessageStep();

            var result = await sut.ExecuteAsync(context, CancellationToken.None);

            Assert.True(result.Succeeded);

            var as4Message = result.MessagingContext.AS4Message;

            Assert.False(as4Message.IsEmpty);
            Assert.True(as4Message.IsUserMessage);
            Assert.Equal(fromParty.Role, as4Message.PrimaryUserMessage.Sender.Role);
            Assert.Equal(fromParty.PartyIds.First().Id, as4Message.PrimaryUserMessage.Sender.PartyIds.First().Id);
            Assert.Equal(fromParty.PartyIds.First().Type, as4Message.PrimaryUserMessage.Sender.PartyIds.First().Type);
        }

        [Fact]
        public async Task MessageIsCreatedWithDefaultSender_IfNoneIsSpecified()
        {
            var receivingParty = CreatePModeParty("receiver", "c3", "eu.edelivery.services");

            var pmode = CreateSendingPMode(fromParty: null, toParty: receivingParty);

            var submitMessage = CreateSubmitMessage(pmode, fromParty: null, toParty: null);

            var context = new MessagingContext(submitMessage);
            context.SendingPMode = pmode;

            var sut = new CreateAS4MessageStep();

            var result = await sut.ExecuteAsync(context, CancellationToken.None);

            Assert.True(result.Succeeded);
            Assert.Equal(Constants.Namespaces.EbmsDefaultFrom, result.MessagingContext.AS4Message.PrimaryUserMessage.Sender.PartyIds.First().Id);
            Assert.Equal(Constants.Namespaces.EbmsDefaultRole, result.MessagingContext.AS4Message.PrimaryUserMessage.Sender.Role);
        }

        [Fact]
        public async Task MessageIsCreatedWithDefaultReceiver_IfNoneIsSpecified()
        {            
            var pmode = CreateSendingPMode(fromParty: null, toParty: null);

            var submitMessage = CreateSubmitMessage(pmode, fromParty: null, toParty: null);

            var context = new MessagingContext(submitMessage);
            context.SendingPMode = pmode;

            var sut = new CreateAS4MessageStep();

            var result = await sut.ExecuteAsync(context, CancellationToken.None);

            Assert.True(result.Succeeded);
            Assert.Equal(Constants.Namespaces.EbmsDefaultTo, result.MessagingContext.AS4Message.PrimaryUserMessage.Receiver.PartyIds.First().Id);
            Assert.Equal(Constants.Namespaces.EbmsDefaultRole, result.MessagingContext.AS4Message.PrimaryUserMessage.Receiver.Role);
        }

        public void FailToCreateMessageWhenNoToPartyAvailable() { }

        private static Party CreatePModeParty(string role, string id, string type)
        {
            return new Party(role, new PartyId() { Id = id, Type = type });
        }

        private static AS4.Model.Common.Party CreateSubmitMessageParty(string role, string type, string id)
        {
            return new AS4.Model.Common.Party() { Role = role, PartyIds = new[] { new AS4.Model.Common.PartyId(id, type), } };
        }

        private static SubmitMessage CreateSubmitMessage(SendingProcessingMode pmode, AS4.Model.Common.Party fromParty, AS4.Model.Common.Party toParty)
        {
            var msg = new SubmitMessage
            {
                PartyInfo = new AS4.Model.Common.PartyInfo()
                {
                    FromParty = fromParty,
                    ToParty = toParty
                }
            };

            msg.PMode = pmode;

            return msg;
        }

        private static SendingProcessingMode CreateSendingPMode(Party fromParty, Party toParty)
        {
            var pmode = ValidSendingPModeFactory.Create();

            pmode.MessagePackaging = new SendMessagePackaging()
            {
                PartyInfo = new PartyInfo()
                {
                    FromParty = fromParty,
                    ToParty = toParty
                }

            };

            return pmode;
        }
    }
}
