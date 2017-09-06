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
    public class GivenExceptionEntityPersitenceFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task ExceptionOperationIsCorrectlyPersisted()
        {
            long savedId;

            using (var db = this.GetDataStoreContext())
            {
                var inException = new InException();
                inException.SetOperation(Operation.Sent);

                db.InExceptions.Add(inException);

                await db.SaveChangesAsync();

                savedId = inException.Id;

                Assert.NotEqual(default(long), savedId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = db.InExceptions.FirstOrDefault(i => i.Id == savedId);

                Assert.NotNull(inMessage);
                Assert.Equal(Operation.Sent, OperationUtils.Parse(inMessage.Operation));
            }
        }
    }
}
