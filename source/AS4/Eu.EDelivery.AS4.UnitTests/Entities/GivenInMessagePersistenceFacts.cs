using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
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
                var inMessage = new InMessage();
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
                Assert.Equal(Operation.Sent, OperationUtils.Parse(inMessage.Operation));
            }
        }

        [Fact]
        public async Task InMessageMEPIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = new InMessage();
                inMessage.SetMessageExchangePattern(MessageExchangePattern.Pull);

                db.InMessages.Add(inMessage);

                await db.SaveChangesAsync();

                savedInMessageId = inMessage.Id;

                Assert.NotEqual(default(long), savedInMessageId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = db.InMessages.FirstOrDefault(i => i.Id == savedInMessageId);

                Assert.NotNull(inMessage);
                Assert.Equal(MessageExchangePattern.Pull, MessageExchangePatternUtils.Parse(inMessage.MEP));
            }
        }

        [Fact]
        public async Task InMessageStatusIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = new InMessage();
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
                Assert.Equal(InStatus.Notified, InStatusUtils.Parse(inMessage.Status));
            }
        }

        [Fact]
        public async Task InMessageMessageTypeIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = new InMessage();
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
                Assert.Equal(MessageType.Receipt, MessageTypeUtils.Parse(inMessage.EbmsMessageType));
            }
        }
    }
}
