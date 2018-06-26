using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    public class GivenInMessagePersistenceFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task InMessageOperationIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = new InMessage(Guid.NewGuid().ToString());
                inMessage.SetOperation(Operation.Sent);

                db.InMessages.Add(inMessage);

                await db.SaveChangesAsync();

                savedInMessageId = inMessage.Id;

                Assert.NotEqual(default(long), savedInMessageId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = db.InMessages.FirstOrDefault(i => i.Id == savedInMessageId);

                Assert.NotNull(inMessage);
                Assert.Equal(Operation.Sent, inMessage.Operation);
            }
        }

        [Fact]
        public async Task InMessageMEPIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = new InMessage(Guid.NewGuid().ToString());
                inMessage.MEP = MessageExchangePattern.Pull;

                db.InMessages.Add(inMessage);

                await db.SaveChangesAsync();

                savedInMessageId = inMessage.Id;

                Assert.NotEqual(default(long), savedInMessageId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = db.InMessages.FirstOrDefault(i => i.Id == savedInMessageId);

                Assert.NotNull(inMessage);
                Assert.Equal(MessageExchangePattern.Pull, inMessage.MEP);
            }
        }

        [Fact]
        public async Task InMessageStatusIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = new InMessage(Guid.NewGuid().ToString());
                inMessage.SetStatus(InStatus.Notified);

                db.InMessages.Add(inMessage);

                await db.SaveChangesAsync();

                savedInMessageId = inMessage.Id;

                Assert.NotEqual(default(long), savedInMessageId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = db.InMessages.FirstOrDefault(i => i.Id == savedInMessageId);

                Assert.NotNull(inMessage);
                Assert.Equal(InStatus.Notified, inMessage.Status.ToEnum<InStatus>());
            }
        }

        [Fact]
        public async Task InMessageMessageTypeIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = new InMessage(Guid.NewGuid().ToString());
                inMessage.SetEbmsMessageType(MessageType.Receipt);

                db.InMessages.Add(inMessage);

                await db.SaveChangesAsync();

                savedInMessageId = inMessage.Id;

                Assert.NotEqual(default(long), savedInMessageId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = db.InMessages.FirstOrDefault(i => i.Id == savedInMessageId);

                Assert.NotNull(inMessage);
                Assert.Equal(MessageType.Receipt, inMessage.EbmsMessageType);
            }
        }

        [Fact]
        public async Task PModeInformationIsCorrectlyPersisted()
        {
            long savedId;

            const string pmodeId = "pmodeId";
            const string pmodeContent = "<pmode><id>pmodeId</id></pmode>";

            using (var db = GetDataStoreContext())
            {
                var message = new InMessage("some-message-id");
                message.SetPModeInformation(pmodeId, pmodeContent);

                db.InMessages.Add(message);

                await db.SaveChangesAsync();

                savedId = message.Id;
            }

            using (var db = this.GetDataStoreContext())
            {
                var message = db.InMessages.FirstOrDefault(i => i.Id == savedId);

                Assert.NotNull(message);
                Assert.Equal(pmodeId, message.PModeId);
                Assert.Equal(pmodeContent, message.PMode);
            }
        }
    }
}
