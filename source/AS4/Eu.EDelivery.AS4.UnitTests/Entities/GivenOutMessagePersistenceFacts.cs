using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    public class GivenOutMessagePersistenceFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task OutMessageOperationIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var outMessage = new OutMessage(Guid.NewGuid().ToString());
                outMessage.SetOperation(Operation.Sent);

                db.OutMessages.Add(outMessage);

                await db.SaveChangesAsync();

                savedInMessageId = outMessage.Id;

                Assert.NotEqual(default(long), savedInMessageId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var message = db.OutMessages.FirstOrDefault(i => i.Id == savedInMessageId);

                Assert.NotNull(message);
                Assert.Equal(Operation.Sent, OperationUtils.Parse(message.Operation));
            }
        }

        [Fact]
        public async Task OutMessageStatusIsCorrectlyPersisted()
        {
            long savedInMessageId;

            using (var db = this.GetDataStoreContext())
            {
                var msg = new OutMessage("ebmsMessageId");
                msg.SetStatus(OutStatus.Ack);

                db.OutMessages.Add(msg);

                await db.SaveChangesAsync();

                savedInMessageId = msg.Id;

                Assert.NotEqual(default(long), savedInMessageId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var msg = db.OutMessages.FirstOrDefault(i => i.Id == savedInMessageId);

                Assert.NotNull(msg);
                Assert.Equal(OutStatus.Ack, OutStatusUtils.Parse(msg.Status));
            }
        }
    }
}
