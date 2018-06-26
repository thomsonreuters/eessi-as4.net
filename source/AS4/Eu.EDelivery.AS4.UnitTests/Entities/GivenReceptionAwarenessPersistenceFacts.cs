using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    public class GivenReceptionAwarenessPersistenceFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task ReceptionAwarenessStatusIsCorrectlyPersisted()
        {
            long id;

            using (var db = GetDataStoreContext())
            {
                var ra = new ReceptionAwareness(1, "id");
                ra.Status = ReceptionStatus.Busy;

                db.ReceptionAwareness.Add(ra);

                await db.SaveChangesAsync();

                id = ra.Id;
            }

            using (var db = GetDataStoreContext())
            {
                var ra = db.ReceptionAwareness.First(r => r.Id == id);

                Assert.Equal(ReceptionStatus.Busy, ra.Status);
            }
        }
    }
}
